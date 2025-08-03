using System.Collections;
using UnityEngine;


public class NPC_Stone : MonoBehaviour
{
    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        TriggerFalse();
        StartCoroutine(stoneControl());
    }

    private IEnumerator TriggerFalse()
    {
        yield return new WaitForSeconds(0.1f);
        sphereCollider.isTrigger = false;
    }

    private IEnumerator stoneControl()
    {

        yield return new WaitForSeconds(3.9f);
        rb.useGravity = true;
        yield return new WaitForSeconds(4);
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Human"))
        {
            rb.useGravity = true;
        }
    }
}
