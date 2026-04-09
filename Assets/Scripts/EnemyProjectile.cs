using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 16f;
    [SerializeField] private float maxLifetimeSeconds = 5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerDamageLayers;

    [Header("Visual")]
    [SerializeField] private float visualFacingAngleOffset;

    private Rigidbody2D _rb;
    private Vector2 _dir;
    private float _despawnAtTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void SetDamage(int value)
    {
        damage = Mathf.Max(0, value);
    }

    public void Launch(Vector2 direction)
    {
        _dir = direction.normalized;
        if (_dir.sqrMagnitude < 0.0001f)
            _dir = Vector2.right;
        _despawnAtTime = Time.time + maxLifetimeSeconds;
        AlignRotationToDirection();
        _rb.linearVelocity = _dir * speed;
    }

    private void AlignRotationToDirection()
    {
        float z = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg + visualFacingAngleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }

    private void Update()
    {
        if (Time.time >= _despawnAtTime)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;
        var playerBullet = other.GetComponentInParent<PlayerBulletProjectile>();
        if (playerBullet != null)
        {
            Destroy(playerBullet.gameObject);
            Destroy(gameObject);
            return;
        }
        if (((1 << other.gameObject.layer) & playerDamageLayers) == 0)
        {
            Destroy(gameObject);
            return;
        }

        var health = other.GetComponentInParent<Health>();
        if (health != null && health.IsAlive)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}
