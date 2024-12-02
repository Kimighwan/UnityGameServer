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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(50, 26950);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }


    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }
}
