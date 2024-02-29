using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//���� �������� ������ �̱��� ����
public class PlayerHP_Manager : MonoBehaviour
{
    public int PlayerNumber; // 1P: 1, 2P: 2    

    //���� ü��
    public int m_myHP = 100;
    //���� �ִ� ü��
    public int m_maxHP = 100;
    //����� ü��
    public int m_enemyHP = 100;
    //����� �ִ� ü��
    public int m_enemyMaxHP = 100;

    //���� ü�¹�
    public Image m_HPBarImage;

    // ����� ü�¹�
    public Image m_enemyHPBarImage;

    //�ִϸ��̼�
    private Animator m_animator;

    [SerializeField] bool m_noBlood = false;  //�ǰݽ� �� ����Ʈ ��������

    //���� ��ü
    private UDPclient m_udpClient;

    private void Start()
    {
        //�ִϸ����� ������Ʈ�� �����´�.
        m_animator = GetComponent<Animator>();
        // ���� ��ü�� �����´�.
        m_udpClient = GameObject.Find("NetworkManager").GetComponent<UDPclient>();
    }

    private void Update()
    {
        //�����̽��ٸ� ������ ü���� 10 ��´�.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            m_myHP -= 10;
            //��ó���� �ִϸ��̼�
            m_animator.SetTrigger("Hurt");
            //�÷��̾ �ǰݴ��ߴٰ� �˸���.
            m_udpClient.OnPlayerDamaged(PlayerNumber,10);
        }

        //ü�¹��� ������ ����
        decimal hp = (decimal)m_myHP / (decimal)m_maxHP; //ü���� ����
        m_HPBarImage.fillAmount = (float)hp; //ü�¹��� ������ ����

        if (m_myHP <= 0)
        {
            //�׾��� ��
            //���� ����
            Debug.Log("���� ����");
            //������ �׾��ٰ� �˸���.
            m_udpClient.OnPlayerDead(PlayerNumber, true);
            // ���� �ִϸ��̼�
            m_animator.SetBool("noBlood", m_noBlood); 
            m_animator.SetTrigger("Death");
        }
    }
}
