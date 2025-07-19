using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowCollect : MonoBehaviour
{
    [Header("Raycast Ayarları")]
    public Transform rayOriginTransform;
    public float rayDistance = 3f;
    public LayerMask collectableLayer;

    [Header("UI")]
    public GameObject pressTextUI;

    [Header("Crow Direct Attack")]
    public CrowDirectAttack crowDirectAttack;

    [Header("Tasima Yerleri")]
    public Transform uzunTasimaYeri;
    public Transform tasima1;
    public Transform tasima2;

    [Header("is Carrying Items")]
    public int maxStoneCount = 2;
    public List<GameObject> collectedStones = new List<GameObject>();
    public GameObject collectedDiamond = null;
    public GameObject collectedThorn = null;


    void Update()
    {
        CollectItem();
    }

    private void CollectItem()
    {
        Debug.DrawRay(rayOriginTransform.position, rayOriginTransform.forward * rayDistance, Color.yellow);

        Ray ray = new Ray(rayOriginTransform.position, rayOriginTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, collectableLayer))
        {
            if (hit.collider.CompareTag("Stone") || hit.collider.CompareTag("Diamond") || hit.collider.CompareTag("Thorn"))
            {
                OpenUI();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.rigidbody.freezeRotation = true;
                    hit.transform.parent = null;
                    BoxCollider[] boxColliders = hit.collider.gameObject.GetComponents<BoxCollider>();
                    foreach (BoxCollider boxCollider in boxColliders) {
                        boxCollider.isTrigger = true;
                    }
                    switch (hit.collider.tag)
                        {
                            case "Stone":
                                if (collectedStones.Count >= maxStoneCount)
                                {
                                    Debug.Log("Kapasite doludur, taş alamazsın!");
                                    break;
                                }
                                // Taşıdığı şeyleri bırakacağı bir kod da olmalı burada
                                CollectStone(hit);
                                break;

                            case "Diamond":
                                print("ÇALIŞIYOR MU LOOOOO");
                                // taşıdığı her şeyi bırakacağı bir kod olmalı
                                collectedDiamond = hit.collider.gameObject;
                                CollectItems(hit);
                                //CollectDiamond();
                                break;

                            case "Thorn":
                                collectedThorn = hit.collider.gameObject;
                                CollectItems(hit);
                                //CollectThorn();
                                break;
                            default:
                                break;
                        }

                }

            }
        }

    }


    private void OpenUI()
    {
        pressTextUI.SetActive(true);
    }
    private void CloseUI()
    {
        pressTextUI.SetActive(false);
    }

    private void CollectStone(RaycastHit hit)
    {
        collectedStones.Add(hit.collider.gameObject);
        var fp = hit.collider.gameObject.GetComponent<FollowPlayer>();
        fp.isTransforming = true;
        fp.carryingIndex = collectedStones.Count - 1;
        fp.isCollected = true;
    }

    private void CollectItems(RaycastHit hit)
    {
        var fp = hit.collider.gameObject.GetComponent<FollowPlayer>();
        fp.isTransforming = true;
        fp.isCollected = true;
    }


}