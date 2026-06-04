using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board _board;
    [SerializeField] private LevelGenerator _levelGenerator;

    [Header("Settings")]
    [SerializeField] private int _totalLevels = 10;

    private SaveData _saveData;
    private LevelData _levelData;
    public int CurrentLevelString => _saveData.CurrentLevel;

    
    private void Start()
    {
        _saveData = SaveSystem.Load();
        LoadCurrentLevel();
    }

    public void LoadCurrentLevel()
    {
        LoadLevel(_saveData.CurrentLevel);
    }

    public void LoadNextLevel()
    {
        int nextLevelId = _saveData.CurrentLevel + 1;
        if(nextLevelId > _totalLevels)
        {
            Debug.Log("[LEVEL MANAGER] Đã hoàn thành tất cả màn chơi");
            return;
        }

        _saveData.CurrentLevel = nextLevelId;
        SaveSystem.Save(_saveData);

        LoadLevel(nextLevelId);
    }

    public void ReplayLevel()
    {
        LoadLevel(_saveData.CurrentLevel);
    }

    /// <summary>
    /// Load level có ID = levelID
    /// </summary>
    /// <param name="levelID"></param>
    public void LoadLevel(int levelID)
    {
        TextAsset jsonTextAssets = Resources.Load<TextAsset>("Levels/Level_" + levelID);
        if (jsonTextAssets == null)
        {
            Debug.LogError($"[LEVEL MANAGER] Không tìm thấy file Level_{levelID} tronng Resource hệ thống");
            return;
        }

        _levelData = JsonUtility.FromJson<LevelData>(jsonTextAssets.text);
        if (_levelData == null)
        {
            Debug.LogError($"[LEVEL MANAGER] Parse JSON thất bại");
            return;
        }

        List<TileData> tileDatas = GenerateTilesData(_levelData);
        _board.InitBoard(tileDatas, _levelData.SpacingX, _levelData.SpacingY);
    }

    /// <summary>
    /// Tạo Danh sách các Tiles và kiểm tra có giải được hay không
    /// </summary>
    /// <param name="levelData"></param>
    /// <returns></returns>
    private List<TileData> GenerateTilesData(LevelData levelData)
    {
        List<TileData> result = ConvertDataToGrid(levelData);
        while(!CheckIfCanSolve(result))
        {
            result = ConvertDataToGrid(levelData);
        }
        return result;
    }

    /// <summary>
    /// Chuyeerm dữ liệu từ Level Data thành List<TileData> để Board có thể đọc và tạo Object
    /// </summary>
    /// <param name="levelData"></param>
    /// <returns></returns>
    private List<TileData> ConvertDataToGrid(LevelData levelData)
    {
        List<TileData> _unsignedTileDatas = new List<TileData>();

        foreach(LayerData layer in levelData.Layers)
        {
            foreach(TilePosition tilePos in layer.TilePositions)
            {
                TileData newTileData = new TileData(tilePos.X, tilePos.Y, layer.ZIndex, -1);
                _unsignedTileDatas.Add(newTileData);
            }
        }

        List<int> pool = new List<int>();
        int set = _levelData.TotalTiles / 3;
        for(int i = 0; i < set; i++)
        {
            pool.Add(_levelData.AvailableTileIDs[i % _levelData.AvailableTileIDs.Count]);
        }

        /// Mix _tileList bằng Fisher yates
        ShuffleList(pool);
        ShuffleList(_unsignedTileDatas);
       
        int mark = 0;
        for(int i = 0; i < set; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                _unsignedTileDatas[mark++].TileID = pool[i];
            }
        }
        return _unsignedTileDatas;
    }

    /// <summary>
    /// Kiểm tra 1 list Tile Data có thể giải được hay không
    /// </summary>
    /// <param name="tileDatas"></param>
    /// <returns></returns>
    private bool CheckIfCanSolve(List<TileData> tileDatas)
    {
        // List<TileData> dummy = new List<TileData>(tileDatas);
        List<TileData> dummy = tileDatas.Select(t => t.Clone()).ToList();   // deep copy để tránh ảnh hưởng đến tile data gốc
        Dictionary<int, int> unblockedTiles = new Dictionary<int, int>();
        while(dummy.Count > 0)
        {
            unblockedTiles.Clear();
            foreach(var tile in dummy) tile.IsBlocked = IsTileBlocked(tile, dummy);

            foreach(var tile in dummy)
            {
                if (!tile.IsBlocked)
                {
                    if(unblockedTiles.ContainsKey(tile.TileID))
                        unblockedTiles[tile.TileID]++;
                        
                    else
                        unblockedTiles.Add(tile.TileID, 1);
                }
            }

            int matchId = -1;
            foreach(var pair in unblockedTiles)
            {
                if (pair.Value >= 3) 
                {
                    matchId = pair.Key;
                    break;
                }
            }

            if(matchId == -1) return false;

            int removed = 0;
            for (int i = dummy.Count - 1; i >= 0 && removed < 3; i--)
            {
                if (dummy[i].TileID == matchId && !dummy[i].IsBlocked)
                {
                    dummy.RemoveAt(i);
                    removed++;
                }
            }
        }
        return true;
    }

    private bool IsTileBlocked(TileData tileToCheck, List<TileData> tileList)
    {
        foreach (var other in tileList)
        {
            if (other == tileToCheck) continue;
            if (other.Z > tileToCheck.Z)
            {
                float disX = Mathf.Abs(other.X - tileToCheck.X);
                float disY = Mathf.Abs(other.Y - tileToCheck.Y);
                if (disX < tileToCheck.Width && disY < tileToCheck.Height)
                    return true;
            }
        }
        return false;
    }

    public static void ShuffleList<T>(List<T> list)
    {
        for(int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}


