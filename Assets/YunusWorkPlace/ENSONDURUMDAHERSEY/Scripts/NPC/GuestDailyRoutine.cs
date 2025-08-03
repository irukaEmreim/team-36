using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GuestDailyRoutine : MonoBehaviour
{
    // 11 dakika = 660 saniye
    private const float DAY_DURATION = 660f;
    public float timer = 0f;
    private float speed = 3.5f;
    private float runningSpeed = 5f;
    public enum Phase
    {
        WanderMorning,  // 0–1
        Workout,        // 1–2
        Breakfast,      // 2–3
        Swim,           // 3–4
        WanderAfternoon,// 4–6
        Dinner,         // 6–7
        Snack,          // 7–9
        WanderEvening,  // 9–11
        Ended
    }


    [Header("Diamond Reaction")]
    public float diamondChaseDuration = 8f;      // kovalamaca süresi
    public float diamondThrowDistance = 5f;      // bu mesafeye gelince ilk atış
    private bool isDiamondChasing = false;
    void Start()
    {
        animator.SetBool("doStand", false);
        // Başlangıçta sabah yürüyüşünü başlat
        if (currentPhase == Phase.WanderMorning)
            wanderCoroutine = StartCoroutine(WanderMorningRoutine());
    }


    public Phase currentPhase = Phase.WanderMorning;
    public Phase prevPhase = Phase.WanderMorning;

    private bool done = false;

    void Update()
    {
        print(stressComponent.currentStress);

        timer += Time.deltaTime;
        if (done) return;

        if (timer < 60f)
        {
            SetPhase(Phase.WanderMorning, "0–1 dk: Etrafta rastgele yürüyorlar.");
            print("0–1 dk: Etrafta rastgele yürüyorlar.");
        }

        else if (timer < 120f)
        {
            SetPhase(Phase.Workout, "1–2 dk: Spor alanına gidip spor animasyonu oynuyorlar.");
            print("1–2 dk: Spor alanına gidip spor animasyonu oynuyorlar.");
        }

        else if (timer < 180f)
        {
            SetPhase(Phase.Breakfast, "2–3 dk: Restorana gidip sandalyeye oturup kahvaltı yapıyorlar.");
            print("2–3 dk: Restorana gidip sandalyeye oturup kahvaltı yapıyorlar.");
            //animator.SetBool("DoStand", false);

        }

        else if (timer < 240f)
        {
            SetPhase(Phase.Swim, "3–4 dk: Havuz (PoolFloor) üzerinde yüzme animasyonu çalışıyor.");
            print("3–4 dk: Havuz (PoolFloor) üzerinde yüzme animasyonu çalışıyor.");
        }
        else if (timer < 300f)
        {
            SetPhase(Phase.Dinner, " Bu da öğle  yemeği olsun");
            print("4–5 dk: Öğle yemeği.");
        }
        else if (timer < 360f)
        {
            SetPhase(Phase.WanderAfternoon, "4–6 dk: Etrafta rastgele yürüyorlar.");
            print("5–6 dk: Etrafta rastgele yürüyorlar.");
        }
        else if (timer < 420f)
        {
            SetPhase(Phase.Dinner, "6–7 dk: Restorana dönüp akşam yemeği.");
            print("6–7 dk: Restorana dönüp akşam yemeği.");
        }
        else if (timer < 480f) {
            SetPhase(Phase.Swim,"7-8 dk : Yüzme");
        }
        else if (timer < 540f)
        {
            SetPhase(Phase.Snack, "7–9 dk: Atıştırmalık alanında oturuyorlar.");
            print("7–9 dk: Atıştırmalık alanında oturuyorlar.");
        }
        else if (timer < 660f)
        {
            SetPhase(Phase.WanderEvening, "9–11 dk: Etrafta rastgele yürüyorlar.");
            print("9–11 dk: Etrafta rastgele yürüyorlar.");
        }

        else
        {
            done = true;
            //currentPhase = Phase.Ended;
            SetPhase(Phase.Ended, "Gün sona erdi otele dönülüyor.");
            Debug.Log("11 dk: Gün tamamlandı, otele gidiyorlar ve yok oluyorlar.");
            // İster burada Destroy(gameObject) ile yok edebilirsin:
            // Destroy(gameObject);
        }

        UpdateAnimations();

        CheckCrowProximity();

    }

    private void SetPhase(Phase newPhase, string message)
    {
        if (currentPhase == newPhase) return;
        prevPhase = currentPhase;
        currentPhase = newPhase;
        Debug.Log(message);

        // SABAH YÜRÜYÜŞÜ KISMI
        if (newPhase == Phase.WanderMorning) // sabah yürüyüşü çalıştır
        {
            if (wanderCoroutine != null)    // öncekini durdur
            {
                StopCoroutine(wanderCoroutine);
            }
            wanderCoroutine = StartCoroutine(WanderMorningRoutine());
        }
        else
        {
            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);
                wanderCoroutine = null;
            }
        }
        // SABAH YÜRÜYÜŞÜ KISMI
        // SABAH SPORU KISMI
        if (newPhase == Phase.Workout)
        {
            // Önceki faz coroutineleri iptal et
            if (workoutCoroutine != null) StopCoroutine(workoutCoroutine);
            if (skipCoroutine != null) StopCoroutine(skipCoroutine);

            if (Random.value <= workoutChance)
            {
                // %80 ihtimalle spor
                workoutCoroutine = StartCoroutine(WorkoutRoutine());
            }
            else
            {
                // %20 ihtimalle atla, sadece rastgele dolaş
                Debug.Log("Workout atlandı, sadece dolaşılacak");
                skipCoroutine = StartCoroutine(WanderSkipRoutine(60f));
            }
        }
        else if (workoutCoroutine != null || skipCoroutine != null)
        {
            // Faz değişince durdur
            if (workoutCoroutine != null) StopCoroutine(workoutCoroutine);
            if (skipCoroutine != null) StopCoroutine(skipCoroutine);
            workoutCoroutine = null;
            skipCoroutine = null;
            animator.SetBool("DoExercise", false);
        }

        // SABAH SPORU KISMI


        // KAHVALTI KISMI
        if (newPhase == Phase.Breakfast)
        {
            if (breakfastCoroutine != null) StopCoroutine(breakfastCoroutine);
            breakfastCoroutine = StartCoroutine(BreakfastRoutine());
        }
        else if (breakfastCoroutine != null)
        {
            StopCoroutine(breakfastCoroutine);
            breakfastCoroutine = null;
        }
        if (prevPhase == Phase.Breakfast && newPhase != Phase.Breakfast)
        {
            if (sittindDodgeRoutine != null) StopCoroutine(sittindDodgeRoutine);
            CleanupBreakfast();
            // ayrıca coroutine’i kill etmek istiyorsan:
            if (breakfastCoroutine != null)
            {
                StopCoroutine(breakfastCoroutine);
                breakfastCoroutine = null;
            }
        }

        // KAHVALTI KISMI
        // YÜZME KISMI

        if (newPhase == Phase.Swim)
        {
            if (swimCoroutine != null) StopCoroutine(swimCoroutine);
            if (skipCoroutine != null) StopCoroutine(skipCoroutine);

            if (Random.value <= swimChance)
            {
                swimCoroutine = StartCoroutine(SwimRoutine());
            }
            else
            {
                Debug.Log("Swim atlandı, sadece dolaşılacak");
                skipCoroutine = StartCoroutine(WanderSkipRoutine(60f));
            }

        }
        else if (swimCoroutine != null || skipCoroutine != null)
        {
            if (swimCoroutine != null) StopCoroutine(swimCoroutine);
            if (skipCoroutine != null) StopCoroutine(skipCoroutine);
            swimCoroutine = null;
            skipCoroutine = null;
        }


        // YÜZME KISMI

        // YÜZME SONRASI YÜRÜYÜŞ KISMI

        if (newPhase == Phase.WanderAfternoon)
        {
            animator.SetBool("isSwimming", false);
            if (wanderAfternoonCoroutine != null) StopCoroutine(wanderAfternoonCoroutine);
            wanderAfternoonCoroutine = StartCoroutine(WanderAfternoonRoutine());
        }
        else if (wanderAfternoonCoroutine != null)
        {
            StopCoroutine(wanderAfternoonCoroutine);
            wanderAfternoonCoroutine = null;
        }
        // YÜZME SONRASI YÜRÜYÜŞ KISMI

        // AKŞAM YEMEĞİ KISMII

        if (newPhase == Phase.Dinner)
        {
            if (dinnerCoroutine != null) StopCoroutine(dinnerCoroutine);
            dinnerCoroutine = StartCoroutine(DinnerRoutine());
        }
        else if (dinnerCoroutine != null)
        {
            StopCoroutine(dinnerCoroutine);
            dinnerCoroutine = null;
        }
        if (prevPhase == Phase.Dinner && newPhase != Phase.Dinner)
        {
            CleanupBreakfast();
            // ayrıca coroutine’i kill etmek istiyorsan:
            if (dinnerCoroutine != null)
            {
                StopCoroutine(dinnerCoroutine);
                dinnerCoroutine = null;
            }
        }

        // AKŞAM YEMEĞİ KISMII
        // ATIŞTIRMA KISMIII
        if (newPhase == Phase.Snack)
        {
            if (snackCoroutine != null) StopCoroutine(snackCoroutine);
            snackCoroutine = StartCoroutine(SnackRoutine());
        }
        else if (snackCoroutine != null)
        {
            StopCoroutine(snackCoroutine);
            snackCoroutine = null;
        }

        // ATIŞTIRMA KISMIII

        // AKŞAM ETRAFTA DOLAŞMA KISMIII

        if (newPhase == Phase.WanderEvening)
        {
            // Önceki varsa iptal et
            if (wanderEveningCoroutine != null) StopCoroutine(wanderEveningCoroutine);
            wanderEveningCoroutine = StartCoroutine(WanderEveningRoutine());
        }
        else if (wanderEveningCoroutine != null)
        {
            StopCoroutine(wanderEveningCoroutine);
            wanderEveningCoroutine = null;
        }


        // AKŞAM ETRAFTA DOLAŞMA KISMIII
        // OTELE DÖNME KISMII
        if (newPhase == Phase.Ended)
        {
            Debug.Log(">>> EndOfDay fazı tetiklendi, kapıya gidiliyor");
            if (endOfDayCoroutine != null) StopCoroutine(endOfDayCoroutine);
            endOfDayCoroutine = StartCoroutine(EndOfDayRoutine());
        }
        else if (endOfDayCoroutine != null)
        {
            StopCoroutine(endOfDayCoroutine);
            endOfDayCoroutine = null;
        }
        // OTELE DÖNME KISMII
    }

    // GEREKLİ DEĞİŞKEN TANIMLAMALARIIIIIIIII

    private NavMeshAgent agent;
    private Animator animator;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stressComponent = GetComponent<NPC_Base_Test>();
        stoneThrower = GetComponent<StoneThrower>();
    }


    // SABAH YÜRÜYÜŞÜÜÜÜÜÜÜ
    private Coroutine wanderCoroutine;
    private IEnumerator WanderMorningRoutine()
    {
        agent.ResetPath();
        // Bu döngü, faz değiştiğinde veya 60s geçince kendiliğinden sonlanacak
        while (currentPhase == Phase.WanderMorning && timer < 60f)
        {

            // 1) Rastgele bir nokta seç
            Vector3 randomOffset = Random.insideUnitSphere;
            randomOffset.y = 0;
            float distance = Random.Range(15f, 50f);
            Vector3 samplePos = transform.position + randomOffset.normalized * distance;

            // 2) Yukarıdan aşağı raycast
            RaycastHit hit;
            Vector3 rayOrigin = samplePos + Vector3.up * 10f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f)
                && hit.collider.CompareTag("Ground"))
            {
                Vector3 targetPoint = hit.point;

                // 3) NavMeshAgent ile git
                agent.SetDestination(targetPoint);

                // 4) Varana kadar bekle
                yield return new WaitUntil(() =>
                    !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance);

                // 5) 2–3 saniye orada bekle
                float waitTime = Random.Range(2f, 3f);
                yield return new WaitForSeconds(waitTime);
            }

            else
            {
                // Eğer raycast başarısızsa hemen bir sonraki karede tekrar dene
                yield return null;
            }
        }
        // Faz dışına çıkıldığında veya süre dolunca buraya gelir ve rutin biter
    }


    ///// SPOR KISMI İÇİN DEĞİŞKENLER
    [Header("Workout Phase Settings")]
    public Transform workoutAreaCenter;    // Atayın inspector’dan
    public float workoutAreaRadius = 7f;   // Yarıçap
    public float minWorkoutTime = 5f, maxWorkoutTime = 10f;

    private Coroutine workoutCoroutine;
    private IEnumerator WorkoutRoutine()
    {
        Debug.Log(">>> WorkoutRoutine başladı");
        agent.ResetPath();

        // 1) Alan içinde tek bir kez rasgele nokta
        Vector2 rnd2D = Random.insideUnitCircle * workoutAreaRadius;
        Vector3 samplePos = workoutAreaCenter.position + new Vector3(rnd2D.x, 0, rnd2D.y);

        // 2) Geçerli NavMesh pozisyonu bul
        NavMeshHit navHit;
        if (!NavMesh.SamplePosition(samplePos, out navHit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Workout: NavMesh üzerinde nokta bulunamadı!");
            yield break;
        }

        // 3) Hedefi ayarla ve git
        print("Gitmeli şu an");
        agent.SetDestination(navHit.position);
        yield return new WaitUntil(() => agent.pathPending == false);
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        Debug.Log("Workout: Spor alanına varıldı");

        // 4) Faz bitene kadar aynı konumda animasyonu tetikle
        while (currentPhase == Phase.Workout && Vector3.Distance(transform.position, workoutAreaCenter.transform.position) < 7f)
        {
            if (!isFleeing)
            {
                animator.SetBool("DoExercise", true);
            }
            // animasyon aralıkları için rasgele bekleme
            float t = Random.Range(minWorkoutTime, maxWorkoutTime);
            yield return new WaitForSeconds(t);
        }

        Debug.Log("<<< WorkoutRoutine bitti");
        animator.SetBool("DoExercise", false);
    }

    //// KAHVALTI KISMIIII
    private Coroutine breakfastCoroutine;
    private Chair chosenChair;     // <— Burayı ekle

    private IEnumerator BreakfastRoutine()
    {
        animator.SetBool("doStand", false);
        Debug.Log(">>> Breakfast fazı başladı");

        // 1) Tüm sandalyeleri çek
        var allChairs = FindObjectsOfType<Chair>();

        // 2) Boş bir sandalye bulana kadar bekle
        while (currentPhase == Phase.Breakfast && chosenChair == null)
        {
            // Boş olanları filtrele
            var freeChairs = System.Array.FindAll(allChairs, c => !c.IsOccupied);
            if (freeChairs.Length > 0)
            {
                // Random seç
                int idx = Random.Range(0, freeChairs.Length);
                if (freeChairs[idx].TryOccupy())
                    chosenChair = freeChairs[idx];
            }
            if (chosenChair == null)
                yield return new WaitForSeconds(0.5f);
        }

        if (chosenChair == null)
        {
            Debug.LogWarning("Breakfast: Hiç boş sandalye yok!");
            yield break;
        }

        // 3) Sandalyenin seatPoint’una git
        Vector3 target = chosenChair.seatPoint.position;
        agent.ResetPath();
        agent.SetDestination(target);

        // Gidişi bekle
        yield return new WaitUntil(() => agent.pathPending == false);
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

        agent.ResetPath();
        agent.enabled = false;
        //sittingCollider.enabled = true;
        //standingCollider.enabled = false;

        Vector3 sitPos = chosenChair.seatPoint.position;
        Quaternion sitrot = chosenChair.seatPoint.rotation;
        transform.position = sitPos;
        transform.rotation = sitrot;

        // 4 Oturma animasyonunu başlat
        animator.SetBool("DoStand", false);
        animator.SetBool("isSitting", true);




        // 5) Faz bitene kadar bekle (2–3 dk arası)
        while (currentPhase == Phase.Breakfast)
            yield return null;

        // 6) Faz değişince kalk
        print("isSittingFalseOlmalıııı");
        animator.SetBool("isSitting", false);
        animator.SetBool("doStand", true);
        agent.enabled = true;

        chosenChair.Vacate();

        Debug.Log("<<< Breakfast fazı bitti");
    }

    private void CleanupBreakfast()
    {
        Debug.Log(">>> Breakfast temizliği (kalkma) yapılıyor");
        animator.SetBool("isSitting", false);
        animator.SetBool("doStand", true);
        agent.enabled = true;
        if (chosenChair != null)
        {
            chosenChair.Vacate();
            chosenChair = null;
        }
    }

    //// YÜZME KISMIIII

    [Header("Swim Phase Settings")]
    public Transform swimAreaCenter;    // Inspector’dan atayacağız
    public float swimAreaRadius = 7f;   // Yarıçap (örneğin 5 metre)
    public string poolFloorTag = "PoolFloor"; // Havuz zemini için Tag
    public float minSwimTime = 5f, maxSwimTime = 9f; // Bir noktada kalma süresi

    public float navSampleMaxDistance = 1f;  // NavMesh.SamplePosition en çok bu kadar yakın bakar

    private Coroutine swimCoroutine;

    private IEnumerator SwimRoutine()
    {
        Debug.Log(">>> Swim fazı başladı");
        agent.ResetPath();
        yield return new WaitForSeconds(3.5f);

        while (currentPhase == Phase.Swim)
        {
            Vector3 targetPoint = Vector3.zero;
            bool foundValid = false;

            // En fazla 50 kez dene
            for (int i = 0; i < 50; i++)
            {
                // 1) Havuz çevresinde rasgele bir nokta
                Vector2 rnd2D = Random.insideUnitCircle * swimAreaRadius;
                Vector3 samplePos = swimAreaCenter.position + new Vector3(rnd2D.x, 0, rnd2D.y);

                // 2) Raycast ile "PoolFloor" zemine denk gelen nokta
                RaycastHit hit;
                Vector3 rayOrigin = samplePos + Vector3.up * 1f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2f)
                    && hit.collider.CompareTag(poolFloorTag))
                {
                    // 3) O noktayı NavMesh üzerinde sample et
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(
                            hit.point,
                            out navHit,
                            /*maxDistanceToNavMesh=*/ 1.0f,
                            NavMesh.AllAreas))
                    {
                        targetPoint = navHit.position;
                        foundValid = true;
                        Debug.Log($"Swim: {i + 1}. denemede geçerli nokta bulundu: {targetPoint}");
                        break;
                    }
                    else
                    {
                        Debug.Log($"Swim: {i + 1}. deneme — raycast OK, ama NavMesh yok");
                    }
                }
                else
                {
                    Debug.Log($"Swim: {i + 1}. deneme — raycast PoolFloor tag’iyle eşleşmedi");
                }
            }

            if (!foundValid)
            {
                Debug.LogWarning("Swim: 50 deneme de geçerli nokta bulamadı, tekrar denenecek");
                yield return null;
                continue;
            }

            // 4) Geçerli hedef bulundu → yürü
            agent.SetDestination(targetPoint);
            yield return new WaitUntil(() => agent.pathPending == false);
            yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

            // 5) Yüzme animasyonunu başlat
            animator.SetBool("isSwimming", true);

            // 6) Rasgele süre bekle
            float waitTime = Random.Range(minSwimTime, maxSwimTime);
            yield return new WaitForSeconds(waitTime);

            // (İstersen burda animator.SetBool("isSwimming", false); yapabilirsin)
        }

        // Faz bitince animasyonu kapat
        Debug.Log("<<< Swim fazı bitti");
        animator.SetBool("isSwimming", false);
    }



    //// YÜZME KISMIIII

    /// YÜRÜYÜŞ KISMIIII
    [Header("Wander Afternoon Settings")]
    public float afternoonWanderRadius = 40f;    // Dolaşacakları yarıçap (tercihe göre 15–50 gibi)
    public float minAfternoonWait = 1f, maxAfternoonWait = 3f; // Noktada ne kadar bekleyecekler
    public string groundTag = "Ground";          // Zemin tag’i (WanderMorning’da kullandığınla aynı olabilir)
    private Coroutine wanderAfternoonCoroutine;

    private IEnumerator WanderAfternoonRoutine()
    {
        Debug.Log(">>> WanderAfternoon fazı başladı");
        agent.ResetPath();

        // 4–6 dk arası yürütecek coroutine, faz değişince Stop edilir
        while (currentPhase == Phase.WanderAfternoon)
        {
            // 1) Rastgele bir nokta seç (kirli zemin kontrolü için raycast kullanıyoruz)
            Vector3 randomOffset = Random.insideUnitSphere;
            randomOffset.y = 0;
            float distance = Random.Range(afternoonWanderRadius * 0.2f, afternoonWanderRadius);
            Vector3 samplePos = transform.position + randomOffset.normalized * distance;

            // 2) Yukarıdan aşağı raycast at, zemini kontrol et
            RaycastHit hit;
            Vector3 rayOrigin = samplePos + Vector3.up * 10f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f)
                && hit.collider.CompareTag(groundTag))
            {
                Vector3 target = hit.point;

                // 3) NavMesh üzerinde sample edelim, çok küçük ofsetle
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(target, out navHit, 1f, NavMesh.AllAreas))
                {
                    target = navHit.position;

                    // 4) Gidişi başlat
                    agent.SetDestination(target);
                    yield return new WaitUntil(() => !agent.pathPending);
                    yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

                    // 5) Varınca biraz bekle
                    float wait = Random.Range(minAfternoonWait, maxAfternoonWait);
                    yield return new WaitForSeconds(wait);
                }
                else
                {
                    // eğer NavMesh yoksa, hemen bir frame bekle
                    yield return null;
                }
            }
            else
            {
                // raycast zemini tutmadıysa
                yield return null;
            }
        }

        // Faz bitince
        Debug.Log("<<< WanderAfternoon fazı bitti");
    }




    /// YÜRÜYÜŞ KISMIII

    /// AKŞAM YEMEĞİ KISMIIII
    private Coroutine dinnerCoroutine;

    private IEnumerator DinnerRoutine()
    {
        Debug.Log(">>> Dinner fazı başladı");
        animator.SetBool("doStand", false);

        // 1) Sahnedeki tüm Chair’ları bul
        var allChairs = FindObjectsOfType<Chair>();
        Chair chosen = null;

        // 2) Boş sandalye bulunana kadar dene
        while (currentPhase == Phase.Dinner && chosen == null)
        {
            var free = System.Array.FindAll(allChairs, c => !c.IsOccupied);
            if (free.Length > 0)
            {
                int idx = Random.Range(0, free.Length);
                if (free[idx].TryOccupy())
                    chosen = free[idx];
            }
            if (chosen == null)
                yield return new WaitForSeconds(0.5f);
        }

        if (chosen == null)
        {
            Debug.LogWarning("Dinner: Hiç boş sandalye yok!");
            yield break;
        }

        // 3) Sandalyeye git
        agent.ResetPath();
        agent.SetDestination(chosen.seatPoint.position);
        yield return new WaitUntil(() => !agent.pathPending);
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

        // 4) Otur
        agent.enabled = false;
        transform.position = chosen.seatPoint.position;
        transform.rotation = chosen.seatPoint.rotation;
        animator.SetBool("DoStand", false);
        animator.SetBool("isSitting", true);

        // 5) Faz bitene kadar bekle (360–420s)
        while (currentPhase == Phase.Dinner)
            yield return null;

        // 6) Faz bitince kalk ve temizlik
        Debug.Log("<<< Dinner fazı bitiyor, cleanup yapılıyor");
        animator.SetBool("isSitting", false);
        animator.SetBool("doStand", true);
        agent.enabled = true;
        chosen.Vacate();

        Debug.Log("<<< Dinner fazı bitti");
    }


    /// AKŞAM YEMEĞİ KISMIIII
    /// 
    /// ATIŞTIRMALIK KISMII

    [Header("Snack (7–9 dk) Settings")]
    public Transform snackAreaCenter;     // Sahnedeki Snack alanının ortasındaki boş GO
    public float snackAreaRadius = 10f;   // Kaç metre içinde dolaşacaklar
    public float snackMinWait = 2f;       // Noktaya varınca en az bekleme süresi
    public float snackMaxWait = 5f;       // Noktaya varınca en çok bekleme süresi
    private Coroutine snackCoroutine;


    private IEnumerator SnackRoutine()
    {
        Debug.Log(">>> Snack fazı başladı");
        agent.ResetPath();

        yield return new WaitForSeconds(3.5f);
        // currentPhase değişene kadar dön
        while (currentPhase == Phase.Snack)
        {
            // 1) Rastgele bir yön ve mesafe seç
            Vector2 rnd2D = Random.insideUnitCircle * snackAreaRadius;
            Vector3 samplePos = snackAreaCenter.position + new Vector3(rnd2D.x, 0, rnd2D.y);

            // 2) Yukarıdan aşağı raycast atarak zemini kontrol et
            RaycastHit hit;
            Vector3 rayOrigin = samplePos + Vector3.up * 10f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f)
                && hit.collider.CompareTag(groundTag))
            {
                Vector3 target = hit.point;

                // 3) NavMesh üzerinde yakın bir nokta sample et
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(target, out navHit, 1f, NavMesh.AllAreas))
                {
                    target = navHit.position;

                    // 4) NavMeshAgent ile git
                    agent.SetDestination(target);

                    // Yol hesaplanana kadar bekle
                    yield return new WaitUntil(() => !agent.pathPending);
                    // Hedefe varana kadar bekle
                    yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

                    // 5) Varınca rastgele 2–5 saniye bekle
                    float waitTime = Random.Range(snackMinWait, snackMaxWait);
                    yield return new WaitForSeconds(waitTime);
                }
                else
                {
                    // NavMesh yoksa bir frame bekle, sonra yeniden dene
                    yield return null;
                }
            }
            else
            {
                // Zemin tag’i tutmadıysa bir frame bekle
                yield return null;
            }
        }

        // Faz sonu temizliği
        Debug.Log("<<< Snack fazı bitti");
    }


    /// ATIŞTIRMALIK KISMII

    /// SON AKŞAM YÜRÜYÜŞÜ KISMII

    [Header("Wander Evening Settings")]
    public float eveningWanderRadius = 40f;    // Dolaşacakları yarıçap (bütün otel/alanı kapsayacak şekilde)
    public float eveningMinWait = 1f;          // Noktada en az bekleme süresi
    public float eveningMaxWait = 4f;          // Noktada en çok bekleme süresi
    private Coroutine wanderEveningCoroutine;

    private IEnumerator WanderEveningRoutine()
    {
        Debug.Log(">>> WanderEvening fazı başladı");
        agent.ResetPath();

        while (currentPhase == Phase.WanderEvening)
        {
            // 1) Rastgele bir nokta oluştur
            Vector2 rnd2D = Random.insideUnitCircle * eveningWanderRadius;
            Vector3 samplePos = transform.position + new Vector3(rnd2D.x, 0, rnd2D.y);

            // 2) Raycast ile zemini kontrol et
            RaycastHit hit;
            Vector3 rayOrigin = samplePos + Vector3.up * 1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2f)
                && hit.collider.CompareTag(groundTag))
            {
                Vector3 target = hit.point;

                // 3) NavMesh.SamplePosition ile yürünebilir bir nokta bul
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(target, out navHit, 1f, NavMesh.AllAreas))
                {
                    target = navHit.position;

                    // 4) NavMeshAgent ile git
                    agent.SetDestination(target);
                    yield return new WaitUntil(() => !agent.pathPending);
                    yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

                    // 5) Varınca rasgele bir süre bekle
                    float wait = Random.Range(eveningMinWait, eveningMaxWait);
                    yield return new WaitForSeconds(wait);
                }
                else
                {
                    // NavMesh üzerinde nokta yoksa, yalnızca bir kare bekle
                    yield return null;
                }
            }
            else
            {
                // Zemin tag’i uyuşmazsa
                yield return null;
            }
        }

        Debug.Log("<<< WanderEvening fazı bitti");
    }


    /// SON AKŞAM YÜRÜYÜŞÜ KISMII

    /// OTELE DÖNÜP YOK OLMA KISMII
    [Header("End of Day Settings")]
    public Transform doorTransform;        // Sahnedeki Otel Kapısı objesinin Transform’u
    public float doorStoppingDistance = 1f; // Kapıya ne kadar yaklaştığında durup yok olacak

    private Coroutine endOfDayCoroutine;

    private IEnumerator EndOfDayRoutine()
    {
        // 1) Agent reset ve hedef kapı
        agent.ResetPath();
        agent.SetDestination(doorTransform.position);
        animator.SetBool("isWalking", true); // yoksa kaldırabilirsin

        // 2) Kapıya varana dek bekle
        yield return new WaitUntil(() =>
            !agent.pathPending &&
            agent.remainingDistance <= doorStoppingDistance);

        Debug.Log("NPC kapıya ulaştı, yok oluyor…");

        // 3) Yok et / devre dışı bırak
        Destroy(gameObject, 1f);
        // veya: gameObject.SetActive(false);
    }

    /// OTELE DÖNÜP YOK OLMA KISMII

    /// ŞANS DURUMUNDA ETRAFTA NORMAL DOLAŞMASI
    [Header("Chance Settings")]
    [Range(0f, 1f)] public float workoutChance = 0.8f;
    [Range(0f, 1f)] public float swimChance = 0.8f;
    private Coroutine skipCoroutine;
    private IEnumerator WanderSkipRoutine(float duration)
    {
        float startTime = timer;
        agent.ResetPath();

        while (timer < startTime + duration)
        {
            // wander logic (aynı WanderMorning’den kopyala)
            Vector3 offset = Random.insideUnitSphere;
            offset.y = 0;
            float dist = Random.Range(15f, 50f);
            Vector3 sample = transform.position + offset.normalized * dist;

            RaycastHit hit;
            if (Physics.Raycast(sample + Vector3.up * 10f, Vector3.down, out hit, 20f)
                && hit.collider.CompareTag("Ground"))
            {
                agent.SetDestination(hit.point);
                yield return new WaitUntil(() => !agent.pathPending);
                yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
                yield return new WaitForSeconds(Random.Range(2f, 3f));
            }
            else
            {
                yield return null;
            }
        }
    }
    /// ŞANS DURUMUNDA ETRAFTA NORMAL DOLAŞMASI

    /// Animasyon Kısmı
    [Header("Animation Ground Check")]
    public float swimRayHeight = 1f;      // Karakterin ayak hizasından yukarıda
    public float swimRayDistance = 2f;    // aşağıya atılacak mesafe

    private void UpdateAnimations()
    {
        if (isSittingDodging)
        {
            return;
        }
        Vector3 rayOrigin = transform.position + Vector3.up * swimRayHeight;
        RaycastHit hit;
        bool onPool = Physics.Raycast(rayOrigin, Vector3.down, out hit, swimRayDistance) && hit.collider.CompareTag(poolFloorTag);

        if (onPool)
        {
            // 2) Eğer poolFloor altında ise kesinlikle yüzme animasyonu
            animator.SetBool("isSwimming", true);
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            return;
        }

        animator.SetBool("isSwimming", false);


        // 4) Hıza bakarak idle/walk/run üçlüsünden birini seç
        float speed = agent.velocity.magnitude;

        bool isIdle = speed < 0.1f;
        bool isWalking = speed >= 0.1f && speed < 4.5f;
        bool isRunning = speed >= 4.5f;

        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);

        if (isFleeing)
        {
            print("False edilmeli");
            animator.SetBool("DoExercise", false);
        }
    }


    [Header("Crow Interaction Settings")]
    public Transform crowTransform;         // Sahnedeki karga objesinin Transform’u
    public float crowFleeDistance = 15f;    // Bu mesafeden daha yakınsa tepki verecek

    public float fleeDistance = 50f;       // Kargadan kaçarken gideceği mesafe
    public float stressThreshold = 50f;    // Stress >= bu değerse AngryYelling
    public float sittingDodgeDistance = 5f;// Otururken bu mesafeye gelirse dodge

    // İçeride tutacağımız component ve flag’ler
    private NPC_Base_Test stressComponent;
    private bool isFleeing = false;
    private bool isSittingDodging = false;

    public bool isAttacked = false;
    public void CheckCrowProximity()
    {
        Debug.Log("▶ CheckCrowProximity çağrıldı");
        if (crowTransform == null)
        {
            Debug.LogWarning("▶ crowTransform atanmamış!");
            return;
        }
        if (isFleeing)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, crowTransform.position);

        // Otururken dodge
        if (animator.GetBool("isSitting")
            && dist < sittingDodgeDistance
            && !isSittingDodging)
        {
            Debug.Log("   → SittingDodge tetiklendi");
            sittindDodgeRoutine = StartCoroutine(SittingDodgeRoutine());
            return;
        }
        else if ((dist < crowFleeDistance || isAttacked) && !isFleeing && !animator.GetBool("isSitting"))
        {

            print("BeginFlee");
            BeginFlee();
        }


    }


    private IEnumerator FleeRoutine()
    {
        Debug.Log(">>> FleeRoutine başladı");
        isFleeing = true;
        animator.SetBool("DoExercise", false);

        // 1) Koşu hızı ve animasyonu
        agent.speed = runningSpeed;
        agent.isStopped = false;

        Vector3 targetPoint = transform.position;
        bool found = false;

        // 2) Önce karganın tersine ± sapma ile dene
        for (int i = 0; i < 50; i++)
        {
            Vector3 fromCrow = (transform.position - crowTransform.position).normalized;
            float angleOffset = Random.Range(-30f, +30f);
            Vector3 baseDir = Quaternion.Euler(0, angleOffset, 0) * fromCrow;
            float dist = Random.Range(fleeDistance * 0.8f, fleeDistance);
            Vector3 samplePos = transform.position + baseDir * dist;

            // a) Yukarıdan aşağı raycast ile "Ground" kontrolü
            RaycastHit groundHit;
            if (Physics.Raycast(samplePos + Vector3.up * 5f, Vector3.down, out groundHit, 10f)
             && groundHit.collider.CompareTag("Ground"))
            {
                // b) NavMesh üzerinde örnekle
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(groundHit.point, out navHit, 50f, NavMesh.AllAreas))
                {
                    targetPoint = navHit.position + fromCrow * 5;
                    found = true;
                    Debug.Log($"FleeRoutine (dir): {i + 1}. denemede nokta bulundu: {targetPoint}");
                    break;
                }
            }
        }

        // 3) Eğer hâlâ bulamadıysa, tamamen rastgele bir noktayı dene
        if (!found)
        {
            Debug.Log("FleeRoutine: yönlü kaçma başarısız, rastgele nokta aranıyor...");
            for (int i = 0; i < 50; i++)
            {
                // yarıçap içinde rastgele yön ve mesafe
                Vector2 rnd2D = Random.insideUnitCircle * fleeDistance * 5;
                Vector3 samplePos = transform.position + new Vector3(rnd2D.x, 0, rnd2D.y);

                // raycast ve Ground tag kontrolü
                RaycastHit groundHit;
                if (Physics.Raycast(samplePos + Vector3.up * 1f, Vector3.down, out groundHit, 10f)
                 && groundHit.collider.CompareTag("Ground"))
                {
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(groundHit.point, out navHit, 50f, NavMesh.AllAreas))
                    {
                        targetPoint = navHit.position;
                        found = true;
                        Debug.Log($"FleeRoutine (rand): {i + 1}. denemede nokta bulundu: {targetPoint}");
                        break;
                    }
                }
            }

            if (!found)
                Debug.LogWarning("FleeRoutine: rastgele noktada da uygun zemin bulunamadı, olduğu yerde kalınıyor.");
        }

        // 4) Agent’a hedefi ver ve oraya git
        agent.ResetPath();
        agent.SetDestination(targetPoint);
        yield return new WaitUntil(() =>
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance);
        Debug.Log(">>> FleeRoutine varış tamamlandı");

        // 5) Birkaç saniye olduğu yerde takıl
        yield return new WaitForSeconds(2f);

        // 6) Hızı normale döndür ve normal rutine devam et
        agent.speed = speed;
        isFleeing = false;
        ResumePhaseRoutine();
        Debug.Log(">>> FleeRoutine bitti, isFleeing=false");
    }






    private Coroutine sittindDodgeRoutine;
    private IEnumerator SittingDodgeRoutine()
    {
        isSittingDodging = true;
        print("DODGLEEEEE");
        // Otururken dodge animasyonu
        animator.SetTrigger("SittingDodge");

        // Anim süresi kadar bekle
        yield return new WaitForSeconds(1f);

        isSittingDodging = false;
    }

    private StoneThrower stoneThrower;
    private IEnumerator ThrowRock()
    {
        isFleeing = true;
        agent.isStopped = true;
        animator.SetTrigger("Throw");
        float t = 0f;
        bool atisOlacak = true;
        while (t < 4.5f)
        {
            Vector3 lookPos = crowTransform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            if (t >= 2.2f && atisOlacak)
            {
                atisOlacak = false;
                stoneThrower.ThrowStone();
            }

            t += Time.deltaTime;
            yield return null;  // bir sonraki frame'e geç
        }

        isFleeing = false;
        agent.isStopped = false;
        ResumePhaseRoutine();
    }
    private IEnumerator AngryYellingRoutine()
    {
        isFleeing = true;
        agent.isStopped = true;

        // 1) Öfke animasyonunu tetikle
        animator.SetTrigger("isAngry");

        // 2) İlk bakış ve 7.4s boyunca sürekli kargaya dön
        float firstPhase = 7.4f;
        float t = 0f;
        while (t < firstPhase)
        {
            Vector3 lookPos = crowTransform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            t += Time.deltaTime;
            yield return null;  // bir sonraki frame'e geç
        }

        // 3) (İsteğe bağlı) burada istersen bir kez daha tetikleyebilirsin

        // 4) İkinci bakış süresi: 4.15s boyunca yine sürekli kargaya dön
        float secondPhase = 4.15f;
        t = 0f;
        bool atisOlacak = true;
        while (t < secondPhase)
        {
            Vector3 lookPos = crowTransform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            if (t > 2.6 && atisOlacak)
            {
                atisOlacak = false;
                stoneThrower.ThrowStone();
            }

            t += Time.deltaTime;
            yield return null;
        }

        // 5) Süre tamamlandı, normale dön
        isFleeing = false;
        agent.isStopped = false;
        ResumePhaseRoutine();
    }

    private void BeginFlee()
    {
        // diyer faz coroutinelerini iptal et
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
        if (workoutCoroutine != null)
        {
            StopCoroutine(workoutCoroutine);
            workoutCoroutine = null;
        }
        if (breakfastCoroutine != null)
        {
            StopCoroutine(breakfastCoroutine);
            breakfastCoroutine = null;
        }
        if (swimCoroutine != null)
        {
            StopCoroutine(swimCoroutine);
            swimCoroutine = null;
        }
        if (wanderAfternoonCoroutine != null)
        {
            StopCoroutine(wanderAfternoonCoroutine);
            wanderAfternoonCoroutine = null;
        }
        if (dinnerCoroutine != null)
        {
            StopCoroutine(dinnerCoroutine);
            dinnerCoroutine = null;
        }
        if (snackCoroutine != null)
        {
            StopCoroutine(snackCoroutine);
            snackCoroutine = null;
        }
        if (wanderEveningCoroutine != null)
        {
            StopCoroutine(wanderEveningCoroutine);
            wanderEveningCoroutine = null;
        }

        if (stressComponent.currentStress <= 50)
        {
            print("SADECE KAÇÇ");
            StartCoroutine(FleeRoutine());
            isAttacked = false;
            print("ÇALIŞIYOR MU LOOO");
        }
        else if (stressComponent.currentStress <= 75)
        {
            print("DENEMEEE");
            StartCoroutine(AngryYellingRoutine());
            isAttacked = false;
        }
        else
        {
            StartCoroutine(ThrowRock());
            isAttacked = false;
        }

    }

    private void ResumePhaseRoutine()
    {
        switch (currentPhase)
        {
            case Phase.WanderMorning:
                if (wanderCoroutine == null)
                    wanderCoroutine = StartCoroutine(WanderMorningRoutine());
                break;
            case Phase.Workout:
                if (workoutCoroutine == null)
                    workoutCoroutine = StartCoroutine(WorkoutRoutine());
                break;
            case Phase.Breakfast:
                if (breakfastCoroutine == null)
                    breakfastCoroutine = StartCoroutine(BreakfastRoutine());
                break;
            case Phase.Swim:
                if (swimCoroutine == null)
                    swimCoroutine = StartCoroutine(SwimRoutine());
                break;
            case Phase.WanderAfternoon:
                if (wanderAfternoonCoroutine == null)
                    wanderAfternoonCoroutine = StartCoroutine(WanderAfternoonRoutine());
                break;
            case Phase.Dinner:
                if (dinnerCoroutine == null)
                    dinnerCoroutine = StartCoroutine(DinnerRoutine());
                break;
            case Phase.Snack:
                if (snackCoroutine == null)
                    snackCoroutine = StartCoroutine(SnackRoutine());
                break;
            case Phase.WanderEvening:
                if (wanderEveningCoroutine == null)
                    wanderEveningCoroutine = StartCoroutine(WanderEveningRoutine());
                break;
                // Ended fazı yok, otele dönüş vs zaten kendi coroutine’inde.
        }
    }

    public void OnDiamondStolen()
    {
        if (!isDiamondChasing)
            StartCoroutine(DiamondChaseRoutine());
    }

