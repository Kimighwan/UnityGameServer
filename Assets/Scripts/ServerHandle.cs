using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 패킷에서 Byte인 데이터들을 적절한 데이터형으로 읽습니다.
// 읽은 데이터를 가지고 각 함수에서 하고자 하는 일을 처리합니다.

public class ServerHandle
{
    public static void Init(int fromClient, Packet packet) // 클라이언트 접속시 초기 설정값 수신
    {
        int clientChk = packet.ReadInt();
        string userName = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} 성공적으로 연결완료! / Player {fromClient}.");
        if (fromClient != clientChk)
        {
            Debug.Log($"Player \"{userName}\" (ID: {fromClient}) 클라이언트 ID가 일치하지 않습니다. ({clientChk})");
        }
        Server.clients[fromClient].SendIntToGame(userName);
    }

    public static void PlayerMovement(int fromClient, Packet packet)    // 플레이어 움직임 패킷 수신
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        Server.clients[fromClient].player.SetInput(inputs, rotation);
    }

    public static void PlayerShoot(int fromClient, Packet packet)   // 플레이어 총 쏘는 데이터 패킷 수신
    {
        Vector3 shootDirection = packet.ReadVector3();

        Server.clients[fromClient].player.Shoot(shootDirection);
    }

    public static void PlayerThrowItem(int fromClient, Packet packet)   // 플레이어 슈루탄 던짐 데이터 패킷 수신
    {
        Vector3 throwDirection = packet.ReadVector3();

        Server.clients[fromClient].player.ThrowItem(throwDirection);
    }
}