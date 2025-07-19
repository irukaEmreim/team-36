using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoktaSpot : MonoBehaviour
{
    
    
    
    public static NoktaSpot Instance;

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
