using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ServerSend
{
    private static void SendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.maxPlayer; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(int exceptClinet, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.maxPlayer; i++)
        {
            if (i != exceptClinet)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.maxPlayer; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAll(int exceptClinet, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.maxPlayer; i++)
        {
            if (i != exceptClinet)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    #region Packet

    public static void Welcome(int toClient, string m) // 매개변수 => 어떤 클라이언트, 메세지 
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(m);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.userName);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(toClient, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int playerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerHP(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerHP))
        {
            packet.Write(player.id);
            packet.Write(player.hp);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerReSpawned(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerReSpawned))
        {
            packet.Write(player.id);

            SendTCPDataToAll(packet);
        }
    }

    public static void CreateItemSpawner(int toClient, int spawnerId, Vector3 spawnerPos, bool hasItem)
    {
        using (Packet packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            packet.Write(spawnerId);
            packet.Write(spawnerPos);
            packet.Write(hasItem);

            SendTCPData(toClient, packet);
        }
    }

    public static void ItemSpawned(int spawnerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(spawnerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void ItemPickedUp(int spawnerId, int byPlayer)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(spawnerId);
            packet.Write(byPlayer);

            SendTCPDataToAll(packet);
        }
    }
    #endregion
}
