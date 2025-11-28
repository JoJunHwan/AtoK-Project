using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneTable", menuName = "Game/Scene Table")]
public class SceneTable : ScriptableObject
{
    public List<SceneInfo> sceneInfos = new List<SceneInfo>();

    public SceneInfo GetSceneByName(string name)
    {
        foreach (var sceneInfo in sceneInfos)
        {
            if (sceneInfo.sceneName == name)
            {
                return sceneInfo;
            }
        }
        return null;
    }

    public SceneInfo GetSceneByIndex(int index)
    {
        if (index < 0 || index >= sceneInfos.Count) return null;
        return sceneInfos[index];
    }
}