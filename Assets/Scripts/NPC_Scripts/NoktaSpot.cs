using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    
   
    public Vector3 GetRandomPointInSportArea()
    {
        Vector3 center = sportArea.position;

        // Spor alanının genişliğini burada ayarlıyorsun
        float width = 6f;
        float length = 4f;

        Vector3 randomOffset = new Vector3(
            Random.Range(-width / 2f, width / 2f),
            0,
            Random.Range(-length / 2f, length / 2f)
        );

        Vector3 finalPos = center + randomOffset;

        // NavMesh kontrolü
        if (NavMesh.SamplePosition(finalPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Fallback: merkeze dön
        return center;
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
