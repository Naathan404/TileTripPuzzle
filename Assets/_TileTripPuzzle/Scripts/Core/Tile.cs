using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile Settings")]
    public TileData Data;
    public int TileID => Data.TileID;
    [SerializeField] private Sprite[] _imageList;
    [Header("Tile Components")]
    [SerializeField] private SpriteRenderer _imageSprite;
    private BoxCollider2D _collider;

    public static event System.Action<Tile> OnTileClicked;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(Data.IsBlocked)
        {
            Debug.Log($"Tile at position: ({Data.X}, {Data.Y}) is blocked. Click ignored.");
            return;
        }

        Debug.Log($"Tile clicked at position: ({Data.X}, {Data.Y})");
        _collider.enabled = false;
        Data.IsBlocked = true;

        OnTileClicked?.Invoke(this);

    }

    public void SetData(TileData data)
    {
        Data = data;
        _imageSprite.sprite = _imageList[Data.TileID]; 
        _imageSprite.sortingOrder = data.Z * 10 + 1;
        if(data.IsBlocked)
        {
            _imageSprite.color = Color.gray;
            
        }
    }
}
