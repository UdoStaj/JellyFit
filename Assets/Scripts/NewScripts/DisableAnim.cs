using UnityEngine;

public class DisableAnim : MonoBehaviour
{
    Animator an;

    private void Start()
    {
        an = GetComponent<Animator>();
    }

    public void DisableAnimation()
    {
        an.SetBool("isSlicing", false);
    }
}
