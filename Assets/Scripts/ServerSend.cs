using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// 패킷을 전송하는 메소드
// 패킷을 생성하는 메소드

// 위 두개의 작업을 하는 클래스
public class ServerSend
{
    // 아래의 Send 메서드들은 패킷을 전송

    // Server.clients[toClient].tcp.SendData(packet);
    // 해당 코드를 공부해서 설명이 필요하다

    // 중요한 정보는 TCP를 이용하고
    // 반대로 중요하지 않거나 빠르게 통신이 필요한 경우는 UDP를 사용
    // 또 지속적으로 데이터를 전송하는 경우 데이터를 잃어도 계속 전송하기에 UDP를 사용

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
    // 패킷에 필요한 데이터를 입력
    // 위의 적절한 전송 메서드를 통해서 전송

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

    public static void ItemSpawned(int spawnerId)   // 아이템 데이터 패킷 생성
    {
        using (Packet packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(spawnerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void ItemPickedUp(int spawnerId, int byPlayer)    // 플레이어가 아이템을 획득했다는 데이터 패킷 생성
    {
        using (Packet packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(spawnerId);
            packet.Write(byPlayer);

            SendTCPDataToAll(packet);
        }
    }

    public static void SpawnProjectile(Projectile projectile, int throwByPlayer)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);
            packet.Write(throwByPlayer);

            SendTCPDataToAll(packet);
        }
    }

    public static void ProjectilePosition(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ServerPackets.projectilePostion))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendTCPDataToAll(packet);
        }
    }

    public static void ProjectileExploded(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ServerPackets.projectileExploded))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerCheck(bool check)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerCheck))
        {
            packet.Write(check);

            SendTCPDataToAll(packet);
        }
    }

    //public static void GameResult(bool result)
    //{
    //    using (Packet packet = new Packet((int)ServerPackets.playerCheck))
    //    {
    //        packet.Write(result);

    //        SendTCPDataToAll(packet);
    //    }
    //}

    public static void PlayerDieCount(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDieCount))
        {
            packet.Write(player.id);

            SendTCPDataToAll(packet);
        }
    }
    #endregion
}
