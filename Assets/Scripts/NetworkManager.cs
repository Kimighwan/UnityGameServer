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
        QualitySettings.vSyncCount = 0;   // ���� ����ȭ ��Ȱ��ȭ
        Application.targetFrameRate = 60; // ���� ������ 60���� ����

        Server.Start(2, 33374); // ������ �ʴ� ��Ʈ ��ȣ�� ���� ����, �÷��̾� ���� 2��
                                // https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers ����
    }

    private void Update()
    {
        StartGameTime();
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

    public bool CheckPlayer()     // ������ �÷��̾� üũ
    {
        bool checkPlayer = false;

        if (Server.clients[1].player == null)
            checkPlayer = false;

        else if(Server.clients[1].player != null)
        {
            if (Server.clients[2].player != null)
                checkPlayer = true;
            else
                checkPlayer = false;
        }

        return checkPlayer;
    }

    public void StartGameTime()
    {
        if(CheckPlayer())
            StartCoroutine(StartTime());
    }

    private IEnumerator StartTime()
    {
        yield return new WaitForSeconds(3f); 
        // 3�� ī��Ʈ �ٿ�
        // Ŭ���̾�Ʈ������ 3.. 2.. 1.. ī��Ʈ UI ǥ��

        // 60�� Ÿ�̸� ����
        // Ŭ���̾�Ʈ �߰� ��ܿ� Ÿ�̸� ǥ��

        // 60�� Ÿ�̸� ����� ����� ����

        // ȭ�鿡 ���� UI ǥ��
        // Ŭ���̾�Ʈ������ ���� UI ǥ��

        // 3���� ���� ����
    }
}
