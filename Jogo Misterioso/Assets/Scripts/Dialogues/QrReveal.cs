using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QrReveal : MonoBehaviour
{
    public GameObject qr;

    [HideInInspector]
    public int totalClicks;

    [SerializeField]
    private int clicksNecessarios = 3;
    void Start()
    {
        Debug.Log("Comecou o start do qr");
        totalClicks = 0;
    }
    public void AddClicksQR()
    {
        totalClicks++;
        if (totalClicks >= clicksNecessarios)
        {

            qr.SetActive(false);
            Debug.Log("Apareceu");
            totalClicks = 0;
        }
        Debug.Log("Main um click");
    }
}
