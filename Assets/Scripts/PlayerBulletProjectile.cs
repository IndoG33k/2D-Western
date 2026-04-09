using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBulletProjectile : MonoBehaviour
{
    public static readonly HashSet<PlayerBulletProjectile> Active = new HashSet<PlayerBulletProjectile>();

    [SerializeField] private float normalSpeed = 18f;
    [SerializeField] [Range(0.05f, 1f)] private float deadeyeSpeedFactor = 0.25f;

    [SerializeField] private float maxLifetimeSeconds = 4f;
    [SerializeField] private bool destroyWhenHitAnotherBullet = true;

    [Header("Hit targets")]
    [SerializeField] private int hitDamage = 5;
    [SerializeField] private LayerMask damageableLayers;

    [Header("Visual")]
    [Tooltip("Degrees added if the sprite faces up (+Y) instead of right (+X). Typical: -90 when art points upward.")]
    [SerializeField] private float visualFacingAngleOffset;

    private Rigidbody2D _rb;
    private Vector2 _dir;
    private bool _slowUntilDeadeyeEnds;
    private DeadeyeController _deadeye;
    private bool _released;
    private float _despawnAtTime;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        Active.Add(this);
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

    public Rigidbody2D Rigidbody => _rb;

    public void Launch(Vector2 direction, bool spawnedDuringDeadeye, DeadeyeController deadeye)
    {
        _dir = direction.normalized;
        _slowUntilDeadeyeEnds = spawnedDuringDeadeye;
        _deadeye = deadeye;
        _despawnAtTime = Time.time + maxLifetimeSeconds;

        if (_slowUntilDeadeyeEnds && _deadeye != null)
            _deadeye.DeadeyeEnded += OnDeadeyeEnded;

        AlignRotationToDirection();
        ApplyVelocity();
    }

    private void Update()
    {
        if (Time.time >= _despawnAtTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyWhenHitAnotherBullet)
        {
            var otherBullet = collision.collider.GetComponentInParent<PlayerBulletProjectile>();
            if (otherBullet != null && otherBullet != this)
            {
                Destroy(otherBullet.gameObject);
                Destroy(gameObject);
                return;
            }
        }

        var col = collision.collider;
        var enemyProj = col.GetComponentInParent<EnemyProjectile>();
        if (enemyProj != null)
        {
            GameAudioManager.Instance?.PlayBulletClink();
            Destroy(enemyProj.gameObject);
            Destroy(gameObject);
            return;
        }

        if (((1 << col.gameObject.layer) & damageableLayers) != 0)
        {
            var h = col.GetComponentInParent<Health>();
            if (h != null && h.IsAlive)
                h.TakeDamage(hitDamage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void OnDeadeyeEnded()
    {
        _released = true;
        if (_deadeye != null)
            _deadeye.DeadeyeEnded -= OnDeadeyeEnded;
        ApplyVelocity();
    }

    private void FixedUpdate()
    {
        if (_released || !_slowUntilDeadeyeEnds)
            return;

        if (_deadeye == null || !_deadeye.IsActive)
        {
            _released = true;
            ApplyVelocity();
        }
        else
            ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        float speed = normalSpeed;
        if (_slowUntilDeadeyeEnds && !_released)
            speed *= deadeyeSpeedFactor;

        _rb.linearVelocity = _dir * speed;
    }

    private void AlignRotationToDirection()
    {
        float z = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg + visualFacingAngleOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }

    private void OnDestroy()
    {
        Active.Remove(this);
        if (_deadeye != null)
            _deadeye.DeadeyeEnded -= OnDeadeyeEnded;
    }
}
