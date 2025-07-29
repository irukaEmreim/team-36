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
    private Transform myDiamond;
    private bool hasBeenRobbed = false;
    private bool isSwimming = false; // 🏊 Yüzme durumu kontrolü




    protected void Awake()
    {
        totalGuests++;
        prefersSport = Random.value < 0.65;

        if (prefersSport) sportLovers++;
        else sportAvoiders++;

      //  Debug.Log($"🧠 {name} → prefersSport: {prefersSport}");

        if (totalGuests <= 1)
            StartCoroutine(DelayedSportSummary());
    }


    protected override void Start()
    {
        
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allChildren)
        {
            if (t.name.ToLower().Contains("diamond"))
            {
                myDiamond = t;
                break;
            }
        }
        if (myDiamond != null)
           Debug.Log($"{name} → Elmas bulundu: {myDiamond.name}");
        else
            Debug.LogWarning($"{name} → Elmas bulunamadı! Kovalamayı asla başlatamaz.");




        base.Start();
        animator.applyRootMotion = false; // 💥 Hareket NavMeshAgent'ten gelsin
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
        StartCoroutine(WaitToEnableJewelryCheck());
        StartCoroutine(RandomRoamForSeconds(30f));

       


        
       
    }
  
    private IEnumerator WaitToEnableJewelryCheck()
    {
        yield return new WaitForSeconds(1f); // 1 saniye bekle
        hasBeenInitialized = true;
    }

    IEnumerator DelayedSportSummary()
    {
        yield return new WaitForSeconds(2f); // Tüm NPC’ler başlasın diye bekle
      //  Debug.Log($"🏋️ Sporcu sayısı: {sportLovers} — Spor yapmayanlar: {sportAvoiders} — Toplam misafir: {totalGuests}");
    }
    protected override NPCReactionType GetReactionType()
    {
        return NPCReactionType.Flee;
    }

    protected override void Update()
    {
        
        base.Update();

        
        if (!isSitting && (isReacting || isGoingToMeal))
            return;

        CheckCrowProximity();
        CheckIfJewelryStolen(); // 💎 bu eklendi
        Transform child = GetComponentInChildren<Transform>();
        RaycastHit hit;
        Debug.DrawLine(child.position+Vector3.up*0.5f,(child.position+Vector3.up*0.5f)+Vector3.down*2f,Color.red);
        if (Physics.Raycast(child.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
        {
            if (hit.collider.CompareTag("PoolFloor"))
            {
                if (!isSwimming)
                {
                    animator.SetBool("isSwimming", true);
                    isSwimming = true;
                    Debug.Log($"{name} yüzmeye başladı.");
                }
            }
            else
            {
                if (isSwimming)
                {
                    animator.SetBool("isSwimming", false);
                    isSwimming = false;
                    Debug.Log($"{name} yüzmeyi bıraktı.");
                }
            }
        }
    }
    void CheckIfJewelryStolen()
    {
        if (!hasBeenInitialized) return;
        if (isChasingCrowForJewelry) return;
        if (myDiamond == null) return;
        if (hasBeenRobbed) return; // 👑 YENİ: bir kez çalındıysa tekrar kontrol etme

        bool stillMine = myDiamond.IsChildOf(transform);
        if (!stillMine)
        {
            hasBeenRobbed = true; // 🔒 Artık tekrar tetiklenmesin
            Debug.Log($"{name} → Elmasım çalınmış! Kargayı kovalamaya başlıyorum.");
            OnJewelryStolen();
        }
    }
    public override void OnJewelryStolen()
    {
        if (isChasingCrowForJewelry) return;

        Debug.Log($"{name} → [GUEST] Takısı çalındı! Kargayı kovalamaya başlıyor.");

        isReacting = false;
        StopAllAnimations();

        isChasingCrowForJewelry = true;

        if (jewelryChaseRoutine != null)
            StopCoroutine(jewelryChaseRoutine);

        // GUEST’E ÖZEL VERSİYONU KULLAN!
        jewelryChaseRoutine = StartCoroutine(JewelryChase());
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
        agent.ResetPath();

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
        agent.ResetPath();

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
    
    protected override IEnumerator FleeThenYell()
    {
        Debug.Log($"{name} → [GUEST] Gaak'tan korktu, kaçıyor!");

        animator.SetBool("isRunning", true);
        agent.speed = (currentStress < maxStress * 0.5f) ? runSpeed * 2f : runSpeed;
        Vector3 target = GetRandomNavmeshPoint(15f, 7f);
        agent.SetDestination(target);

        float timeElapsed = 0f;
        float maxChaseTime = 3f;

        while (timeElapsed < maxChaseTime)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                break;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        animator.SetBool("isRunning", false);
        animator.applyRootMotion = false;

        Vector3 frozenPos = transform.position;
        agent.Warp(frozenPos);

        yield return new WaitForSeconds(0.05f);

        animator.CrossFade("AngryYelling", 0.1f);

        // ❗ STATE GEÇİŞİ BEKLE
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("AngryYelling"));
        Debug.Log($"{name} → Animator artık AngryYelling state'inde.");

        // 🔥 ARTIK gerçek süreyi al
        float yellDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log($"{name} → AngryYelling gerçek süresi: {yellDuration:F2}s");

        float elapsed = 0f;
        while (elapsed < yellDuration)
        {
            transform.position = frozenPos;
            agent.nextPosition = frozenPos;
            agent.Warp(frozenPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isReacting = false;
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        animator.applyRootMotion = false;

        Debug.Log($"{name} → [GUEST] Yell gerçekten bitti, roam başlıyor");

        StartCoroutine(ResumeRoamAfterCooldown());
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

        Bounds sportBounds = NoktaSpot.Instance.GetSportBounds();

        // 10x10'luk alanda rastgele bir hedef noktası
        Vector3 randomPos = sportBounds.center + new Vector3(
            Random.Range(-sportBounds.extents.x, sportBounds.extents.x),
            0f,
            Random.Range(-sportBounds.extents.z, sportBounds.extents.z)
        );

        bool arrived = false;

        // NavMesh kontrolü
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            yield return MoveToTarget(hit.position);
            arrived = true;
        }
        else
        {
            Debug.LogWarning($"{name} spor alanındaki hedef NavMesh dışında. Olduğu yerde spor yapacak.");
        }

        // Spor animasyonu başlat
        animator.SetBool("DoExercise", true);
        yield return new WaitForSeconds(30); // Spor süresi
        animator.SetBool("DoExercise", false);

        isBusy = false;
        Debug.Log($"{name} spor yaptı {(arrived ? "ve hedefe ulaştı" : "ama oraya varamadı")}.");
    }

    private Vector3 GetRandomPointInsideBounds(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        return new Vector3(
            Random.Range(min.x, max.x),
            transform.position.y,
            Random.Range(min.z, max.z)
        );
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
            animator.SetBool("isSwimming", true);
            yield return new WaitForSeconds(120);
            animator.SetBool("isSwimming", false);
        }
        else
        {
            // Sandalye
            Transform chair = ChairManager.Instance.GetAvailableChair();
            if (chair != null)
            {
                Debug.Log($"{name} oturmaya gidiyor.");
                yield return MoveToTarget(chair.position);
                animator.SetBool("isSitting", true);
                yield return new WaitForSeconds(120);
                animator.SetBool("isSitting", false);
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




    IEnumerator MoveToTarget(Vector3 target)
    {
        agent.ResetPath();

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(target);
        }
        else
        {
            Debug.LogWarning($"{name} → agent NavMesh'te değil! Hedef atanamadı. fonksiyon movetotarget");
        }

        animator.SetBool("isWalking", true);
        while (Vector3.Distance(transform.position, target) > 1f)
        {
            yield return null;
        }
        animator.SetBool("isWalking", false);
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PoolFloor"))
        {
            animator.SetBool("isSwimming", true);
            Debug.Log($"{name} havuza girdi. Yüzme animasyonu başladı.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PoolFloor"))
        {
            animator.SetBool("isSwimming", false);
            Debug.Log($"{name} havuzdan çıktı. Yüzme animasyonu durdu.");
        }
    }

    public override void StopChasingCrow()
    {
        base.StopChasingCrow();

        StopAllAnimations(); // 🔥 Animasyonları tam temizle
        agent.speed = normalSpeed; // 🐢 Hız normale dönsün

        isReacting = false;
        animator.SetBool("isWalking", false);
        animator.SetBool("throw", false);

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;
        agent.updateRotation = false;

        Vector3 safePos = transform.position;
        agent.Warp(safePos);

        Debug.Log($"{name} → Stres bitti veya takı bırakıldı. Normal yaşama dönüyor.");

        StartCoroutine(ResumeRoamAfterCooldown());
    }



  
    
    protected override IEnumerator JewelryChase()
    {
        GameObject crow = GameObject.FindGameObjectWithTag("lb_bird");
        if (crow == null) yield break;

        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;

        currentTarget = crow;
        agent.speed = runSpeed;
        animator.SetBool("isRunning", true);

        // 🔥 Stres düşürme ayrı başlasın
        if (stressDrainRoutine != null)
            StopCoroutine(stressDrainRoutine);
        stressDrainRoutine = StartCoroutine(DrainStressWhileChasing());

        // 1. faz: 50'nin üstündeyken sadece kovala
        while (isChasingCrowForJewelry && crow != null && currentStress > 60f)
        {
            agent.SetDestination(crow.transform.position);
            yield return null;
        }

        // 2. faz: Throw yap, stres drain devam etsin
        if (crow != null && currentStress <= 60f && currentStress > 0f)
        {
            
            agent.ResetPath();
            yield return StartCoroutine(ChaseThenThrow());
            animator.SetBool("isRunning", true);
        }

        // 3. faz: Stres sıfıra düşene kadar sadece kovala
        while (isChasingCrowForJewelry && crow != null && currentStress > 0f)
        {
            agent.SetDestination(crow.transform.position);
            yield return null;
        }

        // Durdur
        if (stressDrainRoutine != null)
            StopCoroutine(stressDrainRoutine);
        stressDrainRoutine = null;

        StopChasingCrow();
    }

    
    private Coroutine stressDrainRoutine = null;

    private IEnumerator DrainStressWhileChasing()
    {
        while (isChasingCrowForJewelry && currentStress > 0)
        {
            currentStress -= 10f;
            currentStress = Mathf.Clamp(currentStress, 0, maxStress);
            if (stressBar != null) stressBar.UpdateBar(currentStress);

            yield return new WaitForSeconds(2f);
        }
    }









 




}

    

