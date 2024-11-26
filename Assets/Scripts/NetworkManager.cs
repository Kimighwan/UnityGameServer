using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    private void Awake() // 초기화 생명 주기 함수
    {
        if (instance == null) // 아직 만들어지지 않았다면 현재 객체 할당
        {
            instance = this;
        }
        else if (instance != this) // 이미 존재한다면 현재 객체 삭제
        {
            Destroy(this);
        }
    }

    private void Start() // 서버 시작 
    {
        #if UNITY_EDITOR    // 유니티 에디터에서 실행을 했을 때
        Debug.Log("Build the project to start the server!!!");
        #else               // 빌드된 프로젝트(게임 서버)에서 실행을 했을 떄
        Server.Start(50, 26950);
        #endif
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
