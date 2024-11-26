using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    private void Awake() // �ʱ�ȭ ���� �ֱ� �Լ�
    {
        if (instance == null) // ���� ��������� �ʾҴٸ� ���� ��ü �Ҵ�
        {
            instance = this;
        }
        else if (instance != this) // �̹� �����Ѵٸ� ���� ��ü ����
        {
            Destroy(this);
        }
    }

    private void Start() // ���� ���� 
    {
        #if UNITY_EDITOR    // ����Ƽ �����Ϳ��� ������ ���� ��
        Debug.Log("Build the project to start the server!!!");
        #else               // ����� ������Ʈ(���� ����)���� ������ ���� ��
        Server.Start(50, 26950);
        #endif
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
