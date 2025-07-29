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
    private void RemoveDiamondIfExists()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allChildren)
        {
            if (t.name.ToLower().Contains("diamond"))
            {
                Destroy(t.gameObject); // ✅ elması sil
              //  Debug.Log($"{name} → 💼 Employee'den elmas kaldırıldı.");
                break;
            }
        }
    }

    private IEnumerator DelayedInitialize()
    {
        // Diğer nesnelerin hazır olması için 1 frame bekle
        yield return null;

        // Her ihtimale karşı 0.5 saniye daha bekleyebilirsin
        yield return new WaitForSeconds(0.5f);
        RemoveDiamondIfExists(); // 💎 Elmas varsa sil
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.OnMinuteChanged += HandleMinuteChanged;

            scheduledBehaviorRoutine = StartCoroutine(ExecuteDaySchedule());
           // Debug.Log($"{name} → ⏰ Zaman çizelgesine abone oldu.");
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
    private bool isBusy = false;

     public override IEnumerator RandomRoamForSeconds(float duration)
    {
        isBusy = true;
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
                    continue; // Yeni hedef ara
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

            // Hedefe ulaşana kadar bekle
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



            // Yürüyüş bittiğinde durdur
            agent.ResetPath();
            animator.SetBool("isWalking", false);

            float idleTime = Random.Range(3f, 6f); // 🧘‍♂️ dinlenme süresi
            yield return new WaitForSeconds(idleTime);
            timer -= idleTime;
        }

        isBusy = false;
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
                    //MoveNear(NoktaSpot.Instance.GetPoolSpot(), 8f);
                    StartRoaming();
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
    
    protected override IEnumerator ChaseThenThrow()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null)
        {
            isReacting = false;
            yield break;
        }

        currentTarget = crow;
        animator.SetBool("isRunning", true);

        float chaseDuration = 4f;
        float t = 0f;

        while (t < chaseDuration && crow != null)
        {
            agent.SetDestination(crow.transform.position);
            FaceTarget(crow);

            t += Time.deltaTime;
            yield return null;
        }

        // ❄️ Sabitle
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        animator.SetBool("isRunning", false);

        Vector3 frozenPos = transform.position;
        agent.Warp(frozenPos);

        yield return new WaitForSeconds(0.05f); // buffer

        // 🎯 Throw animasyonunu başlat
        animator.SetBool("throw", true);
        animator.CrossFade("throw", 0.1f);

        // 🕒 Sabit süre: 3 saniye bekle
        float throwDuration = 4f;
        float elapsed = 0f;
        while (elapsed < throwDuration)
        {
            transform.position = frozenPos;
            agent.nextPosition = frozenPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 🔄 Bitti
        animator.SetBool("throw", false);

        yield return new WaitForSeconds(0.1f); // geçiş buffer

        StopAllAnimations();

        // 🔓 Agent tekrar aktif
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.nextPosition = frozenPos;

        isReacting = false;
        currentTarget = null;

        Debug.Log($"{name} → Throw (3s) tamamlandı. Hayata dönüyor.");
        StartCoroutine(ResumeRoamAfterCooldown());
    }

    





}
