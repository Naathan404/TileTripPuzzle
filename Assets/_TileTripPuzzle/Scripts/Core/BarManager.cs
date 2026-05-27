using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Collections;

public class BarManager : MonoBehaviour
{
    [Header("Bar Settings")]
    [SerializeField] private int _maxBarCapacity = 7;
    [SerializeField] private List<Tile> _tileList = new List<Tile>();
    [SerializeField] private List<Transform> transforms = new List<Transform>();
    

    [Header("Animation Settings")]
    [SerializeField] private float _tileMoveDuration = 0.3f;
    [SerializeField] private float _disappearDuration = 0.3f;
    [SerializeField] private float _waitTime = 0.5f;

    private void OnEnable()
    {
        Tile.OnTileClicked += AddTileToBar;
    }
    private void OnDisable()
    {
        Tile.OnTileClicked -= AddTileToBar;
    }

    public async void AddTileToBar(Tile tile)
    {
        if (_tileList.Count >= _maxBarCapacity)
        {
            Debug.Log("Bar is at maximum capacity. Cannot add more tiles.");
            return;
        }

        int insertIndex = FindInsertIndex(tile);
        _tileList.Insert(insertIndex, tile);
        UpdateBarDisplay();
        Invoke("CheckForMatches", _waitTime);
    }

    private int FindInsertIndex(Tile tile)
    {
        int result = _tileList.Count; 
        for(int i = 0; i < _tileList.Count; i++)
        {
            if (tile.TileID == _tileList[i].TileID)
            {
                result = i;
                break;
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
            if(_tileList == null) continue;
            Tile currentTile = _tileList[i];
            Vector3 targetPosition = transforms[i].position;
            
            currentTile.transform.DOMove(targetPosition, _tileMoveDuration).SetEase(Ease.OutQuad);
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
                Debug.Log($"Match found at index: {i}");
                tileIDToRemove = _tileList[i].TileID;
                startingRemoveIndex = i;
                break; 
            }
        }

        if(tileIDToRemove == -1)
        {
            if(_tileList.Count > _maxBarCapacity)
            {
                Debug.Log("Game Over!");
                return;
            }
            return;
        }

        StartCoroutine(RemoveMatchedTiles(startingRemoveIndex, tileIDToRemove));
    }

    private IEnumerator RemoveMatchedTiles(int startingRemoveIndex, int idToRemove)
    {
        _tileList[startingRemoveIndex].transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack);
            yield return new WaitForSeconds(_disappearDuration);
        _tileList[startingRemoveIndex + 1].transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack);
            yield return new WaitForSeconds(_disappearDuration);
        _tileList[startingRemoveIndex + 2].transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack);
            yield return new WaitForSeconds(_disappearDuration);

        for(int i = _tileList.Count - 1; i >= 0; i--)
        {
            if(_tileList == null) continue;
            if(_tileList[i].TileID == idToRemove)
            {
                Tile tileToRemove = _tileList[i];
                _tileList.RemoveAt(i);
                Destroy(tileToRemove.gameObject);
            }
        }

        UpdateBarDisplay();
    }

}
