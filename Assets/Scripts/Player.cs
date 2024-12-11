using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 관련 데이터와 로직

public class Player : MonoBehaviour
{
    public int id;
    public string userName;

    public CharacterController controller; // 플레이어 이동
    public Transform shootOrigin; // 총알 나가는 위치

    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float throwForce = 600f; // 던지는 힘
    public float hp;
    public float maxHP = 100f;
    public int itemAmount = 0;  // 현재 아이템 갯수
    public int maxItemAmount = 3;   // 아이템 최대 갯수
    public int dieCount = 0;

    private bool[] inputs;
    private float yVelocity = 0; // 플레이어 수직 속도 저장

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }


    public void Initialize(int _id, string _userName)   // 플레이어 초기 설정
    {
        this.id = _id;
        userName = _userName;
        hp = maxHP;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if (hp <= 0f) return; // 여기에 시간 종료, 아직 상대방이 안들어 옴을 체크하는 변수 넣기

        Vector2 inputDirection = Vector2.zero;
        if (inputs[0])
            inputDirection.y += 1;
        if (inputs[1])
            inputDirection.y -= 1;
        if (inputs[2])
            inputDirection.x -= 1;
        if (inputs[3])
            inputDirection.x += 1;

        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)   // 플레이어 이동
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
                yVelocity = jumpSpeed;
        }
        yVelocity += gravity;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection); // 해당 Vector로 이동

        ServerSend.PlayerPosition(this);    // 플레이어 위치 정보 패킷 전송
        ServerSend.PlayerRotation(this);    // 플레이어 회전 정보 패킷 전송
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)  // 초기 설정
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
    
    public void Shoot(Vector3 viewDirection)    // 총알 발사
    {
        if (hp <= 0f) return;

        if (Physics.Raycast(shootOrigin.position, viewDirection, out RaycastHit hit, 25f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }

    public void ThrowItem(Vector3 viewDircret)  // 수류탄 사용
    {
        if (hp <= 0f)
            return;

        if(itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(viewDircret, throwForce, id);
        }
    }

    public void TakeDamage(float damage)    // 피격 함수
    {
        if (hp <= 0f)
            return;

        hp -= damage;

        if(hp <= 0f)
        {
            hp = 0f;
            dieCount++;
            controller.enabled = false; // 움직임 X
            transform.position = NetworkManager.instance.randomSpawnPos[Random.Range(0, 5)].position; // 리스폰 랜덤
            ServerSend.PlayerPosition(this);    // 위치 정보 패킷 전송
            ServerSend.PlayerDieCount(this);    // 죽은 횟수 패킷 전송
            StartCoroutine("ReSpawn");
        }

        ServerSend.PlayerHP(this);
    }

    private IEnumerator ReSpawn()   // 리스폰
    {
        yield return new WaitForSeconds(5f);

        hp = maxHP;
        controller.enabled = true;
        ServerSend.PlayerReSpawned(this);
    }

    public bool AttempPickUpItem()  // 플레이어가 아이템 줍기 시도
    {
        if(itemAmount >= maxItemAmount) // 이미 플레이어가 최대 아이템 갯수를 가지고 있어서 false를 반환
        {
            return false;   // 아이템 줍기 실패
        }

        itemAmount++;   // 가지고 있는 아이템 갯수 증가
        return true;    // 아이템 줍기 성공
    }

    public void SetDieCount(int count)  // 사망 횟수 체크
    {
        dieCount = count;
    }
}
