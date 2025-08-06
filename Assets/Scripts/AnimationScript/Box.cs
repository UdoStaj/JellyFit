using UnityEngine;

public class Box : MonoBehaviour
{
    public void LevelCompleted()
    {
        LevelManager.instance.OnLevelCompleted();
        SoundManager.Instance.PlaySuccess();
    }
}
