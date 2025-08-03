using UnityEngine;

public class StoneThrower : MonoBehaviour
{
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Rigidbody stonePrefab;
    [SerializeField] private Transform throwOrigin;

    [Header("Stress Ayarları")]
    [SerializeField] private float currentStress = 50f;
    [SerializeField] private float maxStress = 100f;
    [SerializeField] private float maxOffsetAmount = 2f;

    [Header("Atış Ayarları")]
    [SerializeField] private float throwPower = 20f;
    [SerializeField] private float upwardAimOffset = 0.5f;

    private NPC_Base_Test npc_base_test;
    void Start()
    {
        npc_base_test = GetComponent<NPC_Base_Test>();
        maxStress = npc_base_test.maxStress;
    }
    void Update()
    {
        playerTarget = GameObject.Find("Player").transform;
        currentStress = npc_base_test.currentStress;
    }

    public void ThrowStone()
    {
        Vector3 origin = throwOrigin.position;

        // hedefin biraz üstüne sapmalı
        Vector3 finalTarget = GetTargetWithStressOffset(playerTarget.position);

        Vector3 direction = (finalTarget - origin).normalized;

        Rigidbody stone = Instantiate(stonePrefab, origin, Quaternion.identity);
        stone.useGravity = false;
        stone.velocity = direction * throwPower;
    }

    Vector3 GetTargetWithStressOffset(Vector3 originalTarget)
    {
        float stressNormalized = Mathf.Clamp01(currentStress / maxStress);
        float offsetAmount = Mathf.Lerp(maxOffsetAmount, 0f, stressNormalized);
        Vector2 offset = Random.insideUnitCircle * offsetAmount;
        return originalTarget + new Vector3(offset.x, 0f, offset.y);
    }



}
