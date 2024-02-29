using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UDP 클라이언트 클래스
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

    //각 플레이어들의 카메라
    public GameObject player1Camera;
    public GameObject player2Camera;

    // 서버와의 연결 상태
    bool isConnected = false;

    // 플레이어 프리팹 가져오기
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

        // 서버 연결 확인 코루틴 시작
        StartCoroutine(CheckServerConnection());
    }

    void Update()
    {
        // 서버로부터 메시지를 받습니다.
        ReceiveMessageFromServer();
    }

    // 서버로부터 메시지를 받습니다.
    void ReceiveMessageFromServer()
    {
        try
        {
            // 서버로부터 메시지를 받습니다. 클라이언트가 데이터를 수신할 때까지 대기합니다.
            while (client.Available > 0)
            {
                byte[] receiveData = client.Receive(ref serverEndPoint);
                string message = Encoding.ASCII.GetString(receiveData);
                // 받은 메시지에 따른 처리를 수행합니다.
                ProcessMessage(message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message from server: " + e.Message);
        }
    }

    // 받은 메시지에 따라 적절한 처리를 수행합니다.
    void ProcessMessage(string message)
    {
        Debug.Log("Received message from server: " + message);
        // 메시지가 숫자만 있으면 UDP로 받은 플레이어 넘버를 갱신합니다.
        if (int.TryParse(message, out int newPlayerNumber))
        {
            playerNumber = newPlayerNumber;
            Debug.Log("Player number updated: " + playerNumber);
            // 플레이어 넘버에 따라 카메라를 활성화합니다.
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
        // StartGame 메시지를 받으면 타이머를 시작합니다.
        if (message == "StartGame")
        {
            StartTimer();
        }
        // Position 메세지를 받으면 상대의 위치를 업데이트합니다.
        if (message.Contains("Position"))
        {
            // 메시지를 파싱하여 상대의 위치를 업데이트합니다.
            string[] data = message.Split('|');
            // data[0]은 "Player1Position" 또는 "Player2Position"이다. 그러므로 상대의 포지션 정보를 받기위해 7번째 문자를 추출
            int EnemyNumber = int.Parse(data[0][6].ToString()); // 플레이어 넘버를 파싱합니다.
            // 플레이어 넘버를 파싱합니다.
            string[] position = data[1].Split(',');
            Vector3 newPosition = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
            //테스트를 위해 위치 디버그
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
        // EndGame 메시지를 받으면 게임 종료 패널을 활성화합니다.
        if (message == "EndGame")
        {
            ShowEndGamePanel();
        }
    }

    void ShowEndGamePanel()
    {
        // 게임 종료 패널을 활성화합니다.
        endGamePanel.SetActive(true);
    }

    void ConnectToServer()
    {
        client = new UdpClient();
        // 서버의 IP 주소와 포트 번호를 설정합니다.
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
        isConnected = true;
        Debug.Log("Connected to server");
        ConnectMessage.text = "Connected to server";

        // 연결 메시지를 보냅니다.
        string connectMessage = "PlyaerName : " + playerName + " Connect";
        SendMessageToServer(connectMessage);
    }

    // 서버와 연결 상태를 확인하는 코루틴
    IEnumerator CheckServerConnection()
    {
        while (true)
        {
            // 서버와 연결이 끊어졌는지 확인
            if (!isConnected)
            {
                Debug.LogWarning("Server connection lost. Stopping the game...");
                // 게임 정지 로직 추가
                Time.timeScale = 0f; // 게임 일시정지
            }
            yield return new WaitForSeconds(1f); // 1초마다 체크
        }
    }

    // 플레이어가 좌우로 이동했을 때 호출된다.
    public void OnPlayerFlipped(int PlayerID, bool isFlipped)
    {
        string message = "Player"+ PlayerID + "Flipped|" + (isFlipped ? "1" : "0");
        SendMessageToServer(message);
    }

    // 플레이어가 피해를 입었을 때 호출된다.
    public void OnPlayerDamaged(int PlayerID, int damage)
    {
        string message = "Player"+ PlayerID + "Damaged|" + damage.ToString();
        SendMessageToServer(message);
    }

    // 플레이어가 공격했을 때 호출된다.
    public void OnPlayerAttacked(int PlayerID, int attack)
    {
        string message = "Player"+ PlayerID + "Attacked|" + attack.ToString();
        SendMessageToServer(message);
    }

    // 플레이어의 위치가 변경되었을 때 호출된다.
    public void OnPlayerPositionChanged(int PlayerID,Vector3 newPosition)
    {
        string message = $"Player{PlayerID}Position|{newPosition.x},{newPosition.y},{newPosition.z}";
        SendMessageToServer(message);
    }

    // 플레이어가 구르기를 했을 때 호출된다.
    public void OnPlayerRolling(int PlayerID, bool isRolling)
    {
        string message = "Player"+ PlayerID + "Rolling|" + (isRolling ? "1" : "0");
        SendMessageToServer(message);
    }

    // 플레이어가 죽었을 때 호출된다.
    public void OnPlayerDead(int PlayerID, bool isDead)
    {
        string message = "Player"+ PlayerID + "Dead|" + (isDead ? "1" : "0");
        SendMessageToServer(message);
    }

    // 서버에 메시지를 전송한다.
    void SendMessageToServer(string message)
    {
        byte[] sendData = Encoding.ASCII.GetBytes(message);
        client.Send(sendData, sendData.Length, serverEndPoint);
    }

    // 클라이언트 객체를 닫는다.
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

    // 게임 시작 메시지를 받으면 타이머를 시작한다.
    public void StartTimer()
    {
        StartCoroutine(GameTimer());
    }

    // 게임 타이머
    IEnumerator GameTimer()
    {
        // 타이머 텍스트를 업데이트(초마다 변경)
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            Timer.text = "Time Left: " + timeLeft.ToString("F1");
            yield return null; // 1프레임 대기
        }
        // 타이머가 0이 되면 게임 종료 메시지를 서버에 전송한다.
        Debug.Log("Game Over!");
        SendMessageToServer("EndGame");
    }
}