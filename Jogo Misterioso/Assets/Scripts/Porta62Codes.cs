using UnityEngine;
using TMPro;
public class Porta62Codes : MonoBehaviour
{
    public TextMeshProUGUI num1;
    public TextMeshProUGUI num2;
    public TextMeshProUGUI num3;

    private int totalClicks1, totalClicks2, totalClicks3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        num1.text = "0";
        num2.text = "0";
        num3.text = "0";
    }
    public void AddClicks1()
    {
        totalClicks1++;
        if (totalClicks1 > 9)
        {
            totalClicks1 = 0;
        }
        num1.text = totalClicks1.ToString();

    }
    public void AddClicks2()
    {
        totalClicks2++;
        if (totalClicks2 > 9)
        {
            totalClicks2 = 0;
        }
        num2.text = totalClicks2.ToString();

    }
    public void AddClicks3()
    {
        totalClicks3++;
        if (totalClicks3 > 9)
        {
            totalClicks3 = 0;
        }
        num3.text = totalClicks3.ToString();
    }

    public void CodeVerification()
    {
        if (num1.text == "8" && num2.text == "4" && num3.text == "7")
        {
            Debug.Log("C�digo Correto");
        }
    }
}
