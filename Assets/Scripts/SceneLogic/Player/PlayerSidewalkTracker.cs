using UnityEngine;

public class PlayerSidewalkTracker : MonoBehaviour
{
    private int overlapCount;

    public bool IsOnSidewalk => overlapCount > 0;

    public void EnterSidewalk()
    {
        overlapCount++;
    }

    public void ExitSidewalk()
    {
        overlapCount = Mathf.Max(0, overlapCount - 1);
    }
}
