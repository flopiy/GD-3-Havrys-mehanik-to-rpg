using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace UkraineVsZombies
{
    public class GameManager : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private SpawnPoint[] _spawnPoints;

        [Header("Spawn Settings")]
        [SerializeField] private float _minSpawnTime = 2.5f;
        [SerializeField] private float _maxSpawnTime = 4.5f;
        [SerializeField] private int _maxEnemies = 30;

        [Header("Lanes")]
        [SerializeField] private int _laneCount = 5;

        [Header("Base / Player Stats")]
        [SerializeField] private float _maxBaseHealth = 100f;
        [SerializeField] private int _startingCoins = 100;

        [Header("UI - HUD")]
        [SerializeField] private Slider _baseHpSlider;
        [SerializeField] private TextMeshProUGUI _baseHpText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _killsText;

        [Header("UI - Game Over Panel")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;
        [SerializeField] private TextMeshProUGUI _gameOverKillsText;
        [SerializeField] private Button _restartButton;

        private readonly Dictionary<int, List<Enemy>> _enemiesByLane = new();
        private readonly Dictionary<int, List<Tower>> _towersByLane = new();

        private float _currentBaseHealth;
        private int _score;
        private int _coins;
        private int _kills;
        private float _spawnTimer;
        private bool _isGameOver;

        public static GameManager Instance { get; private set; }

        public bool IsGameOver => _isGameOver;
        public int Coins => _coins;
        public float BaseHealth => _currentBaseHealth;

        private void Awake()
        {
            Instance = this;

            for (int i = 0; i < _laneCount; i++)
            {
                _enemiesByLane[i] = new List<Enemy>();
                _towersByLane[i] = new List<Tower>();
            }

            _currentBaseHealth = _maxBaseHealth;
            _coins = _startingCoins;
            _score = 0;
            _kills = 0;
        }

        private void Start()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);

            if (_restartButton != null)
                _restartButton.onClick.AddListener(RestartGame);

            _spawnTimer = Random.Range(_minSpawnTime, _maxSpawnTime);
            UpdateHUD();
        }

        private void Update()
        {
            if (_isGameOver)
            {
                // Quick restart shortcut key
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
                return;
            }

            // Interactive OverlapPoint mechanic for clicking pickups / coins in world
            HandleWorldClicks();

            UpdateSpawning();
            CleanupLists();
        }

        private void HandleWorldClicks()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var camera = Camera.main;
                if (camera != null)
                {
                    Vector2 mouseWorld = camera.ScreenToWorldPoint(Input.mousePosition);
                    // Check if clicked on a collectible item (using OverlapPointAll to ensure enemies don't block clicks)
                    Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);
                    foreach (var hit in hits)
                    {
                        var item = hit.GetComponent<CollectibleItem>();
                        if (item != null)
                        {
                            item.Collect();
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateSpawning()
        {
            int totalEnemies = 0;
            foreach (var list in _enemiesByLane.Values)
                totalEnemies += list.Count;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && totalEnemies < _maxEnemies)
            {
                SpawnEnemy();
                _spawnTimer = Random.Range(_minSpawnTime, _maxSpawnTime);
            }
        }

        private void SpawnEnemy()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0) return;

            int index = Random.Range(0, _spawnPoints.Length);
            var spawnPoint = _spawnPoints[index];

            if (spawnPoint == null) return;

            Enemy enemy = spawnPoint.Spawn();
            if (enemy != null)
                RegisterEnemy(enemy, index);
        }

        public void RegisterEnemy(Enemy enemy, int lane)
        {
            if (lane < 0 || lane >= _laneCount) return;
            _enemiesByLane[lane].Add(enemy);
        }

        public void RegisterTower(Tower tower, int lane)
        {
            if (lane < 0 || lane >= _laneCount) return;
            _towersByLane[lane].Add(tower);
        }

        public void DamageBase(float damage)
        {
            if (_isGameOver) return;

            _currentBaseHealth = Mathf.Max(0f, _currentBaseHealth - damage);
            UpdateHUD();

            if (_currentBaseHealth <= 0f)
            {
                GameOver();
            }
        }

        public void AddScore(int points)
        {
            if (_isGameOver) return;
            _score += points;
            UpdateHUD();
        }

        public void AddCoins(int amount)
        {
            if (_isGameOver) return;
            _coins += amount;
            UpdateHUD();
        }

        public bool TrySpendCoins(int cost)
        {
            if (_coins >= cost)
            {
                _coins -= cost;
                UpdateHUD();
                return true;
            }
            return false;
        }

        public void RegisterEnemyKilled(Enemy enemy)
        {
            if (_isGameOver) return;
            _kills++;
            AddScore(100);
            AddCoins(25);
            UpdateHUD();
        }

        private void CleanupLists()
        {
            foreach (var list in _enemiesByLane.Values)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null)
                        list.RemoveAt(i);
                }
            }

            foreach (var list in _towersByLane.Values)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null)
                        list.RemoveAt(i);
                }
            }
        }

        private void UpdateHUD()
        {
            if (_baseHpSlider != null)
            {
                _baseHpSlider.value = _currentBaseHealth / _maxBaseHealth;
            }

            if (_baseHpText != null)
            {
                _baseHpText.text = $"База HP: {Mathf.CeilToInt(_currentBaseHealth)} / {_maxBaseHealth}";
            }

            if (_scoreText != null)
            {
                _scoreText.text = $"Рахунок: {_score}";
            }

            if (_coinsText != null)
            {
                _coinsText.text = $"Монети: {_coins}";
            }

            if (_killsText != null)
            {
                _killsText.text = $"Знищено: {_kills}";
            }
        }

        public void GameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;

            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
            }

            if (_gameOverScoreText != null)
            {
                _gameOverScoreText.text = $"Фінальний рахунок: {_score}";
            }

            if (_gameOverKillsText != null)
            {
                _gameOverKillsText.text = $"Знищено зомбі: {_kills}";
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
    }
}

