using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PeopleKickedOut : MonoBehaviour
{
        [SerializeField] private TextMeshProUGUI kickedText;
    [SerializeField] private int dayCount = 1; 
    private int totalPeopleCount;
    public int kickedCount = 0;
    private void Start()
    {
        totalPeopleCount = GameObject.FindGameObjectsWithTag("Human").Length;
        UpdateUIText();
    }
    private void Update()
    {
// Güncel "human" sayısını al
        int currentHumanCount = GameObject.FindGameObjectsWithTag("Human").Length;

        // Eksilen kişi varsa kickedCount güncellenir
        int newKicked = totalPeopleCount - currentHumanCount;
        if (newKicked != kickedCount)
        {
            kickedCount = newKicked;
            UpdateUIText();
        }
    }

    private void UpdateUIText()
    {

        kickedText.text = $"{kickedCount}/{totalPeopleCount}";
        
    }
}