using UnityEngine;

public class Chair : MonoBehaviour
{
    [Tooltip("NPC oturunca bu pozisyona gidip orada duracaklar.")]
    public Transform seatPoint;

    private bool isOccupied = false;
    public bool IsOccupied => isOccupied;

    /// <summary>
    /// Boşsa oturulur ve true döner; doluysa false döner.
    /// </summary>
    public bool TryOccupy()
    {
        if (isOccupied) return false;
        isOccupied = true;
        return true;
    }

    /// <summary>
    /// NPC kalkınca sandalyeyi tekrar boşalt.
    /// </summary>
    public void Vacate()
    {
        isOccupied = false;
    }
}
