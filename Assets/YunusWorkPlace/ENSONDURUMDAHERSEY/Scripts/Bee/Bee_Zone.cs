using UnityEngine;

public class Bee_Zone : MonoBehaviour
{
    [Header("Dönüş")]
    public float rotationSpeed = 20f;

    [Header("Arı Pozisyonları")]
    public Transform[] beeOrbitPoints;

    private void Update()
    {
        // Yavaşça dönsün (görsel hava katması için)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    public Transform GetRandomOrbitPoint()
    {
        if (beeOrbitPoints.Length == 0) return transform;
        return beeOrbitPoints[Random.Range(0, beeOrbitPoints.Length)];
    }
}
