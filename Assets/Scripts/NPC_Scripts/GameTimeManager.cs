using UnityEngine;
using System;


namespace NPC_Scripts
{
   
    
public class GameTimeManager : MonoBehaviour
{
    
    

    private float lastReportedMinute = -1;
    public static GameTimeManager Instance;
    private float dayProgress;
    public float dayDurationInSeconds = 600f; // 10 dakika = 1 gün
    private float timer;

    public enum DayActivity
    {
        Roaming,
        Sport,
        Breakfast,
        PoolOrSit,
        Lunch,
        Dinner,
        GoInside,
        None
    }

    public DayActivity CurrentActivity
    {
        get;
        private set;
    } = DayActivity.None;

    public enum MealTime { None, Breakfast, Lunch, Dinner }

    public MealTime CurrentMealTime { get; private set; } = MealTime.None;

    void Awake()
    {
        Instance = this;
    }

    public static event Action<int> OnMinuteChanged;

   

    
    void Update()
    {
        timer += Time.deltaTime;
        dayProgress = timer / dayDurationInSeconds;

        float currentMinute = Mathf.Floor(Time.time / 60f);
        if (currentMinute != lastReportedMinute)
        {
            lastReportedMinute = currentMinute;
            OnMinuteChanged?.Invoke((int)currentMinute);
        }

        // Aktif aktivite kontrolü (dakikaya göre)
        int minute = (int)(timer / 60f);
        switch (minute)
        {
            case 0:
                CurrentActivity = DayActivity.Roaming;
                break;
            case 1:
                CurrentActivity = DayActivity.Sport;
                break;
            case 2:
                CurrentActivity = DayActivity.Breakfast;
                break;
            case 3:
            case 4:
                CurrentActivity = DayActivity.PoolOrSit;
                break;
            case 5:
                CurrentActivity = DayActivity.Lunch;
                break;
            case 6:
            case 7:
            case 8:
                CurrentActivity = DayActivity.PoolOrSit;
                break;
            case 9:
                CurrentActivity = DayActivity.Dinner;
                break;
            case 10:
                CurrentActivity = DayActivity.GoInside;
                break;
            default:
                CurrentActivity = DayActivity.None;
                break;
        }

        // Eski MealTime da bırakılabilir istersek
        if (dayProgress < 0.1f)
            CurrentMealTime = MealTime.Breakfast;
        else if (dayProgress < 0.2f)
            CurrentMealTime = MealTime.Lunch;
        else if (dayProgress < 0.3f)
            CurrentMealTime = MealTime.Dinner;
        else
            CurrentMealTime = MealTime.None;

        if (dayProgress >= 1f)
            timer = 0f; // Yeni gün

        // 🧠 DEBUG: zaman ve aktivite bilgisi
        if (Debug.isDebugBuild)
        {
            int seconds = Mathf.FloorToInt(timer);
         //   Debug.Log($"⏱ Zaman: {seconds}s — Gün İlerlemesi: {dayProgress:F2} — 🔁 Aktif Aktivite: {CurrentActivity}");
        }
    }
    
    public float CurrentTime => timer;
    
    
}



}
