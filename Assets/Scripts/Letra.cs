using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using System.Linq;
using UnityEditor;

public class Letra : MonoBehaviour
{
    public char letra;

    public TextMeshProUGUI texto;
    private bool oculta = true;

    public enum CorLetra
    {
        Verde,
        Vermelha,
        Nenhuma
    }

    void Start()
    {
        texto.SetText("?");
        texto.ForceMeshUpdate(true);
        oculta = true;

        //StartCoroutine(ieAtualizTexto());
    }

    public char GetLetra()
    {
        char[] letrasA = new char[] { 'A', 'À', 'Á', 'Ã', 'Â'};
        char[] letrasE = new char[] { 'E', 'È', 'É', 'Ê'};
        char[] letrasI = new char[] { 'I', 'Ì', 'Í', 'Î'};
        char[] letrasO = new char[] { 'O', 'Ò', 'Ó', 'Õ', 'Ô' };
        char[] letrasU = new char[] { 'U', 'Ù', 'Ú', 'Û' };


        if (letrasA.Contains(letra))
        {
            print("A diferente");
            return letrasA[0];
        }else if (letrasE.Contains(letra))
        {
            print("E diferente");
            return letrasE[0];
        }else if (letrasI.Contains(letra))
        {
            print("I diferente");
            return letrasI[0];
        }else if (letrasO.Contains(letra))
        {
            print("O diferente");
            return letrasO[0];
        }else if (letrasU.Contains(letra))
        {
            print("U diferente");
            return letrasU[0];
        }
        print("Resto");
        return letra;

        //return (char)letra.ToString().Normalize(NormalizationForm.FormD)[0];
    }

    public void SetLetra(char c)
    {
        letra = c.ToString().ToUpper()[0];
        if(letra == '-' || letra == ' ')
        {
            MostrarLetra();
        }

        gameObject.name = $"Letra {letra}";
    }

    public void MostrarLetra()
    {
        texto.SetText(letra.ToString());
        texto.ForceMeshUpdate(true);
        oculta = false;

        if (letra == '-' || letra == ' ')
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void ExplodirLetra(CorLetra cor)
    {
        if(cor == CorLetra.Verde)
        {
            texto.color = CorFromRGB(88,214,141);
        }else if(cor == CorLetra.Vermelha)
        {
            texto.color = CorFromRGB(236,112,99);
        }
        StartCoroutine(ieMostrar());
    }

    IEnumerator ieMostrar()
    {
        yield return new WaitForSeconds(0.01f);
        MostrarLetra();
    }

    public bool GetOculta()
    {
        return oculta;
    }

    private Color CorFromRGB(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
}
