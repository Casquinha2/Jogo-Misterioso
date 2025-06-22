using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumManager1 : MonoBehaviour
{
    public TextMeshProUGUI ClicksCount1;

    float TotalClicks1;
    public void AddClicks()
    {
        TotalClicks1++;
        if (TotalClicks1 > 9)
        {
            TotalClicks1 = 0;
        }
        ClicksCount1.text = TotalClicks1.ToString("0");
        
    }

}
