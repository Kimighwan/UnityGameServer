using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result : MonoBehaviour
{
    public static Result instance;
    public static Dictionary<int, int> result = new Dictionary<int, int>(); // 클라이언트 ID에 해당하는 죽은 횟수
    private int a = 0;
    private int b = 0;

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

    private void GameResult()
    {
        a = result[1]; // client 1번
        b = result[2]; // client 2번
        bool gameResult = a < b;    // a가 이기면 ture // b가 이기면 false
        ServerSend.GameResult(gameResult);
    }
}
