[System.Serializable]
public class TileData
{
    public float X, Y;
    public int Z;
    public int TileID;
    public bool IsBlocked;

    // size of tile
    public float Width = 1f;
    public float Height = 1f;

    public TileData(float x, float y, int z, int id)
    {
        X = x;
        Y = y;
        Z = z;
        TileID = id;
        IsBlocked = false;
    }

    /// <summary>
    ///  Tạo ra một bản sao data của TileData này
    /// </summary>
    /// <returns></returns>
    public TileData Clone()
    {
        return new TileData(X, Y, Z, TileID) 
        { 
            IsBlocked = this.IsBlocked,
            Width = this.Width,
            Height = this.Height
        };
    }
}


