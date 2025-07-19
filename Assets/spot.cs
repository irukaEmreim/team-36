using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spot : MonoBehaviour
{
    public static Spot Instance;

    public Transform sportArea;
    public Transform breakfastArea;
    public Transform poolArea;
    public Transform lobbyArea;

    void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSportArea() => sportArea.position;
    public Vector3 GetBreakfastTable() => breakfastArea.position;
    public Vector3 GetPoolSpot() => poolArea.position;
    public Vector3 GetIndoorArea() => lobbyArea.position;
}
