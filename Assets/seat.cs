using UnityEngine;

[ExecuteAlways]
public class seat : MonoBehaviour
{
    public string seatName;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.2f);

        // 🔠 Sandalyeye isim verildiyse yazı olarak göster
        if (!string.IsNullOrEmpty(seatName))
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, seatName);
        }
    }
}