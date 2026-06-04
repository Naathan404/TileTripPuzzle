using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Cài đặt Level")]
    [SerializeField] private string _levelFileName = "Level_1";
    [Header("Components")]
    [SerializeField] private Board _board; 
    private LevelData _levelData;
    private List<TileData> _tileDatas = new List<TileData>();

    private Vector2 _tileSpacing = new Vector2(1.2f, 1.4f);

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }



    private void Start()
    {
        LoadLevelData();
        StartLevel();
    }

    private void StartLevel()
    {
        _board.InitBoard(_tileDatas, _tileSpacing.x, _tileSpacing.y);
    }

    private void LoadLevelData()
    {
        TextAsset jsonTextAssets = Resources.Load<TextAsset>("Levels/" + _levelFileName);

        if(jsonTextAssets == null)
        {
            Debug.LogError($"[Lỗi] Không tìm thấy file {_levelFileName} trong hệ thống");
            return;
        }

        _levelData = JsonUtility.FromJson<LevelData>(jsonTextAssets.text);

        ConvertDataToLevelGrid();
    }

    private void ConvertDataToLevelGrid()
    {
        List<TileData> _unsignedTileDatas = new List<TileData>();
        _tileSpacing.x = _levelData.SpacingX;
        _tileSpacing.y = _levelData.SpacingY;

        foreach(LayerData layer in _levelData.Layers)
        {
            foreach(TilePosition tilePos in layer.TilePositions)
            {
                TileData newTileData = new TileData(tilePos.X, tilePos.Y, layer.ZIndex, -1);
                _unsignedTileDatas.Add(newTileData);
            }
        }

        /// Mix _tileList bằng Fisher yates
        for(int i = _unsignedTileDatas.Count - 1; i >= 1; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            var temp = _unsignedTileDatas[rnd];
            _unsignedTileDatas[rnd] = _unsignedTileDatas[i];
            _unsignedTileDatas[i] = temp;
        }
        
        int mark = 0;
        for(int i = 0; i < _levelData.TotalTiles / 3; i++)
        {
            int randomId = _levelData.AvailableTileIDs[Random.Range(0, _levelData.AvailableTileIDs.Count)];
            for(int j = 0; j < 3; j++)
            {
                _unsignedTileDatas[mark++].TileID = randomId;
            }
        }



        _tileDatas = _unsignedTileDatas;
    }
}
