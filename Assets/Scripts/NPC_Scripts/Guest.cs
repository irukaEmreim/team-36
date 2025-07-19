using System;
using UnityEngine;
using System.Collections;
using NPC_Scripts;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Guest : BaseNPC
{
    private bool isSitting = false;
    private Transform myChair = null;
    private bool isGoingToMeal = false;
    private float fearDistance = 7f;
    private float fearCooldown = 3f;
    private float lastFearTime = -999f;
    private bool prefersSport;
    public static int totalGuests = 0;
    public static int sportLovers = 0;
    public static int sportAvoiders = 0;
    private Transform hipBone; // 🍑 Oturma hizalaması için
    private bool hasBeenInitialized = false;


    protected void Awake()
    {
        totalGuests++;
        prefersSport = Random.value < 0.8f;

        if (prefersSport) sportLovers++;
        else sportAvoiders++;

        Debug.Log($"🧠 {name} → prefersSport: {prefersSport}");

        if (totalGuests <= 1)
            StartCoroutine(DelayedSportSummary());
    }


    protected override void Start()
    {
        base.Start();

        // Eğer zaten atanmadıysa, burada kesin atansın
       

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
    IEnumerator DelayedSportSummary()
    {
        yield return new WaitForSeconds(2f); // Tüm NPC’ler başlasın diye bekle
        Debug.Log($"🏋️ Sporcu sayısı: {sportLovers} — Spor yapmayanlar: {sportAvoiders} — Toplam misafir: {totalGuests}");
    }
    protected override NPCReactionType GetReactionType()
    {
        return NPCReactionType.Flee;
    }

    private void Update()
    {
        if (!isSitting && (isReacting || isGoingToMeal))
            return;

      

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



    private float actionTimer;
    private bool isBusy = false;

    private void OnEnable()
    {
        GameTimeManager.OnMinuteChanged += HandleMinuteChange;
    }

    private void OnDisable()
    {
        GameTimeManager.OnMinuteChanged -= HandleMinuteChange;
    }
    
    void HandleMinuteChange(int minute)
    {
        if (isBusy) return;

        float chance = Random.value;
        if (minute == 0)
        {
            // Tüm misafirler random gezinsin
            StartCoroutine(RandomRoamForSeconds(60));
        }
        else if (minute == 1)
        {
            if (prefersSport)
            {
                Debug.Log($"{name} sporcu olarak seçildi.");
                StartCoroutine(GoToSport());
            }
            else
            {
                Debug.Log($"{name} sporu sevmedi, dolaşmaya çıktı.");
                StartCoroutine(RandomRoamForSeconds(60));
            }

            Debug.Log($"📊 Şu anki istatistik: Toplam Guest: {totalGuests} — Sporcular: {sportLovers}");
        }
        if (minute == 2)
        {
            if (Random.Range(0f, 1f) <= 0.8f)
                StartCoroutine(GoToBreakfast());
            else
                StartCoroutine(RandomRoamForSeconds(60));
        }
        else if (minute >= 3 && minute < 5)
        {
            if (chance < 0.8f)
                StartCoroutine(GoToPoolOrSit());
            else
                StartCoroutine(RandomRoamForSeconds(120));
        }
        else if (minute == 5)
        {
            StartCoroutine(GoToLunch());
        }
        else if (minute == 6)
        {
            StartCoroutine(GoToPoolOrSit());
        }
        else if (minute == 9)
        {
            StartCoroutine(GoToDinner());
        }
        else if (minute == 10)
        {
            StartCoroutine(GoInside());
        }
    }
    IEnumerator GoToSport()
    {
        isBusy = true;
        Debug.Log($"{name} spora gidiyor.");
        Vector3 sportArea = NoktaSpot.Instance.GetSportArea();
        yield return MoveToTarget(sportArea);

        animator.SetBool("DoExercise", true);
        yield return new WaitForSeconds(30); // Spor süresini kısalt
        animator.SetBool("DoExercise", false);

        isBusy = false; // Kritik nokta: erken bırak
        Debug.Log($"{name} spor yaptı, serbest.");
    }

    IEnumerator GoToBreakfast()
    {
        isBusy = true;
        Debug.Log($"{name} kahvaltıya gidiyor.");

        var animCtrl = GetComponent<AnimationControl>();
        if (animCtrl != null)
            animCtrl.isExternallyControlled = true;

        // Mevcut oturma sistemini kullan:
        yield return StartCoroutine(GoSitAndEatRoutine());

        if (animCtrl != null)
        {
            animCtrl.isExternallyControlled = false;
            animCtrl.SendMessage("StartNextAction");
        }

        isBusy = false;
    }

    IEnumerator GoToLunch() => GoToBreakfast(); // aynı yapı

    IEnumerator GoToDinner() => GoToBreakfast();

    IEnumerator GoToPoolOrSit()
    {
        isBusy = true;
        if (Random.value < 0.5f)
        {
            // Havuz
            Debug.Log($"{name} yüzmeye gidiyor.");
            Vector3 poolPos = NoktaSpot.Instance.GetPoolSpot();
            yield return MoveToTarget(poolPos);
            animator.SetBool("DoSwim", true);
            yield return new WaitForSeconds(120);
            animator.SetBool("DoSwim", false);
        }
        else
        {
            // Sandalye
            Transform chair = ChairManager.Instance.GetAvailableChair();
            if (chair != null)
            {
                Debug.Log($"{name} oturmaya gidiyor.");
                yield return MoveToTarget(chair.position);
                animator.SetBool("DoSit", true);
                yield return new WaitForSeconds(120);
                animator.SetBool("DoSit", false);
                ChairManager.Instance.ReleaseChair(chair);
            }
        }

        isBusy = false;
    }

    IEnumerator GoInside()
    {
        isBusy = true;
        Debug.Log($"{name} otele giriyor.");
        Vector3 lobby = NoktaSpot.Instance.GetIndoorArea();
        yield return MoveToTarget(lobby);
        isBusy = false;
    }

    IEnumerator RandomRoamForSeconds(float duration)
    {
        isBusy = true;
        float timer = duration;

        while (timer > 0)
        {
            Vector3 randomSpot = GetRandomNavmeshPoint(8f);
            agent.SetDestination(randomSpot);
            animator.SetBool("isWalking", true);

            // Yeni hedefe ulaşana kadar bekle (ama çok da takılmasın)
            while (agent.pathPending || agent.remainingDistance > 0.8f)
            {
                // Eğer engelde takılırsa → tekrar yeni hedef
                if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.remainingDistance >= 20f)
                {
                    Debug.Log($"{name} → Patikada sorun! Yeni rota deneniyor...");
                    break; // yeni hedefe geç
                }

                yield return null;
            }

            // Çok kısa hareket ettiyse → biraz zaman harcasın
            float waitTime = Random.Range(1f, 2f);
            yield return new WaitForSeconds(waitTime);
            timer -= waitTime;
        }

        animator.SetBool("isWalking", false);
        isBusy = false;
    }

    IEnumerator MoveToTarget(Vector3 target)
    {
        agent.SetDestination(target);
        animator.SetBool("isWalking", true);
        while (Vector3.Distance(transform.position, target) > 1f)
        {
            yield return null;
        }
        animator.SetBool("isWalking", false);
    }
}

    

