using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{
    [Header("Effect Refs")]
    [SerializeField] private ParticleSystem _tileDisappearEffect;
    public ObjectPool<ParticleSystem> _tileDisappearEffectPool;
    private Transform _tilePoolParent;

    public static EffectManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _tileDisappearEffectPool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(_tileDisappearEffect, _tilePoolParent).GetComponent<ParticleSystem>(),
            actionOnGet: eff => { 
                eff.gameObject.SetActive(true); 
                eff.Play(); 
            },
            actionOnRelease: eff => eff.gameObject.SetActive(true),
            actionOnDestroy: eff => GameObject.Destroy(eff.gameObject),
            maxSize: 9
        );
    }

    public ParticleSystem PlayTileEffect()
    {
        return _tileDisappearEffectPool.Get();
    }

    public void ReleaseTileEffect(ParticleSystem eff)
    {
        _tileDisappearEffectPool.Release(eff);
    }
}

