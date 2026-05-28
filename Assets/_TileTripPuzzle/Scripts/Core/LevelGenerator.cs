using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.VersionControl;
using System.Linq;
using System.Data.Common;
using Unity.VisualScripting;

public class LevelGenerator : MonoBehaviour
{
    [Header("Cài đặt Level")]
    [SerializeField] private string _levelFileName = "Level_1";
    [SerializeField] private int[] _availableTileIDs = { 0, 1, 2, 3, 4 }; 
    [Header("Components")]
    [SerializeField] private Board _board; 
    private LevelData _levelData;
    private List<TileData> _tileDatas = new List<TileData>();
    private Vector2 _tileSpacing = new Vector2(1.2f, 1.4f);

    private void Start()
    {
        LoadLevelData();


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

        int mark = 0;
        for(int i = 0; i < _levelData.TotalTiles / 3; i++)
        {
            int randomId = _availableTileIDs[Random.Range(0, _availableTileIDs.Length)];
            for(int j = 0; j < 3; j++)
            {
                _unsignedTileDatas[mark++].TileID = randomId;
            }
        }

        _tileDatas = _unsignedTileDatas;
    }

#region Các hàm trợ giúp
    // private int CalculateTopZ(float x, float y)
    // {
    //     int maxZ = -1; 
    //     float tileWidth = 1f;
    //     float tileHeight = 1f;

    //     foreach (TileData tile in _tileDatas)
    //     {
    //         float distanceX = Mathf.Abs(tile.X - x);
    //         float distanceY = Mathf.Abs(tile.Y - y);

    //         if (distanceX < tileWidth && distanceY < tileHeight)
    //         {
    //             if (tile.Z > maxZ)
    //             {
    //                 maxZ = tile.Z; 
    //             }
    //         }
    //     }
    //     return maxZ + 1;
    // }

    // public bool CheckTileBlocked(TileData tileToCheck)
    // {
    //     /// Kiểm tra overlapping: |X_A - X_B| < Width |Y_A - Y_B| < Height
    //     foreach(var other in _tileDatas)
    //     {
    //         if(other.Z > tileToCheck.Z)
    //         {
    //             float disX = Mathf.Abs(other.X - tileToCheck.X);
    //             float disY = Mathf.Abs(other.Y - tileToCheck.Y);

    //             if (disX < tileToCheck.Width && disY < tileToCheck.Height)
    //                 return true;
    //         }
    //     }

    //     return false;
    // }
#endregion
}
