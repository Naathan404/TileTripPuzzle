using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile Settings")]
    public TileData Data;
    public int TileID => Data.TileID;
    [SerializeField] private float _fadeInDuration = 0.25f;
    [SerializeField] private Sprite[] _imageList;
    [Header("Tile Components")]
    [SerializeField] private SpriteRenderer _imageSprite;
    private BoxCollider2D _collider;

    private SpriteRenderer _spriteRenderer;
    private Color _disableColor = new Color(0.5f, 0.5f, 0.5f);

    public static event System.Action<Tile> OnTileClicked;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(BarManager.Instance.IsFull) return;
        if(BarManager.Instance.IsProcessing) return;
        
        if(Data.IsBlocked)
        {
            Debug.Log($"Tile at position: ({Data.X}, {Data.Y}) is blocked. Click ignored.");
            return;
        }

        Debug.Log($"Tile clicked at position: ({Data.X}, {Data.Y})");
        _collider.enabled = false;
        Data.IsBlocked = true;

        _spriteRenderer.sortingOrder = 998;
        _imageSprite.sortingOrder = 999;
        OnTileClicked?.Invoke(this);

    }

    public void SetData(TileData data)
    {
        Data = data;
        _imageSprite.sprite = _imageList[Data.TileID]; 
        _spriteRenderer.sortingOrder = (Data.Z * 10) + (int)Data.Y;
        _imageSprite.sortingOrder = (Data.Z * 10) + 9;
    }

    public void SetStateBlocked(bool wasBlocked)
    {
        if(wasBlocked)
        {
            _imageSprite.color = _disableColor;
            _spriteRenderer.color = _disableColor;
            _collider.enabled = false;
        }
        else
        {
            _collider.enabled = true;

            _imageSprite.DOColor(Color.white, _fadeInDuration);
            _spriteRenderer.DOColor(Color.white, _fadeInDuration);
        }
        Data.IsBlocked = wasBlocked;
    }
}
