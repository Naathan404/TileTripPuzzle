using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private List<Tile> _tileList = new List<Tile>();
    
    [Header("References")]
    [SerializeField] private GameObject _tilePrefab;

    private void OnEnable()
    {
        Tile.OnTileClicked += UpdateBoardVisual;
    }

    private void OnDisable()
    {
        Tile.OnTileClicked -= UpdateBoardVisual;
    }

    public void InitBoard(List<TileData> datas, float spacingX, float spacingY)
    {
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
            float posY = this.transform.position.y - tile.Data.Y * spacingY + tile.Data.Z * 0.1f;
            Vector3 finalPos = new Vector3(posX, posY, 0);
            tile.transform.position = finalPos;

            tile.GetComponent<SpriteRenderer>().sortingOrder = tile.Data.Z * 10;
            tile.name = $"Tile_{tile.Data.TileID}_Z{tile.Data.Z}";

            tile.SetStateBlocked(CheckTileBlocked(tile));
        }
    }



    #region Các hàm trợ giúp
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

        Debug.Log("Board nhận được tín hiệu click 1 Tile");
        if(temp.Data.Z == 0) return;

        foreach(var t in _tileList)
        {
            if(t.Data.Z >= temp.Data.Z) continue;

            t.SetStateBlocked(CheckTileBlocked(t));
        }
    }
    #endregion
}
