using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance; // 해당 스크립트 객체를 참조하는 전역 변수

    public GameObject playerPrefab;     // 플레이어 객체
    public GameObject projectilePrefab; // 슈루탄 객체

    public Transform[] randomSpawnPos;  // 리스폰 위치
    public Transform[] spawnPos;        // 초기 스폰 위치
    private int spawnOrder = 0;

    private void Awake() // 유니티 엔진의 생명 주기 함수 -> 게임 실행시 초기에 한 번 실행되는 함수
    {
        if (instance == null) // 아직 만들어지지 않았다면 현재 객체 할당
        {
            instance = this;
        }
        else if (instance != this) // 이미 존재한다면 현재 객체 삭제
        {
            Destroy(this); // 삭제
        }
    }

    private void Start() // 서버 시작 
    {
        QualitySettings.vSyncCount = 0;   // 수직 동기화 비활성화
        Application.targetFrameRate = 60; // 게임 프레임 60으로 고정

        Server.Start(2, 33374); // 사용되지 않는 포트 번호로 서버 열기, 플레이어 수는 2명
                                // https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers 참고
    }

    private void Update()
    {
        CheckPlayer();  // 플레이어 접속 수 체크
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }


    public Player InstantiatePlayer() // 플레이어 생성
    {
        return Instantiate(playerPrefab, spawnPos[spawnOrder++].position, Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Transform shootOrigin) // 슈루탄 생성
    {
        return Instantiate(projectilePrefab, shootOrigin.position + shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
    }

    public bool CheckPlayer()     // 접속한 플레이어 체크
    {
        bool checkPlayer = false;

        if (Server.clients[1].player == null)
            checkPlayer = false;

        else if(Server.clients[1].player != null)
        {
            if (Server.clients[2].player != null)
                checkPlayer = true;
            else
                checkPlayer = false;
        }

        ServerSend.PlayerCheck(checkPlayer);
        return checkPlayer;
    }

}
