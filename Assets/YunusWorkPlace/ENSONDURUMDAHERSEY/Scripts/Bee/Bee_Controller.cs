using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee_Controller : MonoBehaviour
{
    private enum BeeType { Zone, Chase }
    private BeeType beeType;

    private Transform target;
    private Transform currentTarget;
    private Bee_Zone bee_Zone;
    private Rigidbody rb;

    [Header("Ortak Ayarlar")]
    [SerializeField] private float speed = 5f;

    [Header("Hasar Ayarları")]
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float zoneDamage = 10f;
    [SerializeField] private float chaseDamage = 5f;
    [SerializeField] private float chaseIndirectDamage = 15f;

    private float damageTimer = 0f;

    [Header("Zone Arıları")]
    [SerializeField] private float patrolZoneCheckRadius = 1f;

    [Header("Chase Arıları")]
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float orbitInterval = 1.5f;
    [SerializeField] private float npcHasarVermeAlani = 1.5f;

    [Header("Takip Süresi")]
    [SerializeField] private float followDuration = 25f;

    [Header("Efektler")]
[SerializeField] private GameObject deathEffectPrefab;


    private float followTimer = 0f;
    private bool isReturningToHive = false;
    private Vector3 hivePosition; // dönülecek nokta

    [SerializeField] private float zoneDuration = 30f;
    private float zoneTimer = 0f;
    private bool zoneReturning = false;
    private Vector3 offsetToTarget;
    private Vector3 currentOrbitTarget;
    private float orbitTimer = 0f;
    private bool reachedTarget = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (beeType == BeeType.Zone)
        {
            if (!zoneReturning)
            {
                zoneTimer += Time.deltaTime;
                if (zoneTimer >= zoneDuration)
                    zoneReturning = true;
            }

            if (!zoneReturning)
                PatrolZone();
            else
                ReturnToHive();
        }
        else if (beeType == BeeType.Chase)
        {
            if (!isReturningToHive)
            {
                followTimer += Time.deltaTime;
                if (followTimer >= followDuration)
                    isReturningToHive = true;
            }

            if (!isReturningToHive)
                ChaseTarget();
            else
                ReturnToHive();
        }
    }
    public void InitAsZoneBee(GameObject zoneArea, Vector3 hivePos)
    {
        beeType = BeeType.Zone;
        bee_Zone = zoneArea.GetComponent<Bee_Zone>();
        hivePosition = hivePos;
        PickNewTarget();
    }

    public void InitAsChaseBee(Transform player, Vector3 hivePos)
    {
        beeType = BeeType.Chase;
        target = player;

        hivePosition = hivePos; // dönüş için gerekli

        offsetToTarget = Random.onUnitSphere;
        offsetToTarget.y = Mathf.Abs(offsetToTarget.y); // yukarıda vızlasın

    }

    private void PatrolZone()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
            PickNewTarget();

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        rb.MovePosition(transform.position + dir * speed * Time.deltaTime);

        // NPC'lere hasar ver
        Collider[] hitNPCs = Physics.OverlapSphere(transform.position, patrolZoneCheckRadius, LayerMask.GetMask("NPC"));
        foreach (var npc in hitNPCs)
        {
            npc.GetComponent<BaseNPC>()?.TakeDamage((int)zoneDamage);
        }
    }

    private void ChaseTarget()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (!reachedTarget && distance < 1.5f)
        {
            reachedTarget = true;
            PickNewOrbitPosition();
        }

        if (!reachedTarget)
        {
            Vector3 chasePoint = target.position + offsetToTarget;
            Vector3 dir = (chasePoint - transform.position).normalized;
            rb.MovePosition(transform.position + dir * speed * Time.deltaTime);
            RotateTowards(dir);
        }
        else
        {
            OrbitIn3DSphere();
        }

        if (distance < 1f)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                var baseScript = target.GetComponent<Crow_Health>();
                if (baseScript != null)
                {
                    print("BaseScript bulundu!");
                    baseScript.TakeDamage((int)chaseDamage);
                }
                else
                {
                    print("BaseScript YOK!");
                }
                print("HASAR VERİLİD");
                damageTimer = 0f;
            }
        }

        Collider[] hitNPCs = Physics.OverlapSphere(transform.position, npcHasarVermeAlani, LayerMask.GetMask("NPC"));
        foreach (var npc in hitNPCs)
        {
            npc.GetComponent<BaseNPC>()?.TakeDamage((int)chaseIndirectDamage);
        }
    }

    private void OrbitIn3DSphere()
    {
        float dist = Vector3.Distance(transform.position, currentOrbitTarget);
        if (dist < 0.3f)
            PickNewOrbitPosition();

        Vector3 dir = (currentOrbitTarget - transform.position).normalized;
        float moveSpeed = dist < 0.3f ? speed * 0.8f : speed;

        rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);
        RotateTowards(target.position - transform.position);
    }

    private void PickNewTarget()
    {
        if (bee_Zone != null)
            currentTarget = bee_Zone.GetRandomOrbitPoint();
    }

    private void PickNewOrbitPosition()
    {
        orbitTimer = orbitInterval;
        currentOrbitTarget = target.position + Random.onUnitSphere * orbitRadius;
    }

    private void RotateTowards(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
    }

    private void ReturnToHive()
{
    Vector3 dir = (hivePosition - transform.position).normalized;
    rb.MovePosition(transform.position + dir * speed * Time.deltaTime);
    RotateTowards(dir);

    float dist = Vector3.Distance(transform.position, hivePosition);
    if (dist < 0.3f)
    {
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

}
