using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int LevelID;
    public string LevelName;
    public int TotalTiles; 
    public float SpacingX = 0.5f;
    public float SpacingY = 0.5f;
    public int AvailableTileNumber;
    
    public List<LayerData> Layers = new List<LayerData>();
}

[System.Serializable]
public class LayerData
{
    public int ZIndex; 
    public List<TilePosition> TilePositions = new List<TilePosition>();
}

[System.Serializable]
public class TilePosition
{
    public float X;
    public float Y;
    public TilePosition(float x, float y)
    {
        X = x;
        Y = y;
    }
}