using UnityEngine;

[CreateAssetMenu(fileName = "SceneInfo", menuName = "Game/Scene Info")]
public class SceneInfo : ScriptableObject
{
    public string sceneName;        // 실제 빌드 세팅 이름
    public string displayName;      // UI나 로그용 이름
    public Sprite previewImage;     // 미리보기 이미지 (선택)
}