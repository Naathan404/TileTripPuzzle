using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private List<Tile> _tileList = new List<Tile>();
    [SerializeField] private float _stackOffset = 0.05f;
    [SerializeField] private float _boardYOffset = 0.5f;
    
    [Header("References")]
    [SerializeField] private GameObject _tilePrefab;

    public static event Action OnBoardClear;

    private void OnEnable()
    {
        BarManager.OnTileMove += UpdateBoardVisual;
        BarManager.OnTileRemoved += CheckWinConditions;
    }

    private void OnDisable()
    {
        BarManager.OnTileMove -= UpdateBoardVisual;
        BarManager.OnTileRemoved -= CheckWinConditions;
    }

    public void InitBoard(List<TileData> datas, float spacingX, float spacingY)
    {
        transform.position = Vector2.zero + new Vector2(0, _boardYOffset);

        foreach (var tile in _tileList)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        _tileList.Clear();

        foreach (TileData data in datas)
        {

            GameObject tileObj = Instantiate(_tilePrefab, this.transform);
            Tile tileScript = tileObj.GetComponent<Tile>();
            tileScript.SetData(data);
            _tileList.Add(tileScript);
        }


        /// Center board
        float minY = Int32.MaxValue; float maxY = Int32.MinValue;
        float minX = Int32.MaxValue; float maxX = Int32.MinValue;
        foreach(var tile in _tileList)
        {
            minX = Mathf.Min(minX, tile.Data.X);
            maxX = Mathf.Max(maxX, tile.Data.X);
            minY = Mathf.Min(minY, tile.Data.Y);
            maxY = Mathf.Max(maxY, tile.Data.Y);
        }
        float boardCenterX = (minX + maxX) / 2f;
        float boardCenterY = (minY + maxY) / 2f;

        this.transform.position = new Vector3(this.transform.position.x - boardCenterX * spacingX, this.transform.position.y + boardCenterY * spacingY - 1f);

        Debug.Log($"Tọa độ tâm Board: X = {transform.position.x}, Y = {transform.position.y}");
        Debug.Log($"X: min = {minX}, max = {maxX}");
        Debug.Log($"Y: min = {minY}, max = {maxY}");

        foreach(var tile in _tileList)
        {
            float posX = this.transform.position.x + tile.Data.X * spacingX;
            float posY = this.transform.position.y - tile.Data.Y * spacingY + tile.Data.Z * _stackOffset;
            Vector3 finalPos = new Vector3(posX, posY, 0);
            tile.transform.position = finalPos;

            tile.name = $"Tile_{tile.Data.TileID}_Z{tile.Data.Z}";
            tile.SetStateBlocked(CheckTileBlocked(tile));
        }
    }

    public void ClearBoard()
    {
        List<Tile> tilesToDestroy = new List<Tile>(_tileList);
        _tileList.Clear();

        foreach(var tile in tilesToDestroy)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
    }

    #region Helpers
    public bool CheckTileBlocked(Tile tileToCheck)
    {
        /// Kiểm tra overlapping: |X_A - X_B| < Width |Y_A - Y_B| < Height
        foreach(var other in _tileList)
        {
            if(other.Data.Z > tileToCheck.Data.Z)
            {
                float disX = Mathf.Abs(other.Data.X - tileToCheck.Data.X);
                float disY = Mathf.Abs(other.Data.Y - tileToCheck.Data.Y);

                if (disX < tileToCheck.Data.Width && disY < tileToCheck.Data.Height)
                    return true;
            }
        }
        return false;
    }

    public void UpdateBoardVisual(Tile tile)
    {
        Tile temp = tile;
        _tileList.Remove(tile);

        Debug.Log("[BOARD] nhận được tín hiệu click 1 Tile");

        if(temp.Data.Z > 0)
        {
            foreach(var t in _tileList)
            {
                if(t.Data.Z >= temp.Data.Z) continue;

                t.SetStateBlocked(CheckTileBlocked(t));
            }
        }
    }

    private void CheckWinConditions()
    {
        Debug.Log($"[BOARD] Còn {_tileList.Count} thẻ");

        if(_tileList.Count == 0 && BarManager.Instance.IsEmpty)
        {
            Debug.Log("[BOARD] WIN GAMEEEEEEEEEEEEEE");
            OnBoardClear?.Invoke();
            //return true;
            return;
        }
        //return false;
    }

    /// <summary>
    /// Trôn board
    /// </summary>
    private void ShuffleBoard()
    {
        List<int> allIDs = _tileList.Select(t => t.Data.TileID).ToList();
        LevelManager.ShuffleList(allIDs);

        for (int i = 0; i < _tileList.Count; i++)
        {
            _tileList[i].Data.TileID = allIDs[i];
            _tileList[i].RefreshVisual(); 
        }
    }

    #endregion

    # region HINTS
    public void UseHintPowerUp()
    {
        List<Tile> matchingGroup = FindMatchingGroup();
        if (matchingGroup != null)
        {
            foreach (Tile tile in matchingGroup)
                tile.HighlightTile();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Match);
            return;
        }

        int attempt = 0;
        while (matchingGroup == null && attempt < 5)
        {
            ShuffleBoard();
            matchingGroup = FindMatchingGroup();
            attempt++;
        }

        if (matchingGroup == null)
        {
            Debug.Log("[BOARD] Không thể tìm được bộ 3 hợp lệ sau khi shuffle!");
            return;
        }

        foreach (Tile tile in matchingGroup)
            tile.HighlightTile();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Match);
    }

    /// <summary>
    /// Hàm tìm 3 ô giống nhau
    /// </summary>
    /// <returns></returns>
    private List<Tile> FindMatchingGroup()
    {
        List<Tile> tilesOnBar = BarManager.Instance.CurrentTilesOnBar; 
        Dictionary<int, int> barCount = new Dictionary<int, int>();
        foreach (Tile tile in tilesOnBar)
        {
            int id = tile.TileID;
            if (!barCount.ContainsKey(id)) barCount[id] = 0;
            barCount[id]++;
        }

        Dictionary<int, List<Tile>> boardGroups = new Dictionary<int, List<Tile>>();
        foreach (Tile tile in _tileList)
        {
            if (tile.Data.IsBlocked) continue; 

            int id = tile.TileID;
            if (!boardGroups.ContainsKey(id))
            {
                boardGroups[id] = new List<Tile>();
            }
            boardGroups[id].Add(tile);
        }

        foreach (var pair in boardGroups)
        {
            int id = pair.Key;
            List<Tile> tilesOnBoard = pair.Value;

            int countOnBar = barCount.ContainsKey(id) ? barCount[id] : 0;
            int countOnBoard = tilesOnBoard.Count;

            if (countOnBar + countOnBoard >= 3)
            {
                int needFromBoard = 3 - countOnBar;
                List<Tile> hintResult = new List<Tile>();
                for (int i = 0; i < needFromBoard; i++)
                {
                    hintResult.Add(tilesOnBoard[i]);
                }
                return hintResult;
            }
        }

        return null;
    }
    #endregion
}
