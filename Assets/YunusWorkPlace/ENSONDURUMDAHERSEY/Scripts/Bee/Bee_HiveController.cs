using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee_HiveController : MonoBehaviour
{
    public GameObject beePrefab;
    public Transform beeSpawnPoint;
    public int totalBees = 10;

    public float zoneBeeRatio = 0.4f; // %40'ı kovan etrafında uçar
    public float chaseBeeRatio = 0.6f; // %60'ı oyuncuya saldırır

    public GameObject beeZoneArea;

    public bool activated = false;

    private bool isShaking = false;

  [Header("Yeniden Kullanılabilirlik")]
    [SerializeField] private float reactivationCooldown = 10f; // kaç saniye sonra tekrar aktif edilebilir
    private float cooldownTimer = 0f;

    void Update()
    {
        if (activated)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= reactivationCooldown)
            {
                activated = false;
                cooldownTimer = 0f;

            }
        }
    }


    private IEnumerator ShakeHive(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        Quaternion originalRotation = transform.rotation;

        while (elapsed < duration)
        {
            float angle = Mathf.Sin(elapsed * 20f) * magnitude;
            transform.rotation = originalRotation * Quaternion.Euler(0f, 0f, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
        isShaking = false;
    }



    public void ActivateBeehive()
    {
        if (activated)
        {
            return;
        }

        activated = true;

        if (!isShaking)
            StartCoroutine(ShakeHive(1f, 5f)); // 1 saniye, 5 derece sağa-sola

        int zoneBeeCount = Mathf.RoundToInt(totalBees * zoneBeeRatio);
        int chaseBeeCount = totalBees - zoneBeeCount;

        for (int i = 0; i < zoneBeeCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            GameObject bee = Instantiate(beePrefab, beeSpawnPoint.position + randomOffset, Quaternion.identity);
            bee.GetComponent<Bee_Controller>().InitAsZoneBee(beeZoneArea,beeSpawnPoint.position);
        }

        for (int i = 0; i < chaseBeeCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            GameObject bee = Instantiate(beePrefab, beeSpawnPoint.position + randomOffset, Quaternion.identity);
            bee.GetComponent<Bee_Controller>().InitAsChaseBee(GameObject.FindGameObjectWithTag("lb_bird").transform,beeSpawnPoint.position);
        }
    }
}
