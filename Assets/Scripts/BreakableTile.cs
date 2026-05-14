using UnityEngine;

public class BreakableTile : MonoBehaviour, IBreakable
{
    public void Break()
    {
        Destroy(gameObject);
    }
}