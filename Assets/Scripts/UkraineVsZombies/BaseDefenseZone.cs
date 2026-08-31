using UnityEngine;

namespace UkraineVsZombies
{
    [RequireComponent(typeof(Collider2D))]
    public class BaseDefenseZone : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float _defaultZombieDamage = 20f;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                float damage = enemy.BaseDamage > 0 ? enemy.BaseDamage : _defaultZombieDamage;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DamageBase(damage);
                }

                // Destroy or kill enemy when reaching the base line
                enemy.TakeDamage(99999f);
            }
        }
    }
}
