using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using Unity.VisualScripting;

public class Server
{
    public static int maxPlayer { get; private set; } // �÷��̾� �ִ� ���� ��
    public static int port { get; private set; } // ��Ʈ ����

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); // Ŭ���̾�Ʈ ID�� �ش��ϴ� Ŭ���̾�Ʈ�� ����
    public delegate void PacketHandler(int fromClient, Packet packet);             // Ŭ���̾�Ʈ ID�� �ش��ϴ� ��Ŷ�� ����
    public static Dictionary<int, PacketHandler> packetHandlers;                   // Ŭ���̾�Ʈ ID�� �ش��ϴ� PacketHandler�� ����

    private static TcpListener tcpListener; // tcp ��Ʈ��ũ���� Ŭ���̾�Ʈ�� ������ �����ϴ� Ŭ����
    private static UdpClient udpListener; // udp ��Ʈ��ũ���� Ŭ���̾�Ʈ�� ������ �����ϴ� Ŭ����

    public static void Start(int _maxPlayer, int _port) // ������ �ʿ��� ��� ������ ����
    {
        maxPlayer = _maxPlayer;
        port = _port;

        Debug.Log("���� ���� �غ�...");
        InitServerData();

        tcpListener = new TcpListener(IPAddress.Any, port); // ������ port�� ���� ��� Ŭ���̾�Ʈ(��� ip)�� ���
        tcpListener.Start(); // ���� ��û ���� ���� = ���ε� ó��
        tcpListener.BeginAcceptTcpClient(TcpConnectCallBack, null); // ������ ������ �޾Ƶ��̴� �񵿱� �۾��� ����
                                                                    // �񵿱�� �ϱ� ������ ���� ������ ���ÿ� �����ϴ�
                                                                    // ù ��° �Ű������� �۾��� �Ϸ�Ǹ� ȣ��ȴ�.

        udpListener = new UdpClient(port); // ������ port�� ����
        udpListener.BeginReceive(UDPReceiveCallback, null); // ����


        Debug.Log($"��Ʈ ��ȣ : {port}�� ���� ����!!!");
    }


    private static void TcpConnectCallBack(IAsyncResult _result) // �Ű������� IAsyncResult�� �񵿱� �۾��� ����
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(_result); // �񵿱������� ������ �ް� ����� ���� ���ο� TcpClient�� ����
        tcpListener.BeginAcceptTcpClient(TcpConnectCallBack, null); // �� �ٽ� ������ �޾Ƶ��̴� �۾��� �Ͽ� ����ؼ� client�� �޴´�
        Debug.Log($"{client.Client.RemoteEndPoint}���� ���� ���� �õ� ��...");

        for (int i = 1; i <= maxPlayer; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client); // ������ ���� ����� TCP Ŭ���̾�Ʈ�� �����Ͽ� Ŭ���̾�Ʈ ���Ͽ� �Ҵ�
                return;
            }

            Debug.Log($"{client.Client.RemoteEndPoint} ������ ��á���ϴ�..."); 
            // ���� ���������� ������ ���� ���ϸ� ���� Ŭ���̾�Ʈ ������ ���� ���� �Ұ���
        }
    }

    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0); // Ư�� ip �ּҿ� ��Ʈ�� ���� IPEndPoint ����
            byte[] data = udpListener.EndReceive(_result, ref clientEndPoint); // ������ �����͸� data�� �����ϸ� �ٽ� ����Ʈ �迭�� �����ϰ�
                                                                               // clientEndPoint�� �����͸� ������ IPEndPoint �����Ѵ�
            udpListener.BeginReceive(UDPReceiveCallback, null); // ��� ����

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt(); // Ŭ���̾�Ʈ ID �б�

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null) // ũ���̾�Ʈ ��Ʈ�� ���� �� ��Ŷ�� �ǹ�
                {
                    clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                // Ŭ���̾�Ʈ�� ����� IPEndPoint�� ��Ŷ�� IPEndPoint�͸� ��ġ�ϴ��� Ȯ��
                if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())  
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"upd ������ ���� ���� / {e}");
        } 
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"udp�� {clientEndPoint}���� ������ ������ ���� / {e}");
        }
    }

    private static void InitServerData()
    {
        for (int i = 1; i <= maxPlayer; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                {(int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
                {(int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem },
            };
        Debug.Log("Init Packet");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
