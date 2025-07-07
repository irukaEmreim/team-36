using UnityEngine;
using System.Collections.Generic;

public class ChairManager : MonoBehaviour
{
    public static ChairManager Instance;

    public List<Transform> allChairs = new List<Transform>();
    private Dictionary<Transform, bool> chairOccupied = new Dictionary<Transform, bool>();

    void Awake()
    {
        Instance = this;

        foreach (var chair in allChairs)
        {
            chairOccupied[chair] = false;
        }
    }

    public Transform GetAvailableChair()
    {
        foreach (var chair in allChairs)
        {
            if (!chairOccupied[chair])
            {
                chairOccupied[chair] = true;
                return chair;
            }
        }

        return null;
    }

    public void ReleaseChair(Transform chair)
    {
        if (chairOccupied.ContainsKey(chair))
            chairOccupied[chair] = false;
    }
}