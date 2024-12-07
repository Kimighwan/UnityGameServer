using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> spawners = new Dictionary<int, ItemSpawner>();
    private static int nextSpawnerId = 1;

    public int spawnerId;
    public bool hasItem = false;    // ������ �����ʰ� ���� �������� ��ȯ�ϰ� �ִ��� üũ
                                    // false : ���� ��ȯ �̻��� X
                                    // true : ���� ��ȯ��

    private void Start()
    {
        hasItem = false;
        spawnerId = nextSpawnerId;
        nextSpawnerId++;
        spawners.Add(spawnerId, this);

        StartCoroutine(SpawnItem());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hasItem && other.CompareTag("Player"))   // ���� �������� ��ȯ && �÷��̾ �����ϸ� �۵�
        {
            Player player = other.GetComponent<Player>();
            if (player.AttempPickUpItem())
            {
                ItemPickUp(player.id);
            }
        }
    }

    private IEnumerator SpawnItem() // ������ ����
    {
        yield return new WaitForSeconds(10f);

        hasItem = true;
        ServerSend.ItemSpawned(spawnerId);  // �������� ���������� Ŭ���̾�Ʈ���� �ش� ������ ����
    }

    private void ItemPickUp(int byPlayer) // �÷��̾ �������� ȹ����
    {
        hasItem = false;
        ServerSend.ItemPickedUp(spawnerId, byPlayer); // Ŭ���̾�Ʈ���� �������� �������� ���� ȹ��Ǿ��ٴ� ��� ����

        StartCoroutine(SpawnItem());
    }
}
