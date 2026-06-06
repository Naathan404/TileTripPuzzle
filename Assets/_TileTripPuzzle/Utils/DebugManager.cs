using UnityEngine;
using UnityEngine.Rendering;

public class DebugManager : Singleton<DebugManager>
{
    public bool IsDebugMode = true;

    /// <summary>
    /// Debug Log bình thường
    /// </summary>
    /// <param name="log"></param>
    public void Log(string log = "Log some debug")
    {
        if(!IsDebugMode) return;
        Debug.Log(log);
    }

    /// <summary>
    /// Log lỗi
    /// </summary>
    /// <param name="log"></param>
    public void LogError(string log = "Log some error")
    {
        if(!IsDebugMode) return;
        Debug.LogError(log);
    }

    /// <summary>
    /// Log cảnh báo
    /// </summary>
    /// <param name="log"></param>
    public void LogWarning(string log = "Log some warning")
    {
        if(!IsDebugMode) return;
        Debug.LogWarning(log);
    }
}
