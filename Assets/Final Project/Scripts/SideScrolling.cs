using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SideScrolling : MonoBehaviour
{
    private Transform player;
    public float height = 6.5f;
    public float undergroundHeight = -9.5f;
    public float undergroundThreshold = 0f;
    public float followThreshold = 1.5f;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        Vector3 cameraPosition = transform.position;
        float offset = player.position.x - cameraPosition.x;
        if (offset > followThreshold)
        {
            cameraPosition.x = player.position.x - followThreshold;
        }
        transform.position = cameraPosition;
    }

    public void SetUnderground(bool underground)
    {
        Vector3 cameraPosition = transform.position;
        cameraPosition.y = underground ? undergroundHeight : height;
        transform.position = cameraPosition;
    }

}