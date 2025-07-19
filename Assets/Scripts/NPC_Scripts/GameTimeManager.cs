using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    public float dayDurationInSeconds = 600f; // 10 dakika = 1 gün
    private float timer;

    public enum MealTime { None, Breakfast, Lunch, Dinner }

    public MealTime CurrentMealTime { get; private set; } = MealTime.None;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float dayProgress = timer / dayDurationInSeconds;

        if (dayProgress < 0.1f)
            CurrentMealTime = MealTime.Breakfast;
        else if (dayProgress < 0.2f)
            CurrentMealTime = MealTime.Lunch;
        else if (dayProgress < 0.3f)
            CurrentMealTime = MealTime.Dinner;
        else
            CurrentMealTime = MealTime.None;

        if (dayProgress >= 1f)
            timer = 0f; // yeni gün

        // 🧠 DEBUG: anlık zaman ve yemek bilgisi
        if (Debug.isDebugBuild)
        {
            int seconds = Mathf.FloorToInt(timer);
            Debug.Log($"⏱ Zaman: {seconds}s — Gün İlerlemesi: {dayProgress:F2} — Aktif Yemek: {CurrentMealTime}");
        }
    }
    
    public float CurrentTime => timer;
}


