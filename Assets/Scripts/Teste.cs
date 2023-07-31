using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Teste : MonoBehaviour
{
    public TMP_Text texto;


    void Start()
    {
        StartCoroutine(ieAtualizTexto());
    }

    
    void Update()
    {
        
    }

    IEnumerator ieAtualizTexto()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            int rnd = Random.Range(0, 1000);
            texto.SetText(rnd.ToString());
        }
    }
}
