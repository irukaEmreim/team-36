using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_ThrowItem : Crow_Base
{
    [Header("Taş Atma")]
    public float throwForce = 6f;
    private Crow_Collect crowCollect;


    protected override void Awake()
    {
        base.Awake();
        crowCollect = GetComponent<Crow_Collect>();
    }


    public void ThrowItem()
    {
        if (crowCollect == null) return;

        if (crowCollect.collectedStones.Count > 0)
        {
            ThrowRock();
        }
        else if (crowCollect.collectedDiamond != null)
        {
            DropItem(crowCollect.collectedDiamond);
            crowCollect.collectedDiamond = null;
        }
        else if (crowCollect.collectedThorn != null)
        {
            DropItem(crowCollect.collectedThorn);
            crowCollect.collectedThorn = null;
        }
        else if (crowCollect.collectedCurukMeyve != null)
        {
            DropItem(crowCollect.collectedCurukMeyve);
            crowCollect.collectedCurukMeyve = null;
        }
    }

    public void ThrowRock()
    {
        var lastStone = crowCollect.collectedStones[crowCollect.collectedStones.Count-1]; // son taş
        var fp = lastStone.GetComponent<Item_FollowPlayer>();
        fp.isTransforming = false;

        crowCollect.collectedStones.RemoveAt(crowCollect.collectedStones.Count - 1);
        lastStone.GetComponent<TrailRenderer>().enabled = true;

        Throw(lastStone);
    }

    public void DropItem(GameObject obj)
    {
        var fp = obj.GetComponent<Item_FollowPlayer>();
        fp.isTransforming = false;

        Throw(obj);
    }

    private void Throw(GameObject obj)
    {
        Rigidbody itemRb = obj.GetComponent<Rigidbody>();
        if (itemRb == null) return;

        float playerSpeed = rb.velocity.magnitude;
        Vector3 throwDirection;

        if (playerSpeed > 0.5f)
        {
            throwDirection = rb.velocity.normalized;
            itemRb.velocity = throwDirection * throwForce + Vector3.up * 1f;
        }
        else
        {
            itemRb.velocity = Vector3.down * 2f;
        }

        itemRb.angularVelocity = Random.insideUnitSphere * 5f;
        

/*        Vector3 throwDirection = (playerSpeed > 0.5f) ? rb.velocity.normalized : Vector3.down;

                itemRb.velocity = throwDirection * throwForce + Vector3.up * (playerSpeed > 0.5f ? 1f : -1f);
                itemRb.angularVelocity = Random.insideUnitSphere * 5f;
        */
    }


}
