using System.Collections;
using Microlight.MicroBar;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Base_Test : MonoBehaviour
{
    public float maxStress;
    public float currentStress;
    [Tooltip("Sahnedeki MicroBar bileşeni")] 
    public MicroBar stressBar;
    private Animator animator;
    public GuestDailyRoutine guest;
    private NavMeshAgent agent;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        // Türüne göre maxStress
        if (GetComponent<GuestDailyRoutine>() != null) maxStress = 100f;
        else if (GetComponent<EmployeeDailyRoutine>() != null) maxStress = 200f;
        else maxStress = 100f;

        // Başlangıç stresi sıfır
        currentStress = 0f;
    }

    private void Start()
    {
        if (stressBar == null)
        {
            Debug.LogWarning($"{name}: stressBar referansı atanmamış!");
            return;
        }

        // Bar'ı inisyalize et ve 0’dan başlat
        stressBar.Initialize(maxStress);
        stressBar.UpdateBar(0f, UpdateAnim.Damage);

        Debug.Log($"{name}: MicroBar initialized with max={maxStress}");
    }

    public void TakeDamage(float amount)
    {
        currentStress = Mathf.Clamp(currentStress + amount, 0f, maxStress);
            Debug.Log($"{name} TakeDamage çağrıldı, yeni stress = {currentStress}");

            // 2) Bar’ı güncelle
            if (stressBar != null)
            {
                stressBar.UpdateBar(currentStress, UpdateAnim.Damage);
            }
        if (currentStress >= maxStress && !leaving)
        {        var guest = GetComponent<GuestDailyRoutine>();
            if (guest != null)
            {
                guest.StopAllPhaseCoroutines();
                GetComponent<GuestDailyRoutine>().enabled = false;
            }
    var emp = GetComponent<EmployeeDailyRoutine>();
            if (emp != null)
            {
                emp.StopAllPhaseCoroutines();
                GetComponent<EmployeeDailyRoutine>().enabled = false;
            }
    // 2) Başka da bir coroutineler kalmaması için
    StopAllCoroutines();  // Bu script’te başlatılan tüm coroutineleri durdurur

    // 3) LeaveHotel koroutinini başlat
    StartCoroutine(LeaveHotel());
        }
        else
        {
            
            // 1) Stresi güncelle

        var guest = GetComponent<GuestDailyRoutine>();
        if (guest != null)
        {
            guest.isAttacked = true;           // ← Hasar aldığını işaretle
            guest.CheckCrowProximity();        // ← Kaçma kontrolünü hemen çalıştır
            return;
        }

        // 4) Employee ise employee rutinini tetikle
        var emp = GetComponent<EmployeeDailyRoutine>();
        if (emp != null)
        {
            return;
        }
        }
    }
    public Transform leavePoint;
    public DoorAnimation doorAnimation;
    private bool leaving = false;
    private float runningSpeed = 5f;
    private IEnumerator LeaveHotel()
    {
        // A) Hemen agent’ı durdurup animasyonu tetikle
        agent.isStopped = true;
        agent.ResetPath();
        animator.SetTrigger("isAngry");

        // B) Animasyon uzunluğunu bire bir beklemek daha sağlıklı olur
        //    (9 saniyenin tam animasyon sürenle eşleştiğinden emin ol)
        yield return new WaitForSeconds(7f);
        animator.Play("locom_f_running_20f");
        animator.SetBool("isRunning",true);
        // C) LeavePoint NavMesh üzerindeki en yakın noktaya sample et
        NavMeshHit hit;
        Vector3 destination;
        if (NavMesh.SamplePosition(leavePoint.position, out hit, 2f, NavMesh.AllAreas))
            destination = hit.position;
        else
            destination = leavePoint.position; // fallback

        // D) Artık koş (hızını set et!), agent’ı yeniden aç ve oraya git
        agent.speed = runningSpeed;
        agent.isStopped = false;
        agent.SetDestination(destination);

        // E) Koşu animasyonunu ayarla
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", false);


        // F) Varana dek bekle
        yield return new WaitUntil(() =>
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance);

        // G) Kapıyı aç, biraz bekle, kapıyı kapat ve NPC’yi sil
        agent.isStopped = true;
        animator.SetBool("isRunning", false);
        doorAnimation.OpenDoor();
        yield return new WaitForSeconds(1f);
        doorAnimation.CloseDoor();
        Destroy(gameObject);

    }
}
