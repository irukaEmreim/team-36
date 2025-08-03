using System.Collections;
using Microlight.MicroBar;
using UnityEngine;
using UnityEngine.AI;

public class DoorAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        animator.SetTrigger("OpenDoor");
        animator.SetBool("restart", false);
    }

    public void CloseDoor()
    {
        animator.SetTrigger("CloseDoor");
    }

    private IEnumerator restartAnim()
    {
        yield return new WaitForSeconds(5f);
        animator.SetBool("restart", true);
    }

}
