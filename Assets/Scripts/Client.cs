using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class Client // Ŭ���̾�Ʈ ������ �����ϴ� Ŭ����
{
    public int clientId; // Ŭ���̾�Ʈ ID
    public TCP tcp; // tcp ���� Ŭ���� �Ʒ����� ���� ����
    public UDP udp; // udp ���� Ŭ���� �Ʒ����� ���� ����
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
        public TcpClient socket; // ������ ���� ��� TCP Ŭ���̾�Ʈ ����

        private readonly int id;
        private NetworkStream stream;
        private Packet receiveData;
        private byte[] receiveBuffer; // ���Ź��� �����͸� ����Ʈ ������ �޴� ����

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket) // Server������ ���� ����� Ŭ���̾�Ʈ�� �Ű������� �޾� ���Ͽ� �Ҵ�
        {
            socket = _socket;
            socket.ReceiveBufferSize = bufferSize; // ���� ���� ũ�� �ʱ�ȭ
            socket.SendBufferSize = bufferSize;    // �۽Ź��� ũ�� �ʱ�ȭ

            stream = socket.GetStream(); // Ŭ���̾�Ʈ�� ���� �޼����� �޴´�.

            receiveData = new Packet();

            receiveBuffer = new byte[bufferSize];

            stream.BeginRead(receiveBuffer, 0, bufferSize, ReceiveCallBack, null); // NetworkStream �޼ҵ带 ���� �д´�
                                                                                   // ���� ����� �񵿱� �ݹ� �Լ��� �ѱ��.

            ServerSend.Welcome(id, "Welcom Server!!!");
        }


        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); // ��Ʈ���� ����
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
                int byteLength = stream.EndRead(_result); // ��Ʈ������ ���� ����Ʈ ���� �����Ѵ�
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength); // ��Ʈ�� ������ �����Ѵ�.

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
            int packetLength = packet.ReadInt(); // ��Ŷ ����
            byte[] packetBytes = packet.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](id, packet); // ID�� �´� ������ ��Ŷ ó�� �޼��� ȣ��
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
