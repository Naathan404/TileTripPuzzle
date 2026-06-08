using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board _board;

    [Header("Backgrounds")]
    [SerializeField] private int _backgroundChangePeriod = 5;
    [SerializeField] private SpriteRenderer _currentBackground;
    [SerializeField] private List<Sprite> _backgrounds = new List<Sprite>();

    // private SaveData _saveData;
    private LevelData _levelData;
    public int CurrentLevelID => GameManager.Instance.Data.CurrentLevel;

    private List<int> _availableTileIDs = new List<int>();
    List<int> _dummyIdList = new List<int>();

    private const int TOTAL_AVALABLE_TILE_ID = 14;

    
    private void Start()
    {
        if(_backgrounds.Count < 1)
        {
            DebugManager.Instance.LogError("[LEVEL MANAGER] Background List is empty");
            return;
        }
       // _saveData = SaveSystem.Load();
        LoadCurrentLevel();
    }

    /// <summary>
    /// Load dữ liệu level hiện tại
    /// </summary>
    public void LoadCurrentLevel()
    {
        LoadLevel(GameManager.Instance.Data.CurrentLevel);
    }

    /// <summary>
    /// Load dữ liệu level tiếp theo
    /// </summary>
    public void LoadNextLevel()
    {
        LoadLevel(GameManager.Instance.Data.CurrentLevel);
    }

    /// <summary>
    /// Load lại level hiện tại
    /// </summary>
    public void ReplayLevel()
    {
        _board.ClearBoard();
        BarManager.Instance.ClearBar();
        StartCoroutine(LoadLevelNextFrame());
    }

    IEnumerator LoadLevelNextFrame()
    {
        yield return null;
        
        LoadLevel(GameManager.Instance.Data.CurrentLevel);
    }

    /// <summary>
    /// Load level có ID = levelID
    /// </summary>
    /// <param name="levelID"></param>
    public void LoadLevel(int levelID)
    {
        /// load hình background lên
        LoadBackground();

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

        _dummyIdList.Clear();
        _availableTileIDs.Clear();
        for(int i = 1; i <= TOTAL_AVALABLE_TILE_ID; i++)
        {
            _dummyIdList.Add(i);
        }

        for(int i = 0; i < _levelData.AvailableTileNumber; i++)
        {
            int rnd = Random.Range(0, _dummyIdList.Count - 1);
            _availableTileIDs.Add(_dummyIdList[rnd]);
            _dummyIdList.RemoveAt(rnd);
        }

        // Load lại cái barr
        BarManager.Instance.UpdateHideTiles(_levelData.HideTiles);
        // load lại cái ui của màn chơi
        UIManager.Instance.UpdateLevelUI(CurrentLevelID);
        List<TileData> tileDatas = ConvertDataToGrid(_levelData);
        _board.InitBoard(tileDatas, _levelData.SpacingX, _levelData.SpacingY);
    }


    /// <summary>
    /// Chuyeerm dữ liệu từ Level Data thành List<TileData> để Board có thể đọc và tạo Object
    /// </summary>
    /// <param name="levelData"></param>
    /// <returns></returns>
    private List<TileData> ConvertDataToGrid(LevelData levelData)
    {
        List<TileData> _unsignedTileDatas = new List<TileData>();
        List<int> idPool = BuildIDPool(levelData.TotalTiles, _availableTileIDs);

        foreach(LayerData layer in levelData.Layers)
        {
            foreach(TilePosition tilePos in layer.TilePositions)
            {
                TileData newTileData = new TileData(tilePos.X, tilePos.Y, layer.ZIndex, -1);
                _unsignedTileDatas.Add(newTileData);
            }
        }

        /// Mix _tileList bằng Fisher yates
        bool isSuccess = false;
        int attemp = 0;
        while(!isSuccess && attemp < 30)
        {
            foreach (var t in _unsignedTileDatas) t.TileID = -1;
            ShuffleList(idPool);
            isSuccess = AssignTilesBackward(_unsignedTileDatas, idPool);
            attemp += 1;
        }

        if (!isSuccess)
        {
            Debug.LogWarning($"[LEVEL MANAGER] Sinh ngược thất bại sau {attemp} lần, dùng sinh xuôi!");
            AssignTilesForward(_unsignedTileDatas, idPool);
        }
        else
        {
            Debug.Log($"[LEVEL MANAGER] Sinh ngược thành công sau {attemp} lần!");
        }

        return _unsignedTileDatas;
    }

    /// <summary>
    /// Hàm tạo tile bằng cách đặt tuyến tính
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="idPool"></param>
    private void AssignTilesForward(List<TileData> tiles, List<int> idPool)
    {
        ShuffleList(tiles);
        ShuffleList(idPool);

        int mark = 0;
        int setsNeeded = tiles.Count / 3;
        for (int i = 0; i < setsNeeded; i++)
            for (int j = 0; j < 3; j++)
                tiles[mark++].TileID = idPool[i % idPool.Count];
    }

    /// <summary>
    /// Random pool ID có thể xuất hiện
    /// </summary>
    /// <param name="totalTiles"></param>
    /// <param name="availableIDs"></param>
    /// <returns></returns>
    private List<int> BuildIDPool(int totalTiles, List<int> availableIDs)
    {
        List<int> pool = new List<int>();
        int setsNeeded = totalTiles / 3;
 
        for (int i = 0; i < setsNeeded; i++)
            pool.Add(availableIDs[i % availableIDs.Count]);
 
        // Shuffle pool ID
        ShuffleList(pool);
        return pool;
    }

    // private bool AssignTilesBackward(List<TileData> tiles, List<int> listIDs)
    // {
    //     List<TileData> remainingTiles = new List<TileData>(tiles);
    //     int idIndex = 0;

    //     while(remainingTiles.Count > 0)
    //     {
    //         List<TileData> unblockeds = remainingTiles.Where(t => !IsTileBlocked(t, remainingTiles))
    //                                     .ToList();

    //         if(unblockeds.Count < 3)
    //         {
    //             DebugManager.Instance.LogWarning("[LEVEL MANAGER] Số tile được unblock khi chạy thuật toán sinh ngược < 3 -> không thể tiếp tục được");
    //             return false;
    //         }

    //         ShuffleList(unblockeds);
    //         int assignID = listIDs[idIndex++];
    //         for(int i = 0; i < 3; i++)
    //         {
    //             unblockeds[i].TileID = assignID;
    //             remainingTiles.Remove(unblockeds[i]); // xóa khỏi remaining
    //         }


    //     }
    //     return true;
    // }

    // /// <summary>
    // /// Kiểm tra 1 list Tile Data có thể giải được hay không
    // /// </summary>
    // /// <param name="tileDatas"></param>
    // /// <returns></returns>
    // private bool CheckIfCanSolve(List<TileData> tileDatas)
    // {
    //     // List<TileData> dummy = new List<TileData>(tileDatas);
    //     List<TileData> dummy = tileDatas.Select(t => t.Clone()).ToList();   // deep copy để tránh ảnh hưởng đến tile data gốc
    //     Dictionary<int, int> unblockedTiles = new Dictionary<int, int>();
    //     while(dummy.Count > 0)
    //     {
    //         unblockedTiles.Clear();
    //         foreach(var tile in dummy) tile.IsBlocked = IsTileBlocked(tile, dummy);

    //         foreach(var tile in dummy)
    //         {
    //             if (!tile.IsBlocked)
    //             {
    //                 if(unblockedTiles.ContainsKey(tile.TileID))
    //                     unblockedTiles[tile.TileID]++;
                        
    //                 else
    //                     unblockedTiles.Add(tile.TileID, 1);
    //             }
    //         }

    //         int matchId = -1;
    //         foreach(var pair in unblockedTiles)
    //         {
    //             if (pair.Value >= 3) 
    //             {
    //                 matchId = pair.Key;
    //                 break;
    //             }
    //         }

    //         if(matchId == -1) return false;

    //         int removed = 0;
    //         for (int i = dummy.Count - 1; i >= 0 && removed < 3; i--)
    //         {
    //             if (dummy[i].TileID == matchId && !dummy[i].IsBlocked)
    //             {
    //                 dummy.RemoveAt(i);
    //                 removed++;
    //             }
    //         }
    //     }
    //     return true;
    // }
    
    /// <summary>
    /// Hàm sinh ngược tiles
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="listIDs"></param>
    /// <returns></returns>
    private bool AssignTilesBackward(List<TileData> tiles, List<int> listIDs)
    {
        List<TileData> remainingTiles = new List<TileData>(tiles);
        int idIndex = 0;

        while (remainingTiles.Count > 0)
        {
            List<TileData> unblockeds = remainingTiles
                .Where(t => !IsTileBlocked(t, remainingTiles))
                .ToList();

            if (unblockeds.Count < 3) return false;

            ShuffleList(unblockeds);

            bool found = false;
            for (int start = 0; start <= unblockeds.Count - 3; start++)
            {
                List<TileData> candidate = new List<TileData>
                {
                    unblockeds[start],
                    unblockeds[start + 1],
                    unblockeds[start + 2]
                };

                List<TileData> testRemaining = remainingTiles
                    .Where(t => !candidate.Contains(t))
                    .ToList();

                int unblockedAfter = testRemaining
                    .Count(t => !IsTileBlocked(t, testRemaining));

                if (testRemaining.Count == 0 || unblockedAfter >= 3)
                {
                    int assignID = listIDs[idIndex++];
                    foreach (var t in candidate)
                    {
                        t.TileID = assignID;
                        remainingTiles.Remove(t);
                    }
                    found = true;
                    break;
                }
            }

            if (!found) return false;
        }

        return true;
    }
    /// <summary>
    /// Kiểm tra tile có bị khóa không
    /// </summary>
    /// <param name="tileToCheck"></param>
    /// <param name="tileList"></param>
    /// <returns></returns>
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

    private void LoadBackground()
    {
        int bgId = (CurrentLevelID / 5) % _backgrounds.Count;
        _currentBackground.sprite = _backgrounds[bgId];

        _currentBackground.transform.localScale = Vector3.one;
        float spriteWidth = _currentBackground.sprite.bounds.size.x;
        float spriteHeight = _currentBackground.sprite.bounds.size.y;

        Camera mainCamera = Camera.main;
        float worldScreenHeight = mainCamera.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;

        float fitValue = Mathf.Max(scaleX, scaleY);
        _currentBackground.transform.localScale = new Vector3(fitValue, fitValue, 1f);
    }
}


