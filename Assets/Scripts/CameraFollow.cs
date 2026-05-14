using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 newPosition = new Vector3(
            target.position.x,
            target.position.y + 1,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            newPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}