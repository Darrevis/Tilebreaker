using UnityEngine;

public class DashBreakPowerup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player.canDashBreakTiles < player.maxDashBreakCharges) { 
                player.EnableDashBreak();
                Destroy(gameObject);
            }
        }
    }
}