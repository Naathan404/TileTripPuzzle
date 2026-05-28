using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private float _tileSpacing = 1f;
    [SerializeField] private Vector2 _startTilePosition = Vector2.zero;
    [SerializeField] private List<Tile> _tileList = new List<Tile>();
    
    [Header("References")]
    [SerializeField] private GameObject _tilePrefab; 

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

        foreach(var Tile in _tileList)
        {
            float posX = this.transform.position.x + Tile.Data.X * spacingX;
            float posY = this.transform.position.y - Tile.Data.Y * spacingY;
            Vector3 finalPos = new Vector3(posX, posY, 0);
            Tile.transform.position = finalPos;

            Tile.GetComponent<SpriteRenderer>().sortingOrder = Tile.Data.Z * 10;
            Tile.name = $"Tile_{Tile.Data.TileID}_Z{Tile.Data.Z}";
        }
    }
}
