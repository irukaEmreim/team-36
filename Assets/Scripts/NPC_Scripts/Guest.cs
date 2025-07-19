using UnityEngine;
using System.Collections;

public class Guest : BaseNPC
{
    private bool isSitting = false;
    private Transform myChair = null;
    private bool isGoingToMeal = false;
    private float fearDistance = 7f;
    private float fearCooldown = 3f;
    private float lastFearTime = -999f;

    private Transform hipBone; // 🍑 Oturma hizalaması için

    protected override void Start()
    {
        base.Start();

        // Otomatik hipBone bul (LowManHips)
        if (hipBone == null)
        {
            var hips = GetComponentsInChildren<Transform>();
            foreach (var t in hips)
            {
                if (t.name.ToLower().Contains("hip"))
                {
                    hipBone = t;
                    break;
                }
            }

            if (hipBone == null)
                Debug.LogWarning($"{gameObject.name} → HipBone bulunamadı!");
        }
    }

    protected override NPCReactionType GetReactionType()
    {
        return NPCReactionType.Flee;
    }

    private void Update()
    {
        if (!isSitting && (isReacting || isGoingToMeal))
            return;

        if (!isSitting && ShouldGoToMeal())
            StartCoroutine(GoSitAndEatRoutine());

        CheckCrowProximity(); // 👈 bu her zaman çalışsın
    }

    
   

    

    private void CheckCrowProximity()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null || isReacting) return;

        float distance = Vector3.Distance(transform.position, crow.transform.position);
        if (distance < fearDistance)
        {
            if (isSitting)
            {
                animator.SetBool("SittingDodge", true);
                animator.SetBool("SittingTalk", false);
                StartCoroutine(ResetSittingDodge());
            }
            else
            {
                isReacting = true;
                StopAllAnimations();
                StartCoroutine(FleeThenYell());
            }
        }
    }




    private bool ShouldGoToMeal()
    {
        if (GameTimeManager.Instance == null)
            return false;

        var time = GameTimeManager.Instance.CurrentMealTime;
        return time != GameTimeManager.MealTime.None && Random.value < 0.005f;
    }

    private IEnumerator GoSitAndEatRoutine()
    {
        isGoingToMeal = true;
        Debug.Log($"🍽 {gameObject.name} → {GameTimeManager.Instance.CurrentMealTime} zamanı, yemeğe gidiyor.");

        var animCtrl = GetComponent<AnimationControl>();
        if (animCtrl != null) animCtrl.isExternallyControlled = true;

        myChair = ChairManager.Instance.GetAvailableChair();
        if (myChair == null)
        {
            isGoingToMeal = false;
            if (animCtrl != null) animCtrl.isExternallyControlled = false;
            yield break;
        }

        agent.SetDestination(myChair.position);
        agent.isStopped = false;
        animator.SetBool("isWalking", true);

        while (Vector3.Distance(transform.position, myChair.position) > 1f)
            yield return null;

        // 🛑 Sandalyeye vardığında dur
        agent.ResetPath();
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        transform.rotation = myChair.rotation;
        animator.SetBool("isWalking", false);

        // 🍑 HİPBONE hizalaması
        // 🍑 HİPBONE hizalaması (düzenlenmiş)
        if (hipBone != null)
        {
            Vector3 offset = transform.position - hipBone.position;

            // Y değerini sınırla (çok aşağı inmesin)
            offset.y = Mathf.Clamp(offset.y, 0.1f, 0.6f); // örnek aralık: ayar çekebilirsin

            transform.position = myChair.position + offset;
        }
        else
        {
            transform.position = myChair.position + new Vector3(0f, 0.35f, 0f); // yedek hizalama
        }


        animator.SetBool("isSitting", true);
        isSitting = true;

        animator.SetBool("SittingTalk", true);
        Debug.Log($"🪑 {gameObject.name} oturdu. ({myChair.name})");

        yield return new WaitForSeconds(60f);

        animator.SetBool("SittingTalk", false);
        animator.SetBool("isSitting", false);
        animator.SetTrigger("doStand");

        while (animator.GetCurrentAnimatorStateInfo(0).IsName("SitToStand"))
            yield return null;

        isSitting = false;
        isGoingToMeal = false;
        ChairManager.Instance.ReleaseChair(myChair);
        myChair = null;

        agent.Warp(transform.position);
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        
        StartCoroutine(WalkAwayThenRoam(animCtrl));

        if (animCtrl != null)
        {
            animCtrl.isExternallyControlled = false;
            animCtrl.SendMessage("StartNextAction");
        }

        Debug.Log($"🧍 {gameObject.name} yemeği bitirdi, kalktı.");
    }
    
    private IEnumerator WalkAwayThenRoam(AnimationControl animCtrl)
    {
        Vector3 walkTarget = GetRandomNavmeshPoint(3f); // 3 birim uzağa yürü
        agent.SetDestination(walkTarget);
        agent.isStopped = false;
        animator.SetBool("isWalking", true);

        while (Vector3.Distance(transform.position, walkTarget) > 1f)
            yield return null;

        agent.ResetPath();
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(0.3f); // küçük bekleme

        if (animCtrl != null)
        {
            animCtrl.isExternallyControlled = false;
            animCtrl.SendMessage("StartNextAction");
        }
    }

    public override void TakeDamage(float amount)
    {
        currentStress -= amount;
        currentStress = Mathf.Clamp(currentStress, 0, 100f);

        if (stressBar != null)
            stressBar.UpdateBar(currentStress);

        // 🔴 STRESS %50'nin altındaysa → kaç
        if (currentStress < maxStress * 0.5f)
        {
            if (isSitting)
            {
                // Kalkıp kaç!
                StartCoroutine(StandThenFlee());
            }
            else if (!isReacting)
            {
                isReacting = true;
                StopAllAnimations();
                StartCoroutine(FleeThenYell()); // zaten 2x hızla kaçar
            }
            return;
        }

        // 🟢 STRESS yüksekse → eski davranış
        if (isSitting)
        {
            animator.SetBool("SittingDodge", true);
            animator.SetBool("SittingTalk", false);
            StartCoroutine(ResetSittingDodge());
            return;
        }

        base.TakeDamage(amount);
    }
    
    private IEnumerator StandThenFlee()
    {
        Debug.Log($"{gameObject.name} oturuyordu ama stres yüksek → KAÇ!");

        animator.SetBool("SittingTalk", false);
        animator.SetBool("isSitting", false);
        animator.SetTrigger("doStand");

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("SitToStand"))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).IsName("SitToStand"))
            yield return null;

        isSitting = false;
        isGoingToMeal = false;

        if (myChair != null)
        {
            ChairManager.Instance.ReleaseChair(myChair);
            myChair = null;
        }

        agent.Warp(transform.position);
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        isReacting = true;
        StopAllAnimations();

        // Bu coroutine içinde doğrudan Flee’ye geç
        yield return StartCoroutine(FleeThenYell());
    }


    private IEnumerator ResetSittingDodge()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("SittingDodge", false);
    }

    
    
    //---------------------------------------------------------------------------------------------------------
    //-------------------------------------------GÜNLÜK YÖNERGELER---------------------------------------------
    //---------------------------------------------------------------------------------------------------------
    
    
}
