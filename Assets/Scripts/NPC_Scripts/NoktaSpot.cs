using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// In NoktaSpot.cs, add these changes:



public class NoktaSpot : MonoBehaviour
{
    public Vector3 sportAreaSize = new Vector3(10f, 0f, 10f);
    public Vector3 poolAreaSize = new Vector3(30f, 0f, 20f); // Add this line
    
    public static NoktaSpot Instance;

    public Transform sportArea;
    public Transform breakfastArea;
    public Transform poolArea;  // This should already exist
    public Transform lobbyArea;

    void Awake()
    {
        Instance = this;
    }

    // Add this new method
    public Bounds GetPoolBounds()
    {
        return new Bounds(poolArea.position, poolAreaSize);
    }

    // Keep all your existing methods
    public Bounds GetSportBounds()
    {
        return new Bounds(sportArea.position, sportAreaSize);
    }

    public Vector3 GetSportArea() => sportArea.position;
    public Vector3 GetBreakfastTable() => breakfastArea.position;
    public Vector3 GetPoolSpot() => poolArea.position;
    public Vector3 GetIndoorArea() => lobbyArea.position;
}