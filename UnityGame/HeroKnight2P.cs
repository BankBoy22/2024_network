using UnityEngine;
using System.Collections;

public class HeroKnight2P : MonoBehaviour
{
    public int PlayerNumber; // 1P: 1, 2P: 2

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;       // 피 효과를 사용하지 않는지 여부
    [SerializeField] GameObject m_slideDust;

    private Animator m_animator;                
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;

    private bool m_isWallSliding = false;           // 벽을 타고 있는지 여부
    private bool m_grounded = false;                // 지면에 닿아 있는지 여부
    private bool m_rolling = false;                 // 구르기 중인지 여부
    private bool m_isBlocking = false;              // 쉴드로 막고 있는지 여부
    private int m_facingDirection = 1;              // 플레이어가 바라보는 방향
    private int m_currentAttack = 0;                // 현재 공격 중인지 여부
    private float m_timeSinceAttack = 0.0f;         // 마지막 공격 후 경과 시간
    private float m_delayToIdle = 0.0f;             // 대기 상태로 전환하기까지의 시간
    private float m_rollDuration = 8.0f / 14.0f;    // 구르기 지속 시간
    private float m_rollCurrentTime;                // 현재 구르기 시간

    // 서버 객체
    private UDPclient m_udpClient;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();

        // 서버 객체를 가져온다.
        m_udpClient = GameObject.Find("NetworkManager").GetComponent<UDPclient>();

        // 서버 객체의 플레이어 번호는 플레이어 번호와 동일하다.
        PlayerNumber = m_udpClient.playerNumber;
    }

    void Update()
    {
        // 플레이어 넘버가 1이 아니면 조작을 막습니다.
        if (PlayerNumber != 2)
        {
            PlayerNumber = m_udpClient.playerNumber;
            return;
        }
            

        m_timeSinceAttack += Time.deltaTime;

        // 구르기 중이면 현재 시간을 증가시킨다.
        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // 구르기가 끝나면 구르기 상태를 해제한다.
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        // 지면에 닿아 있는지 여부를 갱신한다.
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // 지면에 닿아 있지 않으면 지면에 닿아 있지 않다고 갱신한다.
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // 플레이어의 입력을 받아 처리한다. (수평 입력)
        float inputX = Input.GetAxis("Horizontal");

        // 플레이어의 입력에 따라 플레이어의 방향을 바꾼다.
        if (!IsAttacking() && !m_rolling)
        {
            // 플레이어가 오른쪽 입력을 받았다면 플레이어의 방향을 오른쪽으로 바꾼다.
            if (inputX > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false; // 플레이어의 이미지를 좌우로 뒤집는다.
                m_facingDirection = 1; // 플레이어가 바라보는 방향을 오른쪽으로 설정한다.
                // 서버에게 플레이어의 방향이 바뀌었다고 알린다.
                m_udpClient.OnPlayerFlipped(PlayerNumber,false);
            }
            else if (inputX < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
                m_facingDirection = -1;
                // 서버에게 플레이어의 방향이 바뀌었다고 알린다.
                m_udpClient.OnPlayerFlipped(PlayerNumber,true);
            }

            // 플레이어의 입력에 따라 플레이어의 속도를 설정한다.
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        }

        // 플레이어의 입력에 따라 애니메이션을 설정한다.
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
            Attack();
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
            Roll();
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }

        Vector3 playerPosition = transform.position;
        m_udpClient.OnPlayerPositionChanged(PlayerNumber,playerPosition);
    }

    bool IsAttacking()
    {
        return m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"); // 현재 공격 중인지 여부
    }

    void Attack()
    {
        // 공격 중이 아니라면 공격을 시작한다.
        if (!IsAttacking())
        {
            // 공격 횟수를 증가시킨다.
            m_currentAttack++;

            // 공격 횟수가 3을 넘어가면 1로 초기화한다.
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f; // 마지막 공격 후 경과 시간을 초기화한다.

            m_body2d.velocity = new Vector2(0, m_body2d.velocity.y); // 플레이어의 속도를 초기화한다.
            // 공격 중이라는 것을 서버에게 알린다.
            m_udpClient.OnPlayerAttacked(PlayerNumber,m_currentAttack);
        }
    }

    // 구르기
    void Roll()
    {
        m_rolling = true;
        m_animator.SetTrigger("Roll");
        m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        // 구르기 중이라는 것을 서버에게 알린다.
        m_udpClient.OnPlayerRolling(PlayerNumber, true);
    }

    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}
