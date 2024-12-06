using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance; // �ش� ��ũ��Ʈ ��ü�� �����ϴ� ���� ����

    public GameObject playerPrefab; // �÷��̾� ��ü
    public GameObject projectilePrefab; // ����ź ��ü

    private void Awake() // ����Ƽ ������ ���� �ֱ� �Լ� -> ���� ����� �ʱ⿡ �� �� ����Ǵ� �Լ�
    {
        if (instance == null) // ���� ��������� �ʾҴٸ� ���� ��ü �Ҵ�
        {
            instance = this;
        }
        else if (instance != this) // �̹� �����Ѵٸ� ���� ��ü ����
        {
            Destroy(this); // ����
        }
    }

    private void Start() // ���� ���� 
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(2, 33374); // ������ �ʴ� ��Ʈ ��ȣ�� ���� ����, �÷��̾� ���� 2��
                                // https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers ����
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }


    public Player InstantiatePlayer() // �÷��̾� ����
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Transform shootOrigin)
    {
        return Instantiate(projectilePrefab, shootOrigin.position + shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
    }
}
