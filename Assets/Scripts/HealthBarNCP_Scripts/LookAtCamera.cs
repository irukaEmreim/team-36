using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 lookDir = transform.position - cam.transform.position;
        lookDir.y = 0f; // sadece yatay eksende döndürmek için
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}