using UnityEngine;

/// <summary>
/// Drives the "walk" bool on child Animator(s) based on actual movement.
/// Works with AdvancedPeoplePack controllers (MaleAnimator / Female_animator).
/// </summary>
public class PedestrianAnimator : MonoBehaviour
{
    [SerializeField] private string walkBoolParameter = "walk";
    [SerializeField] private float moveThreshold = 0.002f;

    private Animator[] animators;
    private Vector3 lastPosition;
    private bool isWalking;

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>(true);
        lastPosition = transform.position;
        SetWalking(false);
    }

    private void Update()
    {
        if (animators == null || animators.Length == 0)
            return;

        bool shouldWalk = (transform.position - lastPosition).sqrMagnitude >
                          moveThreshold * moveThreshold;

        if (shouldWalk != isWalking)
        {
            isWalking = shouldWalk;
            SetWalking(isWalking);
        }

        lastPosition = transform.position;
    }

    private void SetWalking(bool walking)
    {
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
                continue;

            animator.SetBool(walkBoolParameter, walking);
        }
    }
}
