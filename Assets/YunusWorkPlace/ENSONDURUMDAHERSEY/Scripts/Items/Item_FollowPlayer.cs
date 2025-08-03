using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_FollowPlayer : MonoBehaviour
{
    [Header("Taşınacak Noktalar")]
    public List<Transform> stoneTransforms; // 0 = left, 1 = right
    public Transform itemTransform;
    public GameObject player;

    [Header("Durumlar")]
    public bool isTransforming = false;   // takipte mi?
    public bool isCollected = false;      // bir kez bile toplandı mı?
    public int carryingIndex = -1;        // taş ise 0 veya 1 olabilir
    public string realTag;                // düşme sonrası geri almak için

    public Collider someTimesTriggerCollider;
    private TrailRenderer trailRenderer;

    private Transform left, right;

    private Rigidbody rb;

    private float velocityMagnitude;
    private bool dropInProgress = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        if (stoneTransforms.Count == 2)
        {
            left = stoneTransforms[0];
            right = stoneTransforms[1];
        }
        else
        {
          //  print("Taş Tasinacak Yerler Atanmamis");
        }
    }


    private float maxTimer = 20f;
    private float _timer = 0f;
    private bool startTimer = false;
    public GameObject owner;
    private void Update()
    {
        velocityMagnitude = rb.velocity.magnitude;

        if (isTransforming)
        {
            isCollected = true;
            rb.useGravity = false;
            someTimesTriggerCollider.isTrigger = true;
        }
        else if (isCollected && !isTransforming && !dropInProgress)
        {
            StartCoroutine(HandleDrop());
        }

        if (CompareTag("Thorn") && isTransforming)
        {
            AlignThornRotation();
        }
        if (startTimer && (gameObject.CompareTag("CurukMeyve") || gameObject.CompareTag("Thorn")))
        {
            tag = "Untagged";
            _timer += Time.deltaTime;
            if (_timer >= maxTimer)
            {
                Destroy(this.gameObject);
            }
        }

        if (this.gameObject.CompareTag("Diamond") && isCollected)
        {
            var guestRoutine = owner.GetComponent<GuestDailyRoutine>();
            if (guestRoutine != null)
                guestRoutine.OnDiamondStolen();

            cancelCollected();
        }

    }

    private IEnumerator cancelCollected()
    {
        yield return new WaitForSeconds(8);
        isCollected = false;
    }

    private void LateUpdate()
    {
        if (!isTransforming)
        {
            return;
        }

        if (CompareTag("Stone"))
        {
            transform.position = carryingIndex == 0 ? left.position : right.position;
        }
        else if (CompareTag("Diamond") || CompareTag("Thorn") || CompareTag("CurukMeyve"))
        {
            transform.position = itemTransform.position;
        }

    }

    IEnumerator HandleDrop()
    {
        trailRenderer.enabled = true;
        dropInProgress = true;
        isCollected = false;
        rb.useGravity = true;
        rb.freezeRotation = false;

        transform.parent = null;
        tag = "Untagged";

        yield return new WaitForSeconds(0.2f);
        someTimesTriggerCollider.isTrigger = false;

        yield return new WaitUntil(() => velocityMagnitude <= 0.25f);


        print("AFKDSF");
        startTimer = true;
        

        tag = realTag;
        trailRenderer.enabled = false;
        dropInProgress = false;
    }

    private void AlignThornRotation()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        rot.x = 0f;
        rot.y = player.transform.rotation.eulerAngles.y;
        rot.z = 90f;
        transform.rotation = Quaternion.Euler(rot);
    }

}
