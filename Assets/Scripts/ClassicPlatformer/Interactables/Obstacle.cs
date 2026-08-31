using UnityEngine;

namespace ClassicPlatformer
{
    public class Obstacle : BaseInteractable
    {
        [Header("Damage")]
        [SerializeField] private int _damage = 1;
        [SerializeField] private bool _instantKill = false;

        public override void Interact(Player player)
        {
            if (_instantKill)
            {
                player.TakeDamage(player.MaxHealth);
            }
            else
            {
                player.TakeDamage(_damage);
            }
            Debug.Log("Obstacle");
        }
    }
}
