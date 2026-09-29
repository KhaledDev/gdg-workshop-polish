using UnityEngine;

public class Stage2_Target : MonoBehaviour
{
    int hitCount;

    public void Hit()
    {
        hitCount++;
        Debug.Log($"Target hit! Total hits: {hitCount}");
    }
}
