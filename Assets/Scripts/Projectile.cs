using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigid;
    public int throwByPlayer;
    public Vector3 initialForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, throwByPlayer);

        rigid.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialize(Vector3 initialMoveDirect, float initialForceStrength, int _throwByPlayer)
    {
        initialForce = initialMoveDirect * initialForceStrength;
        throwByPlayer = _throwByPlayer;
    }

    private IEnumerator ExplodeAfterTime() // 어디에 충돌하지 않아도 10초 뒤에 터지도록 설정
    {
        yield return new WaitForSeconds(10f);

        Explode();
    }

    private void Explode()
    {
        ServerSend.ProjectileExploded(this);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            if(col.CompareTag("Player"))
                col.GetComponent<Player>().TakeDamage(explosionDamage);
        }

        projectiles.Remove(id);
        Destroy(gameObject);
    }
}
