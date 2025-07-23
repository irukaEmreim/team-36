using System.Collections;
using UnityEngine;
using NPC_Scripts;
using UnityEngine.AI;

public class HotelEmployee : BaseNPC
{
    private float aggressionDistance = 4f;
    private Coroutine scheduledBehaviorRoutine;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(DelayedInitialize());
    }
    private IEnumerator DelayedInitialize()
    {
        // Diğer nesnelerin hazır olması için 1 frame bekle
        yield return null;

        // Her ihtimale karşı 0.5 saniye daha bekleyebilirsin
        yield return new WaitForSeconds(0.5f);

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.OnMinuteChanged += HandleMinuteChanged;

            scheduledBehaviorRoutine = StartCoroutine(ExecuteDaySchedule());
            Debug.Log($"{name} → ⏰ Zaman çizelgesine abone oldu.");
        }
        else
        {
            Debug.LogWarning($"{name} → GameTimeManager.Instance bulunamadı!");
        }
    }


    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.OnMinuteChanged -= HandleMinuteChanged;
        }
    }


    private void OnDisable()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.OnMinuteChanged -= HandleMinuteChanged;
        }
    }


    protected override void Update()
    {
        base.Update();
        if (!isReacting)
            CheckCrowProximityAndAttack();
    }

    private void HandleMinuteChanged(int minute)
    {
        if (this == null || gameObject == null) return; // ✅ Ölü referansa karşı önlem

        if (!this.enabled) return; // Komponent devre dışıysa çalışmasın

        Debug.Log($"[EMPLOYEE] {name} dakika {minute} - aktivite: {GameTimeManager.Instance.CurrentActivity}");
    }

    public override IEnumerator RandomRoamForSeconds(float duration)
    {
        float timer = duration;

        while (timer > 0)
        {
            Vector3 roamTarget = GetRandomNavmeshPoint(12f, 5f);

            if (NavMesh.SamplePosition(roamTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (!agent.isOnNavMesh)
                {
                    Debug.LogWarning($"{name} → agent NavMesh'te değil!");
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                agent.ResetPath();
                agent.SetDestination(hit.position);
                yield return new WaitForSeconds(0.1f); // 💥 Yol çizildi mi kontrol et

                if (!agent.hasPath)
                {
                    Debug.LogWarning($"{name} → Yol çizilemedi. agent.hasPath = false");
                    continue;
                }

                agent.isStopped = false;
                animator.SetBool("isWalking", true);

            }
            else
            {
                Debug.LogWarning($"{name} → Uzak hedef NavMesh dışında.");
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float wait = 0f;
            float timeout = 3f;

            while (agent.pathPending || agent.remainingDistance > 1.5f)
            {
                if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    Debug.Log($"{name} → Patika geçersiz veya eksik. Yeni hedef deneniyor.");
                    break;
                }

                wait += Time.deltaTime;
                if (wait > timeout)
                {
                    Debug.Log($"{name} → Patika çok uzun sürdü. Hedef iptal ediliyor.");
                    break;
                }

                yield return null;
            }

            agent.ResetPath();
            animator.SetBool("isWalking", false);

            float idleTime = Random.Range(3f, 6f);
            yield return new WaitForSeconds(idleTime);
            timer -= idleTime;
        }
    }


    private IEnumerator ExecuteDaySchedule()
    {
        while (true)
        {
            if (isReacting)
            {
                yield return null;
                continue;
            }

            if (GameTimeManager.Instance == null)
            {
                Debug.LogWarning($"{name} → GameTimeManager.Instance null! Schedule durdu.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            switch (GameTimeManager.Instance.CurrentActivity)
            {
                case GameTimeManager.DayActivity.Roaming:
                    StartRoaming();
                    break;

                case GameTimeManager.DayActivity.Sport:
                    if (Random.value < 0.66f)
                        MoveNear(NoktaSpot.Instance.GetSportArea(), 8f);
                    else
                        StartRoaming();
                    break;

                case GameTimeManager.DayActivity.Breakfast:
                    MoveNear(NoktaSpot.Instance.GetBreakfastTable(), 8f);
                    break;

                case GameTimeManager.DayActivity.PoolOrSit:
                    MoveNear(NoktaSpot.Instance.GetPoolSpot(), 8f);
                    break;

                case GameTimeManager.DayActivity.Lunch:
                case GameTimeManager.DayActivity.Dinner:
                    MoveNear(NoktaSpot.Instance.GetBreakfastTable(), 8f);
                    break;

                case GameTimeManager.DayActivity.GoInside:
                    MoveNear(NoktaSpot.Instance.GetIndoorArea(), 5f);
                    break;

                default:
                    StartRoaming();
                    break;
            }

            yield return new WaitForSeconds(5f); // Görev sıklığı
        }
    }

    private void MoveNear(Vector3 center, float radius)
    {
        Vector3 offset = Random.insideUnitSphere * radius;
        offset.y = 0f;
        Vector3 destination = center + offset;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            agent.ResetPath();
            agent.SetDestination(hit.position);
            agent.isStopped = false;

            animator.SetBool("isWalking", true);  // 🔁 Eskiden 'isRunning' idi
            animator.SetBool("isRunning", false);
        }
        else
        {
            Debug.LogWarning($"[EMPLOYEE] {name} hedef yakınında NavMesh bulunamadı.");
        }
    }


    private void CheckCrowProximityAndAttack()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null || isReacting) return;

        float distance = Vector3.Distance(transform.position, crow.transform.position);
        if (distance < aggressionDistance)
        {
            isReacting = true;
            StopAllAnimations();
            StartCoroutine(ChaseThenThrow());
        }
    }

    protected override NPCReactionType GetReactionType()
    {
        return NPCReactionType.ChaseAndThrow;
    }
}
