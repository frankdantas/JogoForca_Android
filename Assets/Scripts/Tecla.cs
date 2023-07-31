using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tecla : MonoBehaviour
{
    public char letra;
    public TMP_Text txt;
    public GameController gameController;

    
    void Start()
    {
        txt.text = letra.ToString().ToUpper();
        gameObject.name = "Tecla_" + letra;
    }

    
    void Update()
    {
        
    }

    public void Mostrar()
    {
        gameObject.SetActive(true);
    }
    public void Esconder()
    {
        gameObject.SetActive(false);
    }

    public void onclick_apertar()
    {
        if (GameController.running)
        {
            gameController.ConfereLetra(letra);
            print($"Apertou {letra}");
            Esconder();
        }
        
    }
}
