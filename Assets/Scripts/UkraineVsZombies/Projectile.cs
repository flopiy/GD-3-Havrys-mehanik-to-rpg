using UnityEngine;

namespace UkraineVsZombies
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 12f;
        [SerializeField] private float _lifetime = 4f;

        [Header("Explosion (Physics2D.OverlapCircleAll)")]
        [SerializeField] private bool _isExplosive = true;
        [SerializeField] private float _explosionRadius = 1.2f;
        [SerializeField] private LayerMask _enemyLayerMask = 1 << 8; // Layer 8 is Enemy
        [SerializeField] private GameObject _explosionEffectPrefab;

        private Enemy _target;
        private float _damage;
        private float _timer;
        private bool _hasHit;

        public void Initialize(Enemy target, float damage)
        {
            _target = target;
            _damage = damage;
            _timer = _lifetime;
            _hasHit = false;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 moveDir = Vector3.right;
            if (_target != null && _target.IsAlive)
            {
                Vector3 targetDir = (_target.transform.position - transform.position).normalized;
                if (targetDir != Vector3.zero)
                {
                    moveDir = targetDir;
                }
            }

            transform.position += moveDir * _speed * Time.deltaTime;

            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                _hasHit = true;

                if (_isExplosive)
                {
                    Explode();
                }
                else
                {
                    enemy.TakeDamage(_damage);
                }

                Destroy(gameObject);
            }
        }

        private void Explode()
        {
            // Mechanic: Physics2D.OverlapCircleAll detects all enemies within explosion radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _enemyLayerMask);

            foreach (var hit in hits)
            {
                var hitEnemy = hit.GetComponent<Enemy>();
                if (hitEnemy != null && hitEnemy.IsAlive)
                {
                    // Full or distance-attenuated damage
                    float dist = Vector2.Distance(transform.position, hitEnemy.transform.position);
                    float damageMultiplier = Mathf.Clamp01(1f - (dist / (_explosionRadius * 1.5f)));
                    hitEnemy.TakeDamage(_damage * Mathf.Max(0.5f, damageMultiplier));
                }
            }

            if (_explosionEffectPrefab != null)
            {
                Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_isExplosive)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, _explosionRadius);
            }
        }
    }
}

