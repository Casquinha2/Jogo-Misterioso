using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QReveal : MonoBehaviour
{
    public GameObject QR;
    float TotalClicks1;
    public void AddClicksQR()
    {
        TotalClicks1++;
        if (TotalClicks1 == 3)
        {
            Destroy(QR);  
        }
    }
}
