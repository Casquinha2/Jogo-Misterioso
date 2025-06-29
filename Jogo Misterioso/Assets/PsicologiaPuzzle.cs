using UnityEngine;
using TMPro;
using System.Collections.Generic;  // para List<>
using System.Linq;

public class PsicologiaPuzzle : MonoBehaviour
{
    // lista pra guardar cada letra como GameObject
    [SerializeField] private List<TMP_Text> letras;
    private int progressao = 0;


    public void check(TMP_Text letra)
    {
        switch (progressao)
        {
            case 0:
                if (letra.text == "D")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 1:
                if (letra.text == "I")
                {
                    progressao++;
                    letra.color = new Color32(140, 255, 140, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 2:
                if (letra.text == "R")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 3:
                if (letra.text == "E")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 4:
                if (letra.text == "I")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 5:
                if (letra.text == "T")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 6:
                if (letra.text == "O")
                {
                    letra.color = new Color32(0, 255, 0, 255);
                    Debug.Log("ACERTOU");
                }
                else
                {
                    ResetarTudo();
                }
                break;
        }
    }
    
    private void ResetarTudo()
    {
        progressao = 0;
        foreach (var lt in letras)
            lt.color = new Color32(255, 255, 255, 255);
    }
}
