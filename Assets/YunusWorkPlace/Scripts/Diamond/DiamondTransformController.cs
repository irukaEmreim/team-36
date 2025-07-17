using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondTransformController : MonoBehaviour
{
    public Transform kargaTransform;
    public bool isTransforming = false;

    void LateUpdate()
    {
        if (isTransforming)
        {
            transform.position = kargaTransform.position;
        }
    }
}
