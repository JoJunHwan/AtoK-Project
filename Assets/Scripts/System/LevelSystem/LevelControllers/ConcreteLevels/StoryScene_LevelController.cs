using UnityEngine;

public class StoryScene_LevelController : LevelController
{
    public override void StartLevel()
    {
        base.sceneBGMController.PlayByKey("None");
    }
}
