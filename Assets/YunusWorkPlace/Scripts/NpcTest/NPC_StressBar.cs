using UnityEngine;
using Microlight.MicroBar;

public class NPC_StressBar : MonoBehaviour
{
    public MicroBar bar;

    public void Initialize(float max)
    {
        bar?.Initialize(max);
    }

    public void UpdateBar(float current)
    {
        bar?.UpdateBar(current);
    }
}
