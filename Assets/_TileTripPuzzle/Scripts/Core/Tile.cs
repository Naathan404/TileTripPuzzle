using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile Settings")]
    public int TileID;
    [SerializeField] private int _x;
    [SerializeField] private int _y;
    [SerializeField] private bool _isBlocked;
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
        if(_isBlocked)
        {
            Debug.Log($"Tile at position: ({_x}, {_y}) is blocked. Click ignored.");
            return;
        }

        Debug.Log($"Tile clicked at position: ({_x}, {_y})");
        _collider.enabled = false;
        _isBlocked = true;

        OnTileClicked?.Invoke(this);

    }
}
