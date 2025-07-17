using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birdAnimationController : MonoBehaviour
{

    public string[] idleAnimations;
    private Animator animator;
    public PigeonController pigeonController;
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ActionCycle());

    }

    public string GetRandomIdle()
    {
        return idleAnimations[Random.Range(0, idleAnimations.Length)];
    }

    IEnumerator ActionCycle()
    {
        while (true)
        {
            if (pigeonController.isFlying)
            {
                animator.SetTrigger("isFlying");
                yield break; // Uçuş başladıktan sonra döngüyü sonlandır
            }
            string idleAnim = GetRandomIdle();
            animator.Play(idleAnim);
            float duration = animator.GetCurrentAnimatorStateInfo(0).length;

            float timer = 0f;
            while (timer < duration)
            {
                if (pigeonController.isFlying)
                {
                    animator.SetTrigger("isFlying");
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            yield return null;  
        
        }
    }

}