private IEnumerator DiamondChaseRoutine()
{
    isDiamondChasing = true;

    // A) Mevcut faz coroutine’lerini iptal et
    StopAllPhaseCoroutines();

    // B) Koşma hızı & animasyonu
    agent.speed = runningSpeed;
    animator.SetBool("isRunning", true);
    animator.SetBool("isWalking", false);
    animator.SetBool("isIdle", false);

    float elapsed = 0f;
    bool hasThrown = false;

    while (elapsed < diamondChaseDuration)
    {
        // Kargaya doğru koş
        agent.isStopped = false;
        agent.SetDestination(crowTransform.position);

        // Mesafe kontrolü
        float dist = Vector3.Distance(transform.position, crowTransform.position);
        if (!hasThrown && dist <= diamondThrowDistance)
        {
            hasThrown = true;

            // kovalamayı durdur
            agent.isStopped = true;

            // Taş at coroutine’ini başlat ve bitmesini bekle
            yield return StartCoroutine(ThrowRock());
            break;
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    // Eğer süre dolduysa ve hala atış yapılmadıysa
    if (!hasThrown)
    {
        // koşmayı durdur
        agent.isStopped = true;
        // atışı yap
        yield return StartCoroutine(ThrowRock());
    }

    // C) Hızı normale döndür ve faz rutinine devam et
    agent.speed = speed;
    animator.SetBool("isRunning", false);

    ResumePhaseRoutine();
    isDiamondChasing = false;
    }


    public void StopAllPhaseCoroutines()
    {
        if (wanderCoroutine != null) { StopCoroutine(wanderCoroutine); wanderCoroutine = null; }
        if (workoutCoroutine != null) { StopCoroutine(workoutCoroutine); workoutCoroutine = null; }
        if (breakfastCoroutine != null) { StopCoroutine(breakfastCoroutine); breakfastCoroutine = null; }
        if (swimCoroutine != null) { StopCoroutine(swimCoroutine); swimCoroutine = null; }
        if (wanderAfternoonCoroutine != null) { StopCoroutine(wanderAfternoonCoroutine); wanderAfternoonCoroutine = null; }
        if (dinnerCoroutine != null) { StopCoroutine(dinnerCoroutine); dinnerCoroutine = null; }
        if (snackCoroutine != null) { StopCoroutine(snackCoroutine); snackCoroutine = null; }
        if (wanderEveningCoroutine != null) { StopCoroutine(wanderEveningCoroutine); wanderEveningCoroutine = null; }
        if (endOfDayCoroutine != null) { StopCoroutine(endOfDayCoroutine); endOfDayCoroutine = null; }
        // … varsa diğer skip/flee coroutineleri de …
    }
    
}

