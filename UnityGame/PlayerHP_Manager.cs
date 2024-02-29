using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//현재 서버와의 연동은 미구현 상태
public class PlayerHP_Manager : MonoBehaviour
{
    public int PlayerNumber; // 1P: 1, 2P: 2    

    //나의 체력
    public int m_myHP = 100;
    //나의 최대 체력
    public int m_maxHP = 100;
    //상대의 체력
    public int m_enemyHP = 100;
    //상대의 최대 체력
    public int m_enemyMaxHP = 100;

    //나의 체력바
    public Image m_HPBarImage;

    // 상대의 체력바
    public Image m_enemyHPBarImage;

    //애니메이션
    private Animator m_animator;

    [SerializeField] bool m_noBlood = false;  //피격시 피 이펙트 생성여부

    //서버 객체
    private UDPclient m_udpClient;

    private void Start()
    {
        //애니메이터 컴포넌트를 가져온다.
        m_animator = GetComponent<Animator>();
        // 서버 객체를 가져온다.
        m_udpClient = GameObject.Find("NetworkManager").GetComponent<UDPclient>();
    }

    private void Update()
    {
        //스페이스바를 누르면 체력을 10 깎는다.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            m_myHP -= 10;
            //상처입은 애니메이션
            m_animator.SetTrigger("Hurt");
            //플레이어가 피격당했다고 알린다.
            m_udpClient.OnPlayerDamaged(PlayerNumber,10);
        }

        //체력바의 비율을 변경
        decimal hp = (decimal)m_myHP / (decimal)m_maxHP; //체력의 비율
        m_HPBarImage.fillAmount = (float)hp; //체력바의 비율을 변경

        if (m_myHP <= 0)
        {
            //죽었을 때
            //게임 오버
            Debug.Log("게임 오버");
            //서버에 죽었다고 알린다.
            m_udpClient.OnPlayerDead(PlayerNumber, true);
            // 죽음 애니메이션
            m_animator.SetBool("noBlood", m_noBlood); 
            m_animator.SetTrigger("Death");
        }
    }
}
