using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] baseCharacters;
    public int totalCount = 50;
    public Vector2 area = new Vector2(50, 50);

    public Material[] shirtMaterials;
    public Material[] hairMaterials;
    public Material[] pantsMaterials;
    public Material[] skinMaterials;

    private HashSet<string> usedCombinations = new HashSet<string>();

    
    void Awake()
    {
        StartCoroutine(AssignRolesNextFrame());
    }

    IEnumerator AssignRolesNextFrame()
    {
        yield return null; // 🕐 1 frame bekle

        AssignRoleToScenePrefabs();
    }
    void Start()
    {
        

        int created = 0;
        int guestCount = 0;
        int employeeCount = 0;
        int maxEmployees = 10;
        int maxGuests = 40;
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

            Vector3 pos = new Vector3(
                Random.Range(-area.x / 2f, area.x / 2f),
                0,
                Random.Range(-area.y / 2f, area.y / 2f)
            );

            GameObject chosen = baseCharacters[Random.Range(0, baseCharacters.Length)];
            GameObject clone = Instantiate(chosen, pos, Quaternion.identity);

            ApplyMaterials(clone, shirtMaterials[s], hairMaterials[h], pantsMaterials[p], skinMaterials[sk]);

            // Eğer karakterte zaten bir rol yoksa, yalnızca bir tane ekle
            if (clone.GetComponent<Guest>() == null && clone.GetComponent<HotelEmployee>() == null)
            {
                if (guestCount < maxGuests)
                {
                    clone.AddComponent<Guest>();
                    guestCount++;
                }
                else if (employeeCount < maxEmployees)
                {
                    clone.AddComponent<HotelEmployee>();
                    employeeCount++;
                }
                
                
            }
            
            if (clone.GetComponent<Guest>() != null && clone.GetComponent<HotelEmployee>() != null)
            {
                Debug.LogWarning($"{clone.name} → aynı anda iki rol taşıyor!");
            }



            created++;
        }
    }

    void AssignRoleToScenePrefabs()
    {
        AnimationControl[] allInScene = FindObjectsOfType<AnimationControl>();

        int employeeCount = 0;
        int guestCount = 0;

        foreach (var ac in allInScene)
        {
            GameObject go = ac.gameObject;

            if (go.GetComponent<Guest>() != null || go.GetComponent<HotelEmployee>() != null)
                continue;


            if (employeeCount < 10)
            {
                go.AddComponent<HotelEmployee>();
                employeeCount++;
            }
            else
            {
                go.AddComponent<Guest>();
                guestCount++;
            }
        }

        Debug.Log($"Sahnedeki prefablar: {employeeCount} çalışan, {guestCount} misafir.");
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
