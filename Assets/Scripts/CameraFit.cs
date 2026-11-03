using UnityEngine;

// Держит одинаковую ширину мира (±12) на любом соотношении сторон:
// на планшете 4:3 камера поднимается выше, на 20:9 — опускается.
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    public float halfWidth = 12f;

    void Start() { Fit(); }
    void Update() { Fit(); }

    public void Fit()
    {
        var cam = GetComponent<Camera>();
        cam.orthographicSize = halfWidth / cam.aspect;
    }
}
