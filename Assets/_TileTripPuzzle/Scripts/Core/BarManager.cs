using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Collections;
using System;
using UnityEngine.XR;
using UnityEngine.Rendering;
using System.Linq;

public class BarManager : MonoBehaviour
{
    [Header("Bar Settings")]
    [SerializeField] private int _maxBarCapacity = 7;
    [SerializeField] private float _tileSpacing = 0.5f;
    [SerializeField] private List<Tile> _tileList = new List<Tile>();
    [SerializeField] private List<Transform> _slots = new List<Transform>();

    [Header("Cloud")]
    [SerializeField] private GameObject _hideTilePrefab;
    [SerializeField] private float _hideTileOffset;
    private GameObject[] _hideTiles = new GameObject[7];


    [Header("Animation Settings")]
    [SerializeField] private float _tileMoveDuration = 0.3f;
    
    public bool IsFull => _tileList.Count >= _maxBarCapacity;
    public bool IsEmpty => _tileList.Count == 0;
    private bool _isProcessing = false;
    public bool IsProcessing => _isProcessing;
    public int TileCount => _tileList.Count;

    public static event System.Action OnBarFull;
    public static event Action OnTileRemoved;
    public static event Action<Tile> OnTileMove;

    public List<Tile> CurrentTilesOnBar => _tileList;

    public static BarManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        Tile.OnTileClicked += AddTileToBar;
    }
    private void OnDisable()
    {
        Tile.OnTileClicked -= AddTileToBar;
    }

    private void Start()
    {
        GenerateSlots();
    }

    private void GenerateSlots()
    {
        float totalWidth = (_maxBarCapacity - 1) * _tileSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < _maxBarCapacity; i++)
        {
            Vector3 pos = new Vector3(startX + i * _tileSpacing, this.transform.position.y, 10);
            _slots[i].transform.position = pos;

            GameObject hide = Instantiate(_hideTilePrefab, pos + new Vector3(0, 0.05f), Quaternion.identity);
            _hideTiles[i] = hide;
        }
    }

    public void UpdateHideTiles(bool[] hideTiles)
    {
        for(int i = 0; i < _hideTiles.Length; i++)
        {
            _hideTiles[i].SetActive(hideTiles[i]);
        }
    }

    public void AddTileToBar(Tile tile)
    {
        if (_tileList.Count >= _maxBarCapacity)
        {
            DebugManager.Instance.Log("Bar is at maximum capacity. Cannot add more tiles.");
            return;
        }

        if (_isProcessing)
        {
            return;
        }

        int rnd = UnityEngine.Random.Range(1, 10);
        if(rnd > 5)
            tile.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.2f, vibrato: 1, elasticity: 0.5f);
        else
            tile.transform.DOPunchRotation(new Vector3(0, 0, -15f), 0.2f, vibrato: 1, elasticity: 0.5f);
        tile.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, vibrato: 1, elasticity: 0.5f)
                    .OnComplete(() =>
                    {
                        OnTileMove?.Invoke(tile);
                        HandleAddTile(tile);
                    });

    }

    private void HandleAddTile(Tile tile)
    {
        int insertIndex = FindInsertIndex(tile);
        _tileList.Insert(insertIndex, tile);
        UpdateBarDisplay();

        CancelInvoke(nameof(CheckForMatches));
        Invoke("CheckForMatches", _tileMoveDuration);
    }


    private int FindInsertIndex(Tile tile)
    {
        int result = _tileList.Count;
        for(int i = 0; i < _tileList.Count; i++)
        {
            if (tile.TileID == _tileList[i].TileID)
            {
                result = i + 1;
            }
        }
        return result;
    }

    /// <summary>
    /// Cập nhật hiển thị của thanh Trái cây dựa trên TileList, xử lý animation di chuyển các tile
    /// </summary>
    private void UpdateBarDisplay()
    {
        for(int i = 0; i < _tileList.Count; i++)
        {
            if(_tileList[i] == null) continue;
            Tile currentTile = _tileList[i];
            Vector3 targetPosition = _slots[i].position;
            
            currentTile.transform.DOKill();
            currentTile.transform.DOScale(1f, _tileMoveDuration); 
            currentTile.transform.DOMove(targetPosition, _tileMoveDuration).SetEase(Ease.OutQuad);
        }
    }

    public void ClearBar()
    {
        StopAllCoroutines();
        CancelInvoke(nameof(CheckForMatches));
        _isProcessing = false;
 
        List<Tile> tilesToDestroy = new List<Tile>(_tileList);
        _tileList.Clear();

        foreach(var tile in tilesToDestroy)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
    }

    private void CheckForMatches()
    {
        int tileIDToRemove = -1;
        int startingRemoveIndex = -1;
        for(int i = 0; i < _tileList.Count - 2; i++)
        {
            if(_tileList == null) continue;
            if(_tileList[i].TileID == _tileList[i + 1].TileID && _tileList[i].TileID == _tileList[i + 2].TileID)
            {
                DebugManager.Instance.Log($"[BAR MANAGER] Match found at index: {i}");
                tileIDToRemove = _tileList[i].TileID;
                startingRemoveIndex = i;
                break; 
            }
        }

        if(tileIDToRemove == -1)
        {
            if(_tileList.Count >= _maxBarCapacity)
            {
                DebugManager.Instance.Log("[BAR] Bar is full!");
                OnBarFull?.Invoke();
                return;
            }
            return;
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Match);
        RemoveMatchedTiles(startingRemoveIndex);
    }

    // private IEnumerator RemoveMatchedTiles(int startingRemoveIndex, int idToRemove)
    // {
    //     _isProcessing = true;

    //     _tileList[startingRemoveIndex].transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack);
    //         yield return null;
    //     _tileList[startingRemoveIndex + 1].transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack);
    //         yield return null;
    //     _tileList[startingRemoveIndex + 2].transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack);
    //         yield return new WaitForSeconds(_disappearDuration);

    //     for(int i = 0; i < 3; i++)
    //     {
    //         Tile tileToRemove = _tileList[startingRemoveIndex];
    //         _tileList.RemoveAt(startingRemoveIndex);
    //         Destroy(tileToRemove.gameObject);
    //     }
    //     UpdateBarDisplay();
    //     yield return new WaitForSeconds(_tileMoveDuration);

    //     _isProcessing = false;
    //     CheckForMatches();
    //     OnTileRemoved?.Invoke();
    // }

    private void RemoveMatchedTiles(int startIndex)
    {
        for(int i = 0; i < 3; i++)
        {
            Tile tile = _tileList[startIndex];
            _tileList.RemoveAt(startIndex);
            tile.PlayDisappearAnimation(); 
        }

        UpdateBarDisplay();
        CheckForMatches(); 
        OnTileRemoved?.Invoke();
    }
}