using System;
using UnityEngine;
using UnityEngine.UI;

namespace UkraineVsZombies
{
    public class Tower : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _range = 15f;
        [SerializeField] private float _fireRate = 1.2f;
        [SerializeField] private float _damage = 15f;

        [Header("Targeting (Physics2D.Raycast)")]
        [SerializeField] private LayerMask _enemyLayerMask = 1 << 8; // Layer 8 is Enemy
        [SerializeField] private Vector2 _rayOffset = new Vector2(0.5f, 0f);

        [Header("Projectile")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        [Header("HP Bar")]
        [SerializeField] private Slider _hpSlider;

        private float _currentHealth;
        private float _fireTimer;
        private Enemy _target;

        public bool IsAlive => _currentHealth > 0;
        public float Range => _range;

        private void Awake()
        {
            if (_firePoint == null)
                _firePoint = transform;

            _currentHealth = _maxHealth;
            UpdateHpBar();
        }

        private void Update()
        {
            if (!IsAlive) return;

            ScanForTargetWithRaycast();
            TryFire();
        }

        private void ScanForTargetWithRaycast()
        {
            Vector2 rayOrigin = (Vector2)transform.position + _rayOffset;

            // Using Physics2D.Raycast to detect enemies in front of the tower in this lane
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, _range, _enemyLayerMask);

            // Debug visual ray in Scene view
            Debug.DrawRay(rayOrigin, Vector2.right * _range, hit.collider != null ? Color.red : Color.green);

            if (hit.collider != null)
            {
                var enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    _target = enemy;
                    return;
                }
            }

            _target = null;
        }

        public void SetTarget(Enemy target)
        {
            _target = target;
        }

        private void TryFire()
        {
            _fireTimer -= Time.deltaTime;

            if (_target == null || !_target.IsAlive || _fireTimer > 0f) return;

            Fire();
            _fireTimer = 1f / _fireRate;
        }

        private void Fire()
        {
            if (_projectilePrefab != null)
            {
                var obj = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity);
                var projectile = obj.GetComponent<Projectile>();
                if (projectile != null)
                    projectile.Initialize(_target, _damage);
            }
            else if (_target != null)
            {
                _target.TakeDamage(_damage);
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            _currentHealth -= damage;
            UpdateHpBar();

            if (_currentHealth <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateHpBar()
        {
            if (_hpSlider != null)
                _hpSlider.value = _currentHealth / _maxHealth;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 origin = transform.position + (Vector3)_rayOffset;
            Gizmos.DrawLine(origin, origin + Vector3.right * _range);
        }
    }
}

