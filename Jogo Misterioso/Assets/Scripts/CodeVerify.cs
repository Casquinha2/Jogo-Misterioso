using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeVerify : MonoBehaviour
{
    public TextMeshProUGUI num1;
    public TextMeshProUGUI num2;
    public TextMeshProUGUI num3;

    int n1;
    int n2;
    int n3;

    public void codever()
    {
        n1 = 0;
        n2 = 0;
        n3 = 0;
        string val1 = num1.text;
        string val2 = num2.text;
        string val3 = num3.text;
        if (val1 == "4")
        {
            n1 = 1;
        } 
        if (val2 == "7")
        {
            n2 = 1;
        }
        if (val3 == "8")
        {
            n3 = 1;
        }

        int code = n1 + n2 + n3;
        if(code == 3)
        {
            Debug.Log("Código Correto");
        }
    }

}
