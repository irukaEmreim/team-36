using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crow_DirectAttack : Crow_Base
{
    [Header("Gak Ayarları")]
    public AudioClip[] audioClips;
    public List<GameObject> humans = new List<GameObject>();


    public void GakAttack()
    {
        if (audioClips.Length > 0)
        {
            audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.Play();
        }

        Debug.Log("GAK ATTACK");

        foreach (var human in humans)
        {
            if (human != null)
            {
                human.GetComponent<BaseNPC>()?.TakeDamage(15);
                Debug.Log($"{human.name} 15 hasar aldı!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Human") && !humans.Contains(other.gameObject))
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
}
