using UnityEngine;

// 패킷을 전송하는 메소드 함수들
// 패킷을 생성하는 메소드 함수들

// 위 두개의 작업을 정의한 클래스

public class ServerSend
{
    // 아래의 Send 메서드들은 패킷을 전송

    // Server.clients[toClient].tcp.SendData(packet);
    // 해당 코드를 공부해서 설명이 필요하다

    // 중요한 정보는 TCP를 이용하고
    // 반대로 중요하지 않거나 빠르게 통신이 필요한 경우는 UDP를 사용
    // 또 지속적으로 데이터를 전송하는 경우 데이터를 잃어도 계속 전송하기에 UDP를 사용

    private static void SendTCPData(int toClient, Packet packet) // TCP로 패킷 전달
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet); // 패킷 전달
    }

    private static void SendUDPData(int toClient, Packet packet) // UDP로 패킷 전달
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet) // TCP로 모든 클라이언트에게 패킷 전달
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.maxPlayer; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAll(int exceptClinet, Packet packet) // TCP로 모든 클라이언트에게  패킷 전달
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

    private static void SendUDPDataToAll(Packet packet) // UDP로 모든 클라이언트에게  패킷 전달
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.maxPlayer; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAll(int exceptClinet, Packet packet) // UDP로 모든 클라이언트에게  패킷 전달
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

    #region Packet // 패킷에 필요한 데이터를 입력 // 위의 적절한 전송 메서드를 통해서 전송

    public static void Init(int toClient, string m) // 어떤 클라이언트에게 보내고 싶은 메세지를 담은 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.init))
        {
            packet.Write(m);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int toClient, Player player) // 플레이어 스폰 정보 패킷
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

    public static void PlayerPosition(Player player) // 플레이어 위치 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player) // 플레이어 회전 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int playerId) // 플레이어 연결 끊김 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerHP(Player player) // 플레이어 체력 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerHP))
        {
            packet.Write(player.id);
            packet.Write(player.hp);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerReSpawned(Player player) // 플레이어 리스폰 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerReSpawned))
        {
            packet.Write(player.id);

            SendTCPDataToAll(packet);
        }
    }

    public static void CreateItemSpawner(int toClient, int spawnerId, Vector3 spawnerPos, bool hasItem) // 아이템 생성기 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            packet.Write(spawnerId);
            packet.Write(spawnerPos);
            packet.Write(hasItem);

            SendTCPData(toClient, packet);
        }
    }

    public static void ItemSpawned(int spawnerId) // 아이템 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(spawnerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void ItemPickedUp(int spawnerId, int byPlayer) // 플레이어가 아이템을 획든한 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(spawnerId);
            packet.Write(byPlayer);

            SendTCPDataToAll(packet);
        }
    }

    public static void SpawnProjectile(Projectile projectile, int throwByPlayer) // 폭탄 생성 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);
            packet.Write(throwByPlayer);

            SendTCPDataToAll(packet);
        }
    }

    public static void ProjectilePosition(Projectile projectile) // 폭탄 위치 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.projectilePostion))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendTCPDataToAll(packet);
        }
    }

    public static void ProjectileExploded(Projectile projectile) // 폭탄 폭발 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.projectileExploded))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerCheck(bool check) // 플레이어 2명 접속 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerCheck))
        {
            packet.Write(check);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerDieCount(Player player) // 플레이어 죽은 횟수 정보 패킷
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDieCount))
        {
            packet.Write(player.id);

            SendTCPDataToAll(packet);
        }
    }    
    #endregion
}
