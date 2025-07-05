using UnityEngine;

public class RandomAppearance : MonoBehaviour
{
    [Header("Renk Listeleri")]
    public Material[] hairMaterials;
    public Material[] shirtMaterials;
    public Material[] skinMaterials;

    [Header("Renderer Referansları")]
    public Renderer hairRenderer;
    public Renderer shirtRenderer;
    public Renderer skinRenderer;

    public void ApplyRandomAppearance()
    {
        if (hairRenderer && hairMaterials.Length > 0)
            hairRenderer.material = hairMaterials[Random.Range(0, hairMaterials.Length)];

        if (shirtRenderer && shirtMaterials.Length > 0)
            shirtRenderer.material = shirtMaterials[Random.Range(0, shirtMaterials.Length)];

        if (skinRenderer && skinMaterials.Length > 0)
            skinRenderer.material = skinMaterials[Random.Range(0, skinMaterials.Length)];
    }
}