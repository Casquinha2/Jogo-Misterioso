using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QReveal : MonoBehaviour
{
    public GameObject Qr;
    public int TotalClicks;
    void Start()
    {
        Debug.Log("Comecou o start do qr");
        TotalClicks = 0;
    }
    public void AddClicksQR()
    {
        TotalClicks++;
        if (TotalClicks >= 3)
        {

            Qr.SetActive(false);
            Debug.Log("Apareceu");
            TotalClicks = 0;
        }
        Debug.Log("Main um click");
    }
}
