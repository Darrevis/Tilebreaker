using TMPro;
using UnityEngine;

public class DashPowerupUI : MonoBehaviour
{
    public PlayerMovement player;
    public TMP_Text text;

    void Update()
    {
        string s = "";
        for (int i = 0; i < player.maxDashBreakCharges; i++)
        {
            s += i < player.canDashBreakTiles ? "● " : "○ ";
        }

        text.text = s;
    }
}