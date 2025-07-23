using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] baseCharacters;
    public int totalCount = 50;
    public Vector2 area = new Vector2(30, 30);

    public Material[] shirtMaterials;
    public Material[] hairMaterials;
    public Material[] pantsMaterials;
    public Material[] skinMaterials;

    private HashSet<string> usedCombinations = new HashSet<string>();

    RuntimeAnimatorController guestAnimator;
    RuntimeAnimatorController employeeAnimator;

    void Awake()
    {
        guestAnimator = Resources.Load<RuntimeAnimatorController>("guest");
        employeeAnimator = Resources.Load<RuntimeAnimatorController>("Employee");

        StartCoroutine(InitializeSpawnSequence());
    }

    IEnumerator InitializeSpawnSequence()
    {
        yield return new WaitForEndOfFrame();
        AssignRoleToScenePrefabs();

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SpawnClonesSafely()); // ✅ önce klonlar doğsun

        LogNPCCounts(); // ✅ sonra tüm NPC'leri say
    }


    IEnumerator SpawnClonesSafely()
    {
        int created = 0;
        int guestCount = 0;
        int employeeCount = 0;
        int maxEmployees = Mathf.FloorToInt(totalCount * 0.2f);
        int maxGuests = totalCount - maxEmployees;

        int attempts = 0;
        int maxAttempts = totalCount * 10;

        while (created < totalCount && attempts < maxAttempts)
        {
            attempts++;

            int s = Random.Range(0, shirtMaterials.Length);
            int h = Random.Range(0, hairMaterials.Length);
            int p = Random.Range(0, pantsMaterials.Length);
            int sk = Random.Range(0, skinMaterials.Length);

            string comboKey = $"{s}_{h}_{p}_{sk}";
            if (usedCombinations.Contains(comboKey))
                continue;

            usedCombinations.Add(comboKey);

          /*  Vector3 randomPoint = new Vector3(
                Random.Range(-area.x / 2f, area.x / 2f),
                0,
                Random.Range(-area.y / 2f, area.y / 2f)
            )*/;
          
          Vector3 randomPoint = new Vector3(
              Random.Range(-13f, 40f),
              3f,
              Random.Range(-36f, -24f)
          );


            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"❌ {randomPoint} → NavMesh dışında. Spawn atlandı.");
                continue;
            }

            GameObject chosen = baseCharacters[Random.Range(0, baseCharacters.Length)];
            GameObject clone = Instantiate(chosen, hit.position, Quaternion.identity);
            // 🔥 Prefab’tan gelen Guest/HotelEmployee scriptlerini temizle
            DestroyImmediate(clone.GetComponent<Guest>());
            DestroyImmediate(clone.GetComponent<HotelEmployee>());

            ApplyMaterials(clone, shirtMaterials[s], hairMaterials[h], pantsMaterials[p], skinMaterials[sk]);

            Animator animator = clone.GetComponent<Animator>();
            if (animator == null)
                animator = clone.AddComponent<Animator>();

            // 1. Guest veya Employee zaten var mı kontrol et
            if (clone.GetComponent<Guest>() == null && clone.GetComponent<HotelEmployee>() == null)
            {
                if (employeeCount < maxEmployees)
                {
                    clone.AddComponent<HotelEmployee>();
                    employeeCount++;

                    if (employeeAnimator != null)
                        animator.runtimeAnimatorController = employeeAnimator;
                }
                else if (guestCount < maxGuests)
                {
                    clone.AddComponent<Guest>();
                    guestCount++;

                    if (guestAnimator != null)
                        animator.runtimeAnimatorController = guestAnimator;
                }
            }
            else
            {
                // Zaten varsa → güvenlik katmanı olarak animator’ı yine de set et
                if (clone.GetComponent<HotelEmployee>() != null && employeeAnimator != null)
                    animator.runtimeAnimatorController = employeeAnimator;
                else if (clone.GetComponent<Guest>() != null && guestAnimator != null)
                    animator.runtimeAnimatorController = guestAnimator;
            }
            
            if (clone.GetComponent<Guest>() != null)
            {
                float jewelryChance = 0.7f;
                if (Random.value > jewelryChance)
                {
                    // Elması bul ve yok et
                    Transform[] allChildren = clone.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in allChildren)
                    {
                        if (t.name.ToLower().Contains("diamond"))
                        {
                            Debug.Log($"{clone.name} → 💎 Elmas silindi (random disable).");
                            Destroy(t.gameObject);
                            break;
                        }
                    }
                }
            }


            created++;
            yield return null; // 🔁 her doğumdan sonra 1 frame bekle
        }
        

    }
    void LogNPCCounts()
    {
        var allGuests = FindObjectsOfType<Guest>();
        var allEmployees = FindObjectsOfType<HotelEmployee>();

        int total = allGuests.Length + allEmployees.Length;

        Debug.Log($"📊 Toplam NPC: {total} | 🧢 Guest: {allGuests.Length} | 💼 Employee: {allEmployees.Length}");
    }


    void AssignRoleToScenePrefabs()
    {
        AnimationControl[] allInScene = FindObjectsOfType<AnimationControl>(true);

        int employeeCount = 0;
        int guestCount = 0;
        int maxEmployees = Mathf.FloorToInt(totalCount * 0.2f);

        foreach (var ac in allInScene)
        {
            GameObject go = ac.gameObject;

            // 💡 SADECE sahnedeki elle konmuş objelere uygula
            if (go.scene.name != null && go.scene.name != gameObject.scene.name)
                continue;

            Animator animator = go.GetComponent<Animator>();
            if (animator == null)
                animator = go.AddComponent<Animator>();

            if (go.GetComponent<Guest>() == null && go.GetComponent<HotelEmployee>() == null)
            {
                if (employeeCount < maxEmployees)
                {
                    go.AddComponent<HotelEmployee>();
                    employeeCount++;

                    if (employeeAnimator != null)
                        animator.runtimeAnimatorController = employeeAnimator;
                }
                else
                {
                    go.AddComponent<Guest>();
                    guestCount++;

                    if (guestAnimator != null)
                        animator.runtimeAnimatorController = guestAnimator;
                }
            }
            else
            {
                if (go.GetComponent<HotelEmployee>() != null && employeeAnimator != null)
                    animator.runtimeAnimatorController = employeeAnimator;
                else if (go.GetComponent<Guest>() != null && guestAnimator != null)
                    animator.runtimeAnimatorController = guestAnimator;
            }
        }

        Debug.Log($"🧍 Elle sahneye konan prefablar: {employeeCount} çalışan, {guestCount} misafir.");
    }


    void ApplyMaterials(GameObject obj, Material shirtMat, Material hairMat, Material pantsMat, Material skinMat)
    {
        var renderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var r in renderers)
        {
            Material[] newMats = r.materials;
            for (int i = 0; i < newMats.Length; i++)
            {
                string matName = newMats[i].name.ToLower();
                if (matName.Contains("shirt"))
                    newMats[i] = shirtMat;
                else if (matName.Contains("hair"))
                    newMats[i] = hairMat;
                else if (matName.Contains("pant") || matName.Contains("short"))
                    newMats[i] = pantsMat;
                else if (matName.Contains("skin") || matName.Contains("head") || matName.Contains("body") || matName.Contains("arm") || matName.Contains("leg"))
                    newMats[i] = skinMat;
            }
            r.materials = newMats;
        }
    }
}
