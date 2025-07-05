using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject[] baseCharacters; // 4–6 prefab (2 erkek, 2 kadın vs.)
    public int totalCount = 50;
    public Vector2 area = new Vector2(30, 30);

    public Material[] shirtMaterials;
    public Material[] hairMaterials;
    public Material[] pantsMaterials;
    public Material[] skinMaterials;

    void Start()
    {
        for (int i = 0; i < totalCount; i++)
        {
            // Karakter pozisyonu
            Vector3 pos = new Vector3(
                Random.Range(-area.x / 2f, area.x / 2f),
                0,
                Random.Range(-area.y / 2f, area.y / 2f)
            );

            // Random bir karakter seç
            GameObject chosen = baseCharacters[Random.Range(0, baseCharacters.Length)];
            GameObject clone = Instantiate(chosen, pos, Quaternion.identity);

            // Tişört, saç, şort, cilt renklerini değiştir
            RandomizeColors(clone);
        }
    }

    void RandomizeColors(GameObject obj)
    {
        Material sharedSkinMaterial = GetRandomMaterial(skinMaterials);
        Material shirtMat = GetRandomMaterial(shirtMaterials);
        Material hairMat = GetRandomMaterial(hairMaterials);
        Material pantsMat = GetRandomMaterial(pantsMaterials);

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
                    newMats[i] = sharedSkinMaterial;
            }

            r.materials = newMats;
        }
    }


    Material GetRandomMaterial(Material[] list)
    {
        if (list != null && list.Length > 0)
            return list[Random.Range(0, list.Length)];

        return new Material(Shader.Find("Standard")) { color = Color.gray }; // ya da istediğin default
    }


    enum ColorType { TShirt, Hair, Pants, Skin }

   
}
