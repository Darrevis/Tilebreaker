using UnityEngine;

public class DashReadyPopup : MonoBehaviour
{
    public PlayerMovement player;
    public GameObject popup;

    private bool wasReady;

    void Update()
    {
        bool isReady = player.IsDashReady();

        // trigger only on transition (cooldown → ready)
        if (isReady && !wasReady)
        {
            ShowPopup();
        }

        wasReady = isReady;
    }

    void ShowPopup()
    {
        StopAllCoroutines();
        StartCoroutine(PopupRoutine());
    }

    System.Collections.IEnumerator PopupRoutine()
    {
        popup.SetActive(true);

        yield return new WaitForSeconds(0.8f);

        popup.SetActive(false);
    }
}