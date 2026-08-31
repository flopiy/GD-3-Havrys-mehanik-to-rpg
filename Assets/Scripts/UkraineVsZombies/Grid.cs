using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UkraineVsZombies
{
    public class Grid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _width = 9;
        [SerializeField] private int _height = 5;
        [SerializeField] private Vector2 _cellSize = new Vector2(1.63f, 1.94f);
        [SerializeField] private Vector2 _offset = new Vector2(-6.48f, -4.69f);

        [Header("Tower")]
        [SerializeField] private GameObject _towerPrefab;
        [SerializeField] private int _towerCost = 50;
        [SerializeField] private float _spawnCooldown = 2f;

        [Header("Cooldown UI")]
        [SerializeField] private Image _cooldownFill;
        [SerializeField] private TextMeshProUGUI _costText;

        private Camera _camera;
        private float _cooldownTimer;
        private readonly HashSet<Vector2Int> _occupiedCells = new();

        public bool CanSpawn => _cooldownTimer <= 0f && (GameManager.Instance == null || GameManager.Instance.Coins >= _towerCost);

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            if (_costText != null)
            {
                _costText.text = $"{_towerCost}";
            }
        }

        private void Update()
        {
            UpdateCooldown();
            HandleInput();
        }

        private void UpdateCooldown()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                UpdateCooldownUI();
            }
        }

        private void HandleInput()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
            if (!Input.GetMouseButtonDown(0)) return;

            // Check if user clicked on UI element
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2Int cell = GetCell();
            if (IsValidCell(cell))
            {
                if (GameManager.Instance != null && !GameManager.Instance.TrySpendCoins(_towerCost))
                {
                    // Not enough coins
                    return;
                }

                if (_cooldownTimer > 0f)
                {
                    return;
                }

                PlaceTower(cell);
            }
        }

        private void PlaceTower(Vector2Int cell)
        {
            if (_towerPrefab == null) return;

            Vector3 pos = GetWorldPosition(cell);
            var obj = Instantiate(_towerPrefab, pos, Quaternion.identity);
            var tower = obj.GetComponent<Tower>();

            if (tower != null && GameManager.Instance != null)
                GameManager.Instance.RegisterTower(tower, cell.y);

            _occupiedCells.Add(cell);
            _cooldownTimer = _spawnCooldown;
            UpdateCooldownUI();
        }

        private Vector2Int GetCell()
        {
            if (_camera == null) _camera = Camera.main;
            Vector3 mouse = _camera != null ? _camera.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
            int x = Mathf.FloorToInt((mouse.x - _offset.x) / _cellSize.x);
            int y = Mathf.FloorToInt((mouse.y - _offset.y) / _cellSize.y);
            return new Vector2Int(x, y);
        }

        private Vector3 GetWorldPosition(Vector2Int cell)
        {
            float x = cell.x * _cellSize.x + _offset.x + _cellSize.x / 2f;
            float y = cell.y * _cellSize.y + _offset.y + _cellSize.y / 2f;
            return new Vector3(x, y, 0f);
        }

        private bool IsValidCell(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= _width) return false;
            if (cell.y < 0 || cell.y >= _height) return false;
            return !_occupiedCells.Contains(cell);
        }

        private void UpdateCooldownUI()
        {
            if (_cooldownFill != null)
                _cooldownFill.fillAmount = Mathf.Clamp01(_cooldownTimer / _spawnCooldown);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            float gridWidth = _width * _cellSize.x;
            float gridHeight = _height * _cellSize.y;

            for (int x = 0; x <= _width; x++)
            {
                float xPos = x * _cellSize.x + _offset.x;
                Vector3 start = new Vector3(xPos, _offset.y, 0f);
                Vector3 end = new Vector3(xPos, _offset.y + gridHeight, 0f);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= _height; y++)
            {
                float yPos = y * _cellSize.y + _offset.y;
                Vector3 start = new Vector3(_offset.x, yPos, 0f);
                Vector3 end = new Vector3(_offset.x + gridWidth, yPos, 0f);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}

