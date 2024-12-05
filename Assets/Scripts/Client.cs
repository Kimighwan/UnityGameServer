using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class Client // 클라이언트 정보를 저장하는 클래스
{
    public int clientId; // 클라이언트 ID
    public TCP tcp; // tcp 소켓 클래스 아래에서 직접 생성
    public UDP udp; // udp 소켓 클래스 아래에서 직접 생성
    public static int bufferSize = 4096;
    public Player player;

    public Client(int _clientId)
    {
        clientId = _clientId;
        tcp = new TCP(clientId);
        udp = new UDP(clientId);
    }

    public class TCP
    {
        public TcpClient socket; // 연결을 통해 얻는 TCP 클라이언트 소켓

        private readonly int id;
        private NetworkStream stream;
        private Packet receiveData;
        private byte[] receiveBuffer; // 수신받을 데이터를 바이트 단위로 받는 버퍼

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket) // Server소켓을 통해 연결된 클라이언트를 매개변수로 받아 소켓에 할당
        {
            socket = _socket;
            socket.ReceiveBufferSize = bufferSize; // 수신 버퍼 크기 초기화
            socket.SendBufferSize = bufferSize;    // 송신버퍼 크기 초기화

            stream = socket.GetStream(); // 클라이언트로 부터 메세지를 받는다.

            receiveData = new Packet();

            receiveBuffer = new byte[bufferSize];

            stream.BeginRead(receiveBuffer, 0, bufferSize, ReceiveCallBack, null); // NetworkStream 메소드를 통해 읽는다
                                                                                   // 읽은 결과를 비동기 콜백 함수로 넘긴다.

            ServerSend.Welcome(id, "Welcom Server!!!");
        }


        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); // 스트림을 읽음
                }
            }
            catch (Exception m)
            {
                Debug.Log($"Error sending data to player {id} TCP : {m}");
            }
        }


        private void ReceiveCallBack(IAsyncResult _result)
        {
            try
            {
                int byteLength = stream.EndRead(_result); // 스트림에서 읽은 바이트 수를 저장한다
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength); // 스트림 내용을 복사한다.

                receiveData.Reset(HandleData(data));

                stream.BeginRead(receiveBuffer, 0, bufferSize, ReceiveCallBack, null);
            }
            catch (Exception m)
            {
                Debug.Log($"Error : {m}");
                Server.clients[id].Disconnect();
            }
        }


        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receiveData.SetBytes(data);

            if (receiveData.UnreadLength() >= 4)
            {
                packetLength = receiveData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while (packetLength > 0 && packetLength <= receiveData.UnreadLength())
            {
                byte[] packetBytes = receiveData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });

                packetLength = 0;
                if (receiveData.UnreadLength() >= 4)
                {
                    packetLength = receiveData.ReadInt();
                    if (packetLength <= 0)
                        return true;
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receiveData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        public void SendData(Packet packet)
        {
            Server.SendUDPData(endPoint, packet);
        }

        public void HandleData(Packet packet)
        {
            int packetLength = packet.ReadInt(); // 패킷 길이
            byte[] packetBytes = packet.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](id, packet); // ID에 맞는 적절한 패킷 처리 메서드 호출
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntToGame(string playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(clientId, playerName);

        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                if (client.clientId != clientId)
                {
                    ServerSend.SpawnPlayer(clientId, client.player);
                }
            }
        }

        foreach (Client client in Server.clients.Values)
        {
            if (client.player != null)
            {
                ServerSend.SpawnPlayer(client.clientId, player);
            }
        }

        foreach(ItemSpawner itemSpawner in ItemSpawner.spawners.Values)
        {
            ServerSend.CreateItemSpawner(clientId, itemSpawner.spawnerId, itemSpawner.transform.position, itemSpawner.hasItem);
        }
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnect.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        UnityEngine.Object.Destroy(player.gameObject);
        player = null;

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnected(clientId);
    }
}
