using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : class
{
    public static T Instance;
    protected void Awake()
    {
        if(Instance != null && Instance != this as T)
        {
           Destroy(this.gameObject);
           return; 
        }
        Instance = this as T;
        DontDestroyOnLoad(this.gameObject);
    }
}