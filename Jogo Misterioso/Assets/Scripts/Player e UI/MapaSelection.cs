using UnityEngine;
using UnityEngine.SceneManagement;

public class MapaSelection : MonoBehaviour
{
    [SerializeField] GameObject pisoNeg1;
    [SerializeField] GameObject piso0;
    [SerializeField] GameObject piso1;
    [SerializeField] GameObject piso2;
    [SerializeField] GameObject menu;

    [SerializeField] TabController tabController;

    void OnEnable()
    {
        menu.SetActive(false);

        pisoNeg1.SetActive(false);
        piso0.SetActive(false);
        piso1.SetActive(false);
        piso2.SetActive(false);

        if (SceneManager.GetActiveScene().name == "Piso-1Scene")
        {
            pisoNeg1.SetActive(true);
        }
        else if (SceneManager.GetActiveScene().name == "Piso0Scene")
        {
            piso0.SetActive(true);
        }
        else if (SceneManager.GetActiveScene().name == "Piso1Scene")
        {
            piso1.SetActive(true);
        }
        else if (SceneManager.GetActiveScene().name == "Piso2Scene")
        {
            piso2.SetActive(true);
        }
    }
    public void ActivateMenu()
    {
        menu.SetActive(true);
        tabController.Back(2);
    }
}
