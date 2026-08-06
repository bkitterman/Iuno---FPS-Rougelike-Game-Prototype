using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _cameraTransform;
    public bool flip = false;

    void Start()
    {
        // Find the main camera once at the start
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    // Use LateUpdate to ensure the camera has finished its movement for the frame
    void LateUpdate()
    {
        if (_cameraTransform == null) return;

        // Make this object's forward direction point towards the camera
        transform.LookAt(_cameraTransform);
        if (flip)
            transform.Rotate(new Vector3(0f, 180f, 0f));
    }
}