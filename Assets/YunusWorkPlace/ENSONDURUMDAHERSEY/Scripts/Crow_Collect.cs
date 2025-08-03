using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_Collect : Crow_Base
{

    [Header("Raycast Ayarları")]
    public Transform rayOriginTransform;
    public float rayDistance = 1.5f;
    public LayerMask collectableLayer;

    [Header("UI")]
    public GameObject pressTextUI;

    [Header("Tasima Verileri")]
    public int maxStoneCount = 2;
    public List<GameObject> collectedStones = new List<GameObject>();
    public GameObject collectedDiamond = null;
    public GameObject collectedThorn = null;
    public GameObject collectedCurukMeyve = null;

    protected override void Awake()
    {
        base.Awake(); // bileşenleri çek
    }

    private void Update()
    {
        TryCollectItem();
        UIControl();
    }

    private void TryCollectItem()
    {
        if (Physics.Raycast(rayOriginTransform.position, rayOriginTransform.forward, out RaycastHit hit, rayDistance, collectableLayer))
        {
            string tag = hit.transform.gameObject.tag;

            collectable = true;
            if (tag == "Stone" || tag == "Diamond" || tag == "Thorn" || tag == "CurukMeyve")
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    

                    switch (tag)
                    {
                        case "Stone":
                            if (collectedStones.Count < maxStoneCount)
                                if (collectedDiamond != null || collectedThorn != null)
                                {
                                    DropEverything();
                                    CollectStone(hit.transform.gameObject);
                                    PrepareItem(hit);
                                }
                                else
                                {
                                    print("DOĞRU YERDEYİZ");
                                    CollectStone(hit.transform.gameObject);
                                    PrepareItem(hit);
                                }
                            else
                                Debug.Log("Maksimum taş kapasitesine ulaşıldı.");
                            break;

                        case "Diamond":
                            DropEverything(); // öncekileri bırak
                            PrepareItem(hit);
                            collectedDiamond = hit.transform.gameObject;
                            CollectGeneric(collectedDiamond);
                            break;

                        case "Thorn":
                            DropEverything(); // öncekileri bırak
                            PrepareItem(hit);
                            collectedThorn = hit.transform.gameObject;
                            CollectGeneric(collectedThorn);
                            break;
                        case "CurukMeyve":
                            DropEverything();
                            PrepareItem(hit);
                            collectedCurukMeyve = hit.transform.gameObject;
                            CollectGeneric(collectedCurukMeyve);
                            break;
                    }
                }
            }
        }
        else
        {
            collectable = false;
        }

        Debug.DrawRay(rayOriginTransform.position, rayOriginTransform.forward * rayDistance, Color.yellow);
    }

    public bool collectable = false;
    public bool pushable = false;
    public bool otherBirds = false;
    private void UIControl()
    {
        if (pushable || collectable || otherBirds)
        {
            OpenUI();
        }
        else
        {
            CloseUI();
        }
    }

    private void PrepareItem(RaycastHit hit)
    {
        hit.rigidbody.freezeRotation = true;
        hit.transform.parent = null;
        hit.transform.gameObject.GetComponent<Item_FollowPlayer>().someTimesTriggerCollider.isTrigger = true;

    }
    private void CollectStone(GameObject stone)
    {
        collectedStones.Add(stone);

        var fp = stone.GetComponent<Item_FollowPlayer>();
        fp.isTransforming = true;
        fp.isCollected = true;
        fp.carryingIndex = collectedStones.Count - 1;
    }

    private void CollectGeneric(GameObject obj)
    {
        var fp = obj.GetComponent<Item_FollowPlayer>();
        fp.isTransforming = true;
        fp.isCollected = true;
    }

    private void DropEverything()
    {
        if (collectedStones.Count != 0)
        {
            foreach (var stone in collectedStones)
            {
                var fp = stone.GetComponent<Item_FollowPlayer>();
                fp.isTransforming = false;
                fp.isCollected = false;
            }
            collectedStones.Clear();
        }

        if (collectedDiamond != null)
        {
            var fp = collectedDiamond.GetComponent<Item_FollowPlayer>();
            fp.isTransforming = false;
            fp.isCollected = false;
            collectedDiamond = null;
        }

        if (collectedThorn != null)
        {
            var fp = collectedThorn.GetComponent<Item_FollowPlayer>();
            fp.isTransforming = false;
            fp.isCollected = false;
            collectedThorn = null;
        }
        if (collectedCurukMeyve != null)
        {
            var fp = collectedCurukMeyve.GetComponent<Item_FollowPlayer>();
            fp.isTransforming = false;
            fp.isCollected = false;
            collectedCurukMeyve = null;
        }
    }

    public void OpenUI()
    {
        if (pressTextUI != null)
            pressTextUI.SetActive(true);
    }

    public void CloseUI()
    {
        if (pressTextUI != null)
            pressTextUI.SetActive(false);
    }
    
}

