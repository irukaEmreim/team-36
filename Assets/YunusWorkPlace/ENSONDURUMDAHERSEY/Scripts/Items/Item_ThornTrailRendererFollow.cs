using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_ThornTrailRendererFollow : MonoBehaviour
{
    private TrailRenderer trailRenderer;
    public List<TrailRenderer> trailRenderers;

    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (trailRenderer.enabled)
        {
            foreach (var trailRen in trailRenderers)
            {
                trailRen.enabled = true;
            }
        }
        else
        {
            foreach (var trailRen in trailRenderers)
            {
                trailRen.enabled = false;
            }
        }
    }
}
