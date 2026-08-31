using System;
using UnityEngine;
using UnityEngine.UI;

namespace UkraineVsZombies
{
    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _maxHealth = 50f;
        [SerializeField] private float _moveSpeed = 1.5f;
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _attackRate = 1f;
        [SerializeField] private float _baseDamage = 20f;

        [Header("Drops")]
        [SerializeField] private GameObject _dropPrefab;
        [SerializeField] [Range(0f, 1f)] private float _dropChance = 0.5f;

        [Header("HP Bar")]
        [SerializeField] private Slider _hpSlider;

        private float _currentHealth;
        private float _attackTimer;
        private Tower _targetTower;

        public event Action OnDeath;
        public bool IsAlive => _currentHealth > 0;
        public float BaseDamage => _baseDamage;

        public void Initialize()
        {
            _currentHealth = _maxHealth;
            UpdateHPBar();
        }

        private void Awake()
        {
            _currentHealth = _maxHealth;
            UpdateHPBar();
        }

        private void Update()
        {
            if (!IsAlive) return;

            if (_targetTower != null && _targetTower.IsAlive)
            {
                Attack();
            }
            else
            {
                _targetTower = null;
                Move();
            }
        }

        private void Move()
        {
            transform.position += Vector3.left * _moveSpeed * Time.deltaTime;

            // Fallback safety check if offscreen without trigger
            if (transform.position.x < -12f)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DamageBase(_baseDamage);
                }
                Die(false);
            }
        }

        private void Attack()
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _targetTower.TakeDamage(_attackDamage);
                _attackTimer = 1f / _attackRate;
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            _currentHealth -= damage;
            UpdateHPBar();

            if (_currentHealth <= 0f)
            {
                Die(true);
            }
        }

        private void Die(bool giveRewards)
        {
            if (giveRewards && GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemyKilled(this);

                if (_dropPrefab != null && UnityEngine.Random.value <= _dropChance)
                {
                    Instantiate(_dropPrefab, transform.position, Quaternion.identity);
                }
            }

            OnDeath?.Invoke();
            Destroy(gameObject);
        }

        private void UpdateHPBar()
        {
            if (_hpSlider != null)
                _hpSlider.value = _currentHealth / _maxHealth;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var tower = other.GetComponent<Tower>();
            if (tower != null && tower.IsAlive)
            {
                _targetTower = tower;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_targetTower == null || !_targetTower.IsAlive)
            {
                var tower = other.GetComponent<Tower>();
                if (tower != null && tower.IsAlive)
                {
                    _targetTower = tower;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var tower = other.GetComponent<Tower>();
            if (tower != null && tower == _targetTower)
            {
                _targetTower = null;
            }
        }
    }
}

