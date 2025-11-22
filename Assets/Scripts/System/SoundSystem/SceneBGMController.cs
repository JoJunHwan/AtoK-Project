using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SceneBGMController : MonoBehaviour
{
    [Header("BGM Entries")]
    [SerializeField] private BgmEntry[] bgmEntries;

    private readonly Dictionary<string, BgmEntry> bgmMap = new Dictionary<string, BgmEntry>();

    // ---------- Lifecycle ----------
    public void InitByLevelController()
    {
        BuildMap();
    }

    // ---------- Public API ----------
    public void PlayByKey(string key)
    {
        if (key == "None")
        {
            Debug.LogWarning("BgmEntry가 None입니다");
            return;
        }
        
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("BgmEntry key가 없습니다");
            return;
        }
        if (bgmMap.ContainsKey(key) == false)
        {
            Debug.LogError("BgmEntry key가 다릅니다 ");
            return;
        }
        
        PrepareAndPlay(bgmMap[key]);
    }

    private void BuildMap()
    {
        bgmMap.Clear();
        if (bgmEntries == null) return;
        for (int i = 0; i < bgmEntries.Length; i++)
        {
            BgmEntry e = bgmEntries[i];
            if (e == null) continue;
            if (string.IsNullOrEmpty(e.key)) continue;
            if (bgmMap.ContainsKey(e.key) == false) bgmMap.Add(e.key, e);
        }
    }

    private void PrepareAndPlay(BgmEntry entry)
    {
        Debug.Assert(entry != null, "BgmEntry is null");
        SoundManager.Instance.PlayBGM(entry);
    }
}
