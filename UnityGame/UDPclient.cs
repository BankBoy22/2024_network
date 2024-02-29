using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UDP Ŭ���̾�Ʈ Ŭ����
public class UDPclient : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ConnectMessage;
    UdpClient client;
    IPEndPoint serverEndPoint;

    public int playerNumber;

    public string playerName;

    public GameObject endGamePanel;

    public TextMeshProUGUI Timer;

    public float timeLeft = 60f;

    //�� �÷��̾���� ī�޶�
    public GameObject player1Camera;
    public GameObject player2Camera;

    // �������� ���� ����
    bool isConnected = false;

    // �÷��̾� ������ ��������
    public GameObject player1Prefab; 
    public GameObject player2Prefab;

    void Start()
    {
        PlayerEvents.PlayerPositionChanged += OnPlayerPositionChanged;
        PlayerEvents.PlayerAttacked += OnPlayerAttacked;
        PlayerEvents.PlayerDamaged += OnPlayerDamaged;
        PlayerEvents.PlayerFlipped += OnPlayerFlipped;
        PlayerEvents.PlayerDead += OnPlayerDead;

        playerName = PlayerPrefs.GetString("PlayerName");

        ConnectToServer();

        // ���� ���� Ȯ�� �ڷ�ƾ ����
        StartCoroutine(CheckServerConnection());
    }

    void Update()
    {
        // �����κ��� �޽����� �޽��ϴ�.
        ReceiveMessageFromServer();
    }

    // �����κ��� �޽����� �޽��ϴ�.
    void ReceiveMessageFromServer()
    {
        try
        {
            // �����κ��� �޽����� �޽��ϴ�. Ŭ���̾�Ʈ�� �����͸� ������ ������ ����մϴ�.
            while (client.Available > 0)
            {
                byte[] receiveData = client.Receive(ref serverEndPoint);
                string message = Encoding.ASCII.GetString(receiveData);
                // ���� �޽����� ���� ó���� �����մϴ�.
                ProcessMessage(message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message from server: " + e.Message);
        }
    }

    // ���� �޽����� ���� ������ ó���� �����մϴ�.
    void ProcessMessage(string message)
    {
        Debug.Log("Received message from server: " + message);
        // �޽����� ���ڸ� ������ UDP�� ���� �÷��̾� �ѹ��� �����մϴ�.
        if (int.TryParse(message, out int newPlayerNumber))
        {
            playerNumber = newPlayerNumber;
            Debug.Log("Player number updated: " + playerNumber);
            // �÷��̾� �ѹ��� ���� ī�޶� Ȱ��ȭ�մϴ�.
            if (playerNumber == 1)
            {
                player1Camera.SetActive(true);
                player2Camera.SetActive(false);
            }
            else
            {
                player1Camera.SetActive(false);
                player2Camera.SetActive(true);
            }
        }
        // StartGame �޽����� ������ Ÿ�̸Ӹ� �����մϴ�.
        if (message == "StartGame")
        {
            StartTimer();
        }
        // Position �޼����� ������ ����� ��ġ�� ������Ʈ�մϴ�.
        if (message.Contains("Position"))
        {
            // �޽����� �Ľ��Ͽ� ����� ��ġ�� ������Ʈ�մϴ�.
            string[] data = message.Split('|');
            // data[0]�� "Player1Position" �Ǵ� "Player2Position"�̴�. �׷��Ƿ� ����� ������ ������ �ޱ����� 7��° ���ڸ� ����
            int EnemyNumber = int.Parse(data[0][6].ToString()); // �÷��̾� �ѹ��� �Ľ��մϴ�.
            // �÷��̾� �ѹ��� �Ľ��մϴ�.
            string[] position = data[1].Split(',');
            Vector3 newPosition = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
            //�׽�Ʈ�� ���� ��ġ �����
            Debug.Log("Enemy" + EnemyNumber + " Position: " + newPosition);
            if (EnemyNumber == 1)
            {
                player1Prefab.transform.position = newPosition;
            }
            else
            {
                player2Prefab.transform.position = newPosition;
            }
        }
        if (message.Contains("Flipped"))
        {
            string[] data = message.Split('|');
            int EnemyNumber = int.Parse(data[0][6].ToString());
            bool isFlipped = data[1] == "1" ? true : false;
            if (EnemyNumber == 1)
            {
                player1Prefab.GetComponent<SpriteRenderer>().flipX = isFlipped;
            }
            else
            {
                player2Prefab.GetComponent<SpriteRenderer>().flipX = isFlipped;
            }
        }
        // EndGame �޽����� ������ ���� ���� �г��� Ȱ��ȭ�մϴ�.
        if (message == "EndGame")
        {
            ShowEndGamePanel();
        }
    }

    void ShowEndGamePanel()
    {
        // ���� ���� �г��� Ȱ��ȭ�մϴ�.
        endGamePanel.SetActive(true);
    }

    void ConnectToServer()
    {
        client = new UdpClient();
        // ������ IP �ּҿ� ��Ʈ ��ȣ�� �����մϴ�.
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
        isConnected = true;
        Debug.Log("Connected to server");
        ConnectMessage.text = "Connected to server";

        // ���� �޽����� �����ϴ�.
        string connectMessage = "PlyaerName : " + playerName + " Connect";
        SendMessageToServer(connectMessage);
    }

    // ������ ���� ���¸� Ȯ���ϴ� �ڷ�ƾ
    IEnumerator CheckServerConnection()
    {
        while (true)
        {
            // ������ ������ ���������� Ȯ��
            if (!isConnected)
            {
                Debug.LogWarning("Server connection lost. Stopping the game...");
                // ���� ���� ���� �߰�
                Time.timeScale = 0f; // ���� �Ͻ�����
            }
            yield return new WaitForSeconds(1f); // 1�ʸ��� üũ
        }
    }

    // �÷��̾ �¿�� �̵����� �� ȣ��ȴ�.
    public void OnPlayerFlipped(int PlayerID, bool isFlipped)
    {
        string message = "Player"+ PlayerID + "Flipped|" + (isFlipped ? "1" : "0");
        SendMessageToServer(message);
    }

    // �÷��̾ ���ظ� �Ծ��� �� ȣ��ȴ�.
    public void OnPlayerDamaged(int PlayerID, int damage)
    {
        string message = "Player"+ PlayerID + "Damaged|" + damage.ToString();
        SendMessageToServer(message);
    }

    // �÷��̾ �������� �� ȣ��ȴ�.
    public void OnPlayerAttacked(int PlayerID, int attack)
    {
        string message = "Player"+ PlayerID + "Attacked|" + attack.ToString();
        SendMessageToServer(message);
    }

    // �÷��̾��� ��ġ�� ����Ǿ��� �� ȣ��ȴ�.
    public void OnPlayerPositionChanged(int PlayerID,Vector3 newPosition)
    {
        string message = $"Player{PlayerID}Position|{newPosition.x},{newPosition.y},{newPosition.z}";
        SendMessageToServer(message);
    }

    // �÷��̾ �����⸦ ���� �� ȣ��ȴ�.
    public void OnPlayerRolling(int PlayerID, bool isRolling)
    {
        string message = "Player"+ PlayerID + "Rolling|" + (isRolling ? "1" : "0");
        SendMessageToServer(message);
    }

    // �÷��̾ �׾��� �� ȣ��ȴ�.
    public void OnPlayerDead(int PlayerID, bool isDead)
    {
        string message = "Player"+ PlayerID + "Dead|" + (isDead ? "1" : "0");
        SendMessageToServer(message);
    }

    // ������ �޽����� �����Ѵ�.
    void SendMessageToServer(string message)
    {
        byte[] sendData = Encoding.ASCII.GetBytes(message);
        client.Send(sendData, sendData.Length, serverEndPoint);
    }

    // Ŭ���̾�Ʈ ��ü�� �ݴ´�.
    void OnDestroy()
    {
        if (client != null)
        {
            client.Close();
        }
        PlayerEvents.PlayerPositionChanged -= OnPlayerPositionChanged;
        PlayerEvents.PlayerAttacked -= OnPlayerAttacked;
        PlayerEvents.PlayerDamaged -= OnPlayerDamaged;
        PlayerEvents.PlayerFlipped -= OnPlayerFlipped;
    }

    // ���� ���� �޽����� ������ Ÿ�̸Ӹ� �����Ѵ�.
    public void StartTimer()
    {
        StartCoroutine(GameTimer());
    }

    // ���� Ÿ�̸�
    IEnumerator GameTimer()
    {
        // Ÿ�̸� �ؽ�Ʈ�� ������Ʈ(�ʸ��� ����)
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            Timer.text = "Time Left: " + timeLeft.ToString("F1");
            yield return null; // 1������ ���
        }
        // Ÿ�̸Ӱ� 0�� �Ǹ� ���� ���� �޽����� ������ �����Ѵ�.
        Debug.Log("Game Over!");
        SendMessageToServer("EndGame");
    }
}