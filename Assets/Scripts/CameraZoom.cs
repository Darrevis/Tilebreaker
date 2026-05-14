using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScrollZoom : MonoBehaviour
{
    public Camera cam;
    public float zoomSpeed = 0.5f;
    public float minSize = 3f;
    public float maxSize = 10f;

    void Start()
    {
        cam = Camera.main;
        Debug.Log("Zoom script running");
    }

    void Update()
    {
        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (scroll.y != 0)
        {
            cam.orthographicSize -= scroll.y * zoomSpeed;

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize,
                minSize,
                maxSize
            );
        }
    }
}