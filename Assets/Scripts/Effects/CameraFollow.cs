using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform playerBody;
    Vector3 newPosition;
    // Update is called once per frame
    void Update()
    {
        newPosition = new Vector3 (playerBody.transform.position.x, playerBody.transform.position.y, -2f);
        transform.position = newPosition;
    }
}
