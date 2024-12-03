using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        int clientChk = packet.ReadInt();
        string userName = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected sucessfully and is now Player {fromClient}.");
        if (fromClient != clientChk)
        {
            Debug.Log($"Player \"{userName}\" (ID: {fromClient}) has wrong client ID ({clientChk})");
        }
        Server.clients[fromClient].SendIntToGame(userName);
    }

    public static void PlayerMovement(int fromClient, Packet packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        Server.clients[fromClient].player.SetInput(inputs, rotation);
    }

    public static void PlayerShoot(int fromClient, Packet packet)
    {
        Vector3 shootDirection = packet.ReadVector3();

        Server.clients[fromClient].player.Shoot(shootDirection);
    }

    public static void PlayerThrowItem(int fromClient, Packet packet)
    {
        Vector3 throwDirection = packet.ReadVector3();

        Server.clients[fromClient].player.ThrowItem(throwDirection);
    }
}