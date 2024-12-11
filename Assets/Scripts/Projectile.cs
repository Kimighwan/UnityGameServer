using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 슈루탄 클래스

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();  // ID에 해당하는 슈류탄 컨테이너
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigid;
    public int throwByPlayer;
    public Vector3 initialForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;

    private void Start()            // 시작시 초기 설정
    {
        id = nextProjectileId;      // id 할당
        nextProjectileId++;
        projectiles.Add(id, this);  // 컨테이너에 추가

        ServerSend.SpawnProjectile(this, throwByPlayer);    // 슈루탄 생성 패킷 전송

        rigid.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime()); // 10초 타이머
    }

    private void FixedUpdate()  // 슈루탄 위치 프레임마다 계산하여 패킷 전송
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision) // 충돌시 처리되는 함수
    {
        Explode();
    }

    public void Initialize(Vector3 initialMoveDirect, float initialForceStrength, int _throwByPlayer) // 던지는 힘, 어떤 플레이어가 던졌는지 체크
    {
        initialForce = initialMoveDirect * initialForceStrength;
        throwByPlayer = _throwByPlayer;
    }

    private void Explode()  // 폭발 함수
    {
        ServerSend.ProjectileExploded(this);    // 폭발 패킷 전송

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        // transform.position 위치에서 explosionRadius(반지름)만큼 주위 객체 탐색하여 콜라이더 반환
        foreach (Collider col in colliders)
        {
            if(col.CompareTag("Player"))    // 탐색된 객체 중 플레이어라면...
                col.GetComponent<Player>().TakeDamage(explosionDamage); // 플레이어 피격
        }

        projectiles.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime() // 어디에 충돌하지 않아도 10초 뒤에 터지도록 설정
    {
        yield return new WaitForSeconds(10f);

        Explode();
    }
}
