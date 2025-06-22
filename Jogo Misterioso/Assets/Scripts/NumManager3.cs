using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NumManager3 : MonoBehaviour
{
    public TextMeshProUGUI ClicksCount3;

    float TotalClicks3;
    public void AddClicks3()
    {
        TotalClicks3++;
        if (TotalClicks3 > 9)
        {
            TotalClicks3 = 0;
        }
        ClicksCount3.text = TotalClicks3.ToString("0");

    }
}
