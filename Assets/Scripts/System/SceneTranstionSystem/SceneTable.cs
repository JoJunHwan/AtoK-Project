using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneTable", menuName = "Game/Scene Table")]
public class SceneTable : ScriptableObject
{
    public List<SceneInfo> scenes = new List<SceneInfo>();

    public SceneInfo GetSceneByName(string name)
    {
        foreach (var info in scenes)
        {
            if (info.sceneName == name)
            {
                return info;
            }
        }
        return null;
    }

    public SceneInfo GetSceneByIndex(int index)
    {
        if (index < 0 || index >= scenes.Count) return null;
        return scenes[index];
    }
}