using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumManager2 : MonoBehaviour
{
    public TextMeshProUGUI ClicksCount2;

    float TotalClicks2;
    public void AddClicks2()
    {
        TotalClicks2++;
        if (TotalClicks2 > 9)
        {
            TotalClicks2 = 0;
        }
        ClicksCount2.text = TotalClicks2.ToString("0");

    }
}
