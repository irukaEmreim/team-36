using UnityEngine;
using System.Collections.Generic;

public class ChairManager : MonoBehaviour
{
    public static ChairManager Instance;

    public List<Transform> breakfastChairs = new List<Transform>();
    public List<Transform> lunchChairs = new List<Transform>();
    public List<Transform> dinnerChairs = new List<Transform>();
    
    private Dictionary<Transform, bool> breakfastChairOccupied = new Dictionary<Transform, bool>();
    private Dictionary<Transform, bool> lunchChairOccupied = new Dictionary<Transform, bool>();
    private Dictionary<Transform, bool> dinnerChairOccupied = new Dictionary<Transform, bool>();

    void Awake()
    {
        Instance = this;

        InitializeChairDictionaries(breakfastChairs, breakfastChairOccupied);
        InitializeChairDictionaries(lunchChairs, lunchChairOccupied);
        InitializeChairDictionaries(dinnerChairs, dinnerChairOccupied);
    }

    void InitializeChairDictionaries(List<Transform> chairs, Dictionary<Transform, bool> dict)
    {
        foreach (var chair in chairs)
        {
            dict[chair] = false;
        }
    }

    // Original method without mealType
    public Transform GetAvailableChair()
    {
        // Try breakfast chairs first, then lunch, then dinner
        return GetAvailableChair("Breakfast") ?? 
               GetAvailableChair("Lunch") ?? 
               GetAvailableChair("Dinner");
    }

    // New method with mealType
    public Transform GetAvailableChair(string mealType)
    {
        Dictionary<Transform, bool> targetDict;
        List<Transform> targetChairs;
        
        switch(mealType)
        {
            case "Lunch":
                targetDict = lunchChairOccupied;
                targetChairs = lunchChairs;
                break;
            case "Dinner":
                targetDict = dinnerChairOccupied;
                targetChairs = dinnerChairs;
                break;
            case "Breakfast":
            default:
                targetDict = breakfastChairOccupied;
                targetChairs = breakfastChairs;
                break;
        }

        foreach (var chair in targetChairs)
        {
            if (!targetDict[chair])
            {
                targetDict[chair] = true;
                return chair;
            }
        }

        return null;
    }

    // Original method without mealType
    public void ReleaseChair(Transform chair)
    {
        // Try to release from all areas
        if (breakfastChairOccupied.ContainsKey(chair))
            breakfastChairOccupied[chair] = false;
        else if (lunchChairOccupied.ContainsKey(chair))
            lunchChairOccupied[chair] = false;
        else if (dinnerChairOccupied.ContainsKey(chair))
            dinnerChairOccupied[chair] = false;
    }

    // New method with mealType
    public void ReleaseChair(Transform chair, string mealType)
    {
        Dictionary<Transform, bool> targetDict;
        
        switch(mealType)
        {
            case "Lunch":
                targetDict = lunchChairOccupied;
                break;
            case "Dinner":
                targetDict = dinnerChairOccupied;
                break;
            case "Breakfast":
            default:
                targetDict = breakfastChairOccupied;
                break;
        }

        if (targetDict.ContainsKey(chair))
            targetDict[chair] = false;
    }
}