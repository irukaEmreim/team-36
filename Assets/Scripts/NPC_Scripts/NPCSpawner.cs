using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabları")]
    public GameObject[] npcPrefabs; // Female 1, Male 1, Female 2...

    [Header("Spawn Ayarları")]
    public int npcCount = 20;
    public Vector2 spawnArea = new Vector2(10f, 10f);

    void Start()
    {
        for (int i = 0; i < npcCount; i++)
        {
            SpawnRandomNPC();
        }
    }

    void SpawnRandomNPC()
    {
        Vector3 position = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            0,
            Random.Range(-spawnArea.y, spawnArea.y)
        );

        GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
        GameObject npc = Instantiate(selectedPrefab, position, Quaternion.identity);

        // Renk randomizer varsa çağır
        RandomAppearance appearance = npc.GetComponent<RandomAppearance>();
        if (appearance != null)
        {
            appearance.ApplyRandomAppearance();
        }
    }
}