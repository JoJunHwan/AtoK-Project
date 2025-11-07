using UnityEngine;

[System.Serializable]
public class BgmEntry
{
    public string key;            // 상황 키 (예: "Town", "Battle", "BossIntro")
    public AudioClip clip;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    public bool loop = true;
}