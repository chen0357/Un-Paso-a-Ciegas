using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("IsHover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("IsHover", false);
        animator.SetBool("IsPressed", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool("IsPressed", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        animator.SetBool("IsPressed", false);
    }
}