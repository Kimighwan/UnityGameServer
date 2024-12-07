using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> spawners = new Dictionary<int, ItemSpawner>();
    private static int nextSpawnerId = 1;

    public int spawnerId;
    public bool hasItem = false;    // 아이템 스포너가 현재 아이템을 소환하고 있는지 체크
                                    // false : 현재 소환 미상태 X
                                    // true : 현재 소환됨

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
        if(hasItem && other.CompareTag("Player"))   // 현재 아이템이 소환 && 플레이어가 접촉하면 작동
        {
            Player player = other.GetComponent<Player>();
            if (player.AttempPickUpItem())
            {
                ItemPickUp(player.id);
            }
        }
    }

    private IEnumerator SpawnItem() // 아이템 생성
    {
        yield return new WaitForSeconds(10f);

        hasItem = true;
        ServerSend.ItemSpawned(spawnerId);  // 아이템을 생성했으니 클라이언트에게 해당 정보를 전송
    }

    private void ItemPickUp(int byPlayer) // 플레이어가 아이템을 획득함
    {
        hasItem = false;
        ServerSend.ItemPickedUp(spawnerId, byPlayer); // 클라이언트에게 아이템이 누군가에 의해 획득되었다는 사실 전송

        StartCoroutine(SpawnItem());
    }
}
