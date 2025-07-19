using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrowDirectAttack : MonoBehaviour
{
    private AudioSource audioSource;
    private SphereCollider sphereCollider;
    private Rigidbody rb;

    [Header("Gak")]
    public AudioClip[] audioClips;
    public List<GameObject> humans; // alandaki humanlar

    //[Header("Tas Ayarlari")]
    //public int maxStoneCount = 2;
    //public int currentStoneCount = 0;

    //public GameObject[] rocks;
    //public Transform rockAttackTransform;
    public float throwForce = 6f;

    //[Header("Tas Alma")]
    //public float pickupRange = 3f;
    //public LayerMask pickupLayerMask;
    //public Transform rayOriginTransform;

    //[Header("Tas Iconlari")]
    //public Image[] rockImage;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sphereCollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
    }

    // GAK SALDIRISI BURASIIIIIIIIIIIIIII DURACAKKKK
    public void GakAttack()
    {
        audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.Play();

        Debug.Log("GAK ATTACK");

        if (humans.Count <= 0)
            return;

        foreach (var item in humans)
        {
            item.GetComponent<BaseNPC>()?.TakeDamage(15);     // Bu tarz bir şey olacak humanda
            Debug.Log(item.name + " 15 hasar aldı");
        }
    }

    public CrowCollect crowCollect;
    public void ThrowItem()
    {
        if (crowCollect.collectedStones.Count != 0)
        {
            ThrowRock();
        }
        else
        {
            if (crowCollect.collectedDiamond != null)
            {
                DropItem(crowCollect.collectedDiamond);
                crowCollect.collectedDiamond.GetComponent<BoxCollider>().isTrigger = false;
                crowCollect.collectedDiamond = null;

            }
            else if (crowCollect.collectedThorn != null)
            {
                DropItem(crowCollect.collectedThorn);
                crowCollect.collectedThorn = null;

            }
        }
    }
    
    public void ThrowRock()
    {
        GameObject lastStone = crowCollect.collectedStones[crowCollect.collectedStones.Count - 1];
        var fp = lastStone.GetComponent<FollowPlayer>();
        fp.isTransforming = false;
        crowCollect.collectedStones.RemoveAt(crowCollect.collectedStones.Count - 1);
        lastStone.GetComponent<TrailRenderer>().enabled = true;
        Throw(lastStone);
        

    }
    public void DropItem(GameObject carryingObject)
    {
        var fp = carryingObject.GetComponent<FollowPlayer>();
        fp.isTransforming = false;
        Throw(carryingObject);
    }
private void Throw(GameObject throwObject)
    {
        Rigidbody itemRb = throwObject.GetComponent<Rigidbody>();

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
    }

    // TAS ATMA SALDIRISI BURASIIIIIIIIIIIIIIIIII
    /*
        public void ThrowRock()
        {
            if (currentStoneCount <= 0)
            {
                Debug.Log("TAS YOK!");
                return;
            }

            GameObject selectedRockPrefab = rocks[Random.Range(0, rocks.Length)];
            GameObject spawnedRock = Instantiate(selectedRockPrefab, rockAttackTransform.position, rockAttackTransform.rotation);
            Rigidbody spawnedRb = spawnedRock.GetComponent<Rigidbody>();

            Destroy(spawnedRock, 10f); // Yok Etmeden Emin Degilim 

            if (spawnedRb == null)
            {
                // Rb eklemeyi unuttugum bir prefab yuzunden boyle bir kontrol var :p
                Debug.LogWarning("Olusturulan Tas'ta RB YOK!");
                return;
            }

            // Tas yonu <=> hareket yönü
            float playerSpeed = rb.velocity.magnitude;
            Vector3 throwDirection;

            if (playerSpeed > 0.5f)
            {
                throwDirection = rb.velocity.normalized;
                spawnedRb.velocity = throwDirection * throwForce + Vector3.up * 1f;
            }
            else
            {
                spawnedRb.velocity = Vector3.down * 2f;
            }

            spawnedRb.angularVelocity = Random.insideUnitSphere * 5f;

            currentStoneCount--;
        }
    */


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human"))
        {
            humans.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Human") && humans.Contains(other.gameObject))
        {
            humans.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        //HandleStonePickupInput();
        //UpdateRockUI();

    }

    /*
    // TAS TOPLA
    private void HandleStonePickupInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryRaycastPickup();
        }
    }
    */

    /*
        public float pickupRadius = 1f;
        private void TryRaycastPickup()
        {
            if (currentStoneCount >= maxStoneCount)
            {
                Debug.Log("Kapasite doludur ho");
                return;
            }

            Ray ray = new Ray(rayOriginTransform.position, rayOriginTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
            {
                if (hit.collider.CompareTag("PickupStone"))
                {
                    Destroy(hit.collider.gameObject);
                    currentStoneCount++;
                }
            }
        }
    */
    /*

        // TAS UI'i
        private void UpdateRockUI()
        {
            if (rockImage.Length < 2) return;

            rockImage[0].enabled = currentStoneCount >= 1;
            rockImage[1].enabled = currentStoneCount == 2;
        }
    */
}
