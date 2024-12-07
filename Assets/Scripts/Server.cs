using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using Unity.VisualScripting;

public class Server
{
    public static int maxPlayer { get; private set; } // 플레이어 최대 입장 수
    public static int port { get; private set; } // 포트 설정

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); // 클라이언트 ID에 해당하는 클라이언트을 저장
    public delegate void PacketHandler(int fromClient, Packet packet);             // 클라이언트 ID에 해당하는 패킷을 저장
    public static Dictionary<int, PacketHandler> packetHandlers;                   // 클라이언트 ID에 해당하는 PacketHandler를 저장

    private static TcpListener tcpListener; // tcp 네트워크에서 클라이언트의 연결을 수신하는 클래스
    private static UdpClient udpListener; // udp 네트워크에서 클라이언트의 연결을 수신하는 클래스

    public static void Start(int _maxPlayer, int _port) // 서버에 필요한 모든 설정을 수행
    {
        maxPlayer = _maxPlayer;
        port = _port;

        Debug.Log("서버 시작 준비...");
        InitServerData();

        tcpListener = new TcpListener(IPAddress.Any, port); // 지정된 port로 오는 모든 클라이언트(모든 ip)를 허용
        tcpListener.Start(); // 연결 요청 수신 시작 = 바인드 처리
        tcpListener.BeginAcceptTcpClient(TcpConnectCallBack, null); // 들어오는 연결을 받아들이는 비동기 작업을 시작
                                                                    // 비동기로 하기 때문에 여러 연결을 동시에 가능하다
                                                                    // 첫 번째 매개변수는 작업이 완료되면 호출된다.

        udpListener = new UdpClient(port); // 지정된 port로 연결
        udpListener.BeginReceive(UDPReceiveCallback, null); // 수신


        Debug.Log($"포트 번호 : {port}로 서버 시작!!!");
    }


    private static void TcpConnectCallBack(IAsyncResult _result) // 매개변수의 IAsyncResult는 비동기 작업의 상태
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(_result); // 비동기적으로 연결을 받고 통신을 위해 새로운 TcpClient를 생성
        tcpListener.BeginAcceptTcpClient(TcpConnectCallBack, null); // 또 다시 연결을 받아들이는 작업을 하여 계속해서 client를 받는다
        Debug.Log($"{client.Client.RemoteEndPoint}으로 부터 연결 시도 중...");

        for (int i = 1; i <= maxPlayer; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client); // 위에서 새로 연결된 TCP 클라이언트를 전달하여 클라이언트 소켓에 할당
                return;
            }

            Debug.Log($"{client.Client.RemoteEndPoint} 서버의 꽉찼습니다..."); 
            // 위에 루프문에서 연결을 하지 못하면 남는 클라이언트 소켓이 없어 접속 불가능
        }
    }

    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0); // 특정 ip 주소와 포트가 없는 IPEndPoint 생성
            byte[] data = udpListener.EndReceive(_result, ref clientEndPoint); // 수신한 데이터를 data에 저장하며 다시 바이트 배열로 저장하고
                                                                               // clientEndPoint을 데이터를 수신한 IPEndPoint 설정한다
            udpListener.BeginReceive(UDPReceiveCallback, null); // 계속 수신

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt(); // 클라이언트 ID 읽기

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null) // 크라이언트 포트를 여는 빈 패킷을 의미
                {
                    clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                // 클라이언트에 저장된 IPEndPoint와 패킷의 IPEndPoint와를 일치하는지 확인
                if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())  
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"upd 데이터 수신 오류 / {e}");
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
            Debug.Log($"udp로 {clientEndPoint}에게 데이터 보내기 오류 / {e}");
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
