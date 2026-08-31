using UnityEngine;

namespace UkraineVsZombies
{
    public class CollectibleItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private int _coinValue = 25;
        [SerializeField] private int _scoreValue = 50;
        [SerializeField] private float _lifetime = 10f;
        [SerializeField] private bool _enableBobbing = false;
        [SerializeField] private float _bobbingSpeed = 3f;
        [SerializeField] private float _bobbingHeight = 0f;

        private Vector3 _startPosition;
        private float _timer;
        private bool _isCollected;

        private void Start()
        {
            _startPosition = transform.position;
            _timer = _lifetime;
        }

        private void Update()
        {
            if (_isCollected) return;

            if (_enableBobbing && _bobbingHeight > 0f)
            {
                float newY = _startPosition.y + Mathf.Sin(Time.time * _bobbingSpeed) * _bobbingHeight;
                transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void Collect()
        {
            if (_isCollected) return;
            _isCollected = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(_coinValue);
                GameManager.Instance.AddScore(_scoreValue);
            }

            Destroy(gameObject);
        }

        private void OnMouseDown()
        {
            Collect();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Auto-collect if tower or collector enters
            if (other.GetComponent<Tower>() != null)
            {
                Collect();
            }
        }
    }
}
