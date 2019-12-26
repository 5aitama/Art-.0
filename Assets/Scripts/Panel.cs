
using UnityEngine;

public class Panel : MonoBehaviour
{
    private Animator animator;
    public UnityEngine.UI.Text text;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowHide()
    {
        bool isHide = animator.GetBool("hide");
        animator.SetBool("hide", !isHide);

    }

    public void ChangeName()
    {
        bool isHide = animator.GetBool("hide");
        if(!isHide)
        {
            text.text = "Hide options";
        } else {
            text.text = "Show options";
        }
    }
}
