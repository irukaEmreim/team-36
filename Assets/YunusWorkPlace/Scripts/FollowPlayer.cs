using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class FollowPlayer : MonoBehaviour
{
    public List<Transform> transforms;
    private Transform left, right, center;
    private Rigidbody rb;
    private Collider collider;
    public CrowCollect crowCollect;
    public bool isTransforming = false;
    public bool isCollected = false;
    public int carryingIndex = -1;
    public CrowFlight crowFlight;

    public GameObject player;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    void Start()
    {
        left = transforms[0];
        right = transforms[1];
        center = transforms[2];
    }

    private float itemVel;
    void Update()
    {
        itemVel = rb.velocity.magnitude;
        if (isTransforming)
        {
           // print("EEE");
            isCollected = true;
            //rb.isKinematic = true;
            collider.isTrigger = true;
            rb.useGravity = false;
        }
        if (!isTransforming && isCollected)        // BURASI TAŞI BIRAKINCA ÇALIŞMALI
        {
            print("AOBSFDGPOSDKF");
            collider.isTrigger = false;
            rb.freezeRotation = false;
            //rb.isKinematic = false;
            rb.useGravity = true;
            // geri almak için bu collider'ın trigger'ının  false olması lazım
            if (gameObject.tag == "Diamond")
            {
                gameObject.GetComponent<BoxCollider>().isTrigger = true;
            }
            transform.parent = null;
            StartCoroutine(deneme());
        }
        if (gameObject.tag == "Thorn" && isTransforming)
        {
            Vector3 rot = transform.rotation.eulerAngles;
            rot.x = 0f;
            rot.y = player.transform.rotation.eulerAngles.y;
            rot.z = 90f;
            transform.rotation = Quaternion.Euler(rot);
            
        } 
    }
    private bool yineDeneme = false;

    public string realTag;
    IEnumerator deneme()
    {
        gameObject.tag = "Untagged";
        isCollected = false;
        yield return new WaitForSeconds(0.2f);
        collider.enabled = true;

        yield return new WaitUntil(() => itemVel <= 0.25f);
        gameObject.tag = realTag;

    }

    void LateUpdate()
    {
        if (isTransforming)
        {
            if (this.gameObject.CompareTag("Stone"))
            {
                if (carryingIndex == 0)
                {
                    transform.position = left.position;
                }
                if (carryingIndex == 1)
                {
                    transform.position = right.position;
                }
            }
            else if (this.gameObject.CompareTag("Diamond"))
            {
                transform.position = center.position;

            }
            else if (this.gameObject.CompareTag("Thorn"))
            {
                transform.position = center.position;

            }
        }
    }



}