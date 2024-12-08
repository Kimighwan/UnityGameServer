using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance; // 해당 스크립트 객체를 참조하는 전역 변수

    public GameObject playerPrefab; // 플레이어 객체
    public GameObject projectilePrefab; // 슈루탄 객체

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
        StartGameTime();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }


    public Player InstantiatePlayer() // 플레이어 생성
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Transform shootOrigin)
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

    public void StartGameTime()
    {
        if(CheckPlayer())
            StartCoroutine(StartTime());
    }

    private IEnumerator StartTime()
    {
        yield return new WaitForSeconds(3f);
        // 3초 카운트 다운
        // 클라이언트에서도 3.. 2.. 1.. 카운트 UI 표시

        yield return new WaitForSeconds(3f);
        // 60초 타이머 시작
        // 클라이언트 중간 상단에 타이머 표시

        // 60초 타이머 종료시 몇승패 집계

        // 화면에 승패 UI 표시
        // 클라이언트에서도 승패 UI 표시

        // 3초후 서버 종료
    }
}
