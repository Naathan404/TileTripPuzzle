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
    [SerializeField] private float _fadeInDuration = 0.2f;
    [SerializeField] private float _disappearDuration = 0.2f;
    [SerializeField] private Sprite[] _imageList;
    [Header("Tile Components")]
    [SerializeField] private SpriteRenderer _imageSprite;
    [SerializeField] private ParticleSystem _effect;
    private BoxCollider2D _collider;


    private SpriteRenderer _spriteRenderer;
    private Vector3 _originalScale;
    private Color _disableColor = new Color(0.5f, 0.5f, 0.5f);

    public static event System.Action<Tile> OnTileClicked;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(BarManager.Instance.IsFull)
        {
            return;
        }
        if(GameManager.Instance.CurrentGameState != GameState.Playing)
        {
            return;
        }
        
        if(Data.IsBlocked)
        {
            int rnd = Random.Range(1, 10);
            if(rnd > 5)
                transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.1f, vibrato: 1, elasticity: 0.5f).SetEase(Ease.OutElastic);
            else
                transform.DOPunchRotation(new Vector3(0, 0, -15f), 0.1f, vibrato: 1, elasticity: 0.5f).SetEase(Ease.OutElastic);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Blocked);
            Debug.Log($"Tile at position: ({Data.X}, {Data.Y}) is blocked. Click ignored.");
            return;
        }
        
        AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Tap);
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
            //_collider.enabled = false;
        }
        else
        {
            //_collider.enabled = true;

            _imageSprite.DOColor(Color.white, _fadeInDuration);
            _spriteRenderer.DOColor(Color.white, _fadeInDuration);
        }
        Data.IsBlocked = wasBlocked;
    }

    public void RefreshVisual()
    {
        if (Data.TileID < 0 || Data.TileID >= _imageList.Length) return;
        _imageSprite.sprite = _imageList[Data.TileID];
    }

    public void PlayDisappearAnimation()
    {
        _collider.enabled = false;
        _spriteRenderer.sortingOrder = 998;
        _imageSprite.sortingOrder = 999;

        ParticleSystem eff = EffectManager.Instance.PlayTileEffect();
        eff.transform.position = this.transform.position;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.3f, 0.1f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(0f, _disappearDuration).SetEase(Ease.InBack));
        seq.Join(_imageSprite.DOColor(new Color(500, 500, 500), _disappearDuration * 2f));
        seq.Join(transform.DORotate(new Vector3(0f, 0f, Random.Range(-45f, 45f)), _disappearDuration));
        seq.Join(_spriteRenderer.DOFade(0f, _disappearDuration));
        seq.Join(_imageSprite.DOFade(0f, _disappearDuration));

        seq.OnComplete(() => {
            Destroy(gameObject);
            EffectManager.Instance.ReleaseTileEffect(eff);
        });
    }

    public void HighlightTile()
    {
        transform.DOKill();
        _spriteRenderer.DOKill();
        _imageSprite.DOKill();

        transform.localScale = _originalScale;
        _spriteRenderer.color = Color.white;
        _imageSprite.color = Color.white;

        transform.DOPunchScale(_originalScale * 0.2f, 0.8f, vibrato: 3, elasticity: 0.5f);

        _imageSprite.DOColor(Color.yellow, 0.2f).SetLoops(4, LoopType.Yoyo);
        _spriteRenderer.DOColor(Color.yellow, 0.2f)
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(() => {
                _spriteRenderer.color = Color.white;
                _imageSprite.color = Color.white;
            });
    }
}
