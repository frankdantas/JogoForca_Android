using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine.Networking;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft;
using Newtonsoft.Json;
using System.Xml;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameController : MonoBehaviour
{
    [Header("Telas")]
    public GameObject painelMenu;
    public GameObject painelJogo;

    [Header("Tela menu")]
    public TMP_Text txtMensagemInicio;
    public TMP_Text txtRecord;
    public Button btnStart;

    [Header("Tela jogo")]
    public GameObject[] partesCorpo;
    public GameObject prefabLetra;
    public Transform contentLetras;
    public TMP_Text txtMensagem;
    public TMP_Text txtDica;
    public GameObject btnRestart;
    public GameObject btnDica;
    public Transform contentLetrasDigitadas;
    public TMP_Text txtScore;
    [Header("Sons")]
    public AudioSource somGanhou;
    public AudioSource somPerdeu;
    public AudioSource somDica;
    [Space]
    public int dicasRestantes = 3;
    public TMP_Text txtDicaButton;
    public int score = 200;
    public static int savedScore = 0;


    private List<Letra> listaPrefabLetras = new List<Letra>();
    //private List<char> letrasDigitadas = new List<char>();
    private string palavra;
    private string dica;
    private string palavraNormal;

    private int mostrandoParte = -1;
    private int acertos = 0;

    public static bool running = false;

    private bool conferirHifen = true;

    void Start()
    {
        running = false;

        painelJogo.SetActive(false);
        painelMenu.SetActive(true);
        MostrarMensagem("Bom jogo!");
        txtMensagemInicio.text = "Pronto para iniciar";
        AtualizaBotaoDica();

        if (PlayerPrefs.HasKey("Record"))
        {
            int record = PlayerPrefs.GetInt("Record", 0);
            txtRecord.text = $"Record: {record}";
        }
        else
        {
            txtRecord.text = "";
        }

        //StartCoroutine(ieSortearPalavra());
    }

    void OnGUI()
    {

        if (GUI.Button(new Rect(0, 0, 100, 100), "Click"))
        {
            List<Letra> letrasFaltantes = listaPrefabLetras.Where(x => x.GetOculta()).ToList();
            string letras = string.Join('.', letrasFaltantes.Select(x => x.GetLetra()).ToArray());
            print("Falta: " + letras);
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (!running)
        {
            return;
        }

        txtScore.SetText($"Score: {score}");
        txtScore.ForceMeshUpdate();

        if (conferirHifen)
        {
            conferirHifen = false;
            ConfereHifen();
        }

    }

    IEnumerator ieTimer()
    {
        while (running)
        {
            yield return new WaitForSeconds(1f);
            score--;

            if (score <= 0)
            {
                running = false;
                MostrarMensagem("Acabou o tempo :\\");
                somPerdeu.Play();
                StartCoroutine(ieMostrarPalavra());
                txtScore.text = $"Score: 0";
            }

        }
    }

    void AtualizaBotaoDica()
    {
        txtDicaButton.text = $"Dica ({dicasRestantes})";
    }

    public void ConfereLetra(char letra)
    {
        print("Apertou: " + letra);
        letra = letra.ToString().ToUpper()[0];

        Letra.CorLetra cor = Letra.CorLetra.Verde;
        
        if (palavraNormal.Contains(letra.ToString()))
        {
            if(letra != '-' && letra != ' ')
            {
                MostrarMensagem($"Essa tem :)");
            }
            
            
            foreach (var item in listaPrefabLetras)
            {
                if (item.GetLetra() == letra)
                {
                    if (letra == '-' || letra == ' ')
                    {
                        item.ExplodirLetra(Letra.CorLetra.Verde);
                    }
                    else
                    {
                        item.MostrarLetra();
                        score += 3;
                    }
                        
                    acertos++;
                    
                    if (acertos == palavra.Length)
                    {
                        running = false;
                        print("Acabou!!!");
                        MostrarMensagem("Parabéns!! Você venceu.");
                        somGanhou.Play();
                        SalvarRecord();
                    }
                }
                else
                {
                    //print($"{letra} diferente de {item.GetLetra()}");
                }
            }

        }
        else
        {
            MostrarMensagem("Não tem essa letra :\\");
            score -= 5;
            cor = Letra.CorLetra.Vermelha;
            MostrarErro();
        }

        if (letra != '-' && letra != ' ')
        {
            GameObject copiaLetra = Instantiate(prefabLetra, contentLetrasDigitadas.position, Quaternion.identity, contentLetrasDigitadas);
            Letra letraComp = copiaLetra.GetComponent<Letra>();
            letraComp.SetLetra(letra);
            letraComp.ExplodirLetra(cor);
        }

        ChecaBotaoDica();
    }

    void SalvarRecord()
    {
        //savedScore += score;
        txtScore.text = $"Score: {score}";
        txtScore.ForceMeshUpdate();

        int anterior = PlayerPrefs.GetInt("Record", 0);
        if (score > anterior)
        {
            PlayerPrefs.SetInt("Record", score);
            MostrarMensagem("Novo record atingido");
        }
    }

    IEnumerator ieSortearPalavra()
    {
        string url = "https://api.dicionario-aberto.net/random";
        txtMensagemInicio.text = "Sorteando palavra...";
        btnStart.interactable = false;
        //btnRestart.SetActive(false);

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            print("Buscou palavra");
            string resposta = req.downloadHandler.text;
            print(resposta);
            PalavraRandom palavraAleatoria = JsonConvert.DeserializeObject<PalavraRandom>(resposta);
            print(palavraAleatoria.word);
            palavra = palavraAleatoria.word.ToUpper();
            palavraNormal = removerAcentos(palavra);

            url = "https://api.dicionario-aberto.net/word/" + palavraAleatoria.word;
            req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                resposta = req.downloadHandler.text;
                print(resposta);
                SignificadoCompleto[] significados = JsonConvert.DeserializeObject<SignificadoCompleto[]>(resposta);
                if (significados.Length > 0)
                {
                    string sig = significados[0].GetSignificado();
                    print(sig);
                    dica = sig;
                    txtMensagemInicio.text = "Pronto para jogar";
                    Iniciar();
                    btnStart.interactable = true;
                }

            }
            else
            {
                print("Erro ao buscar");
                txtMensagemInicio.text = "Verifique sua conex�o";
            }

        }
        else
        {
            print("Erro ao buscar");
            txtMensagemInicio.text = "Verifique sua conex�o";
        }
    }

    void ieSortearPalavraTeste()
    {
        //yield return new WaitForSeconds(0.1f);

        List<Palavra> lista = new List<Palavra>();

        lista.Add(new Palavra("Paralelepípedo", "Paralelepípedo"));


        lista.Add(new Palavra("Ana", "Ana"));
        lista.Add(new Palavra("Berinjela", "Berinjela"));
        lista.Add(new Palavra("Normal", "Normal"));

        /*
        lista.Add(new Palavra("Sessão", "Sessao"));
        lista.Add(new Palavra("Mamão", "mamao"));
        lista.Add(new Palavra("Ômega", "Omega"));
        lista.Add(new Palavra("Paralelepípedo", "Paralelepípedo"));
        lista.Add(new Palavra("ùruguai", "ùruguai"));
        lista.Add(new Palavra("úruguaiana", "úruguaiana"));
        lista.Add(new Palavra("Nhõnho", "Nhõnho"));
        lista.Add(new Palavra("Melância", "Melância"));
        lista.Add(new Palavra("Mistério", "Mistério"));


        lista.Add(new Palavra("Guarda-chuva", "guarda-chuva"));
        lista.Add(new Palavra("São Paulo", "São Paulo"));
        lista.Add(new Palavra("Marca-passo", "Marca-passo"));
        lista.Add(new Palavra("Abra cadabra", "Abra cadabra"));
        
        */

        int indexSorteio = Random.Range(0, lista.Count);
        Palavra sorteada = lista[indexSorteio];



        txtMensagemInicio.text = "Sorteando palavra...";
        btnStart.interactable = false;

        palavra = sorteada.Descricao.ToUpper();
        dica = sorteada.Significado;
        palavraNormal = removerAcentos(palavra);

        txtMensagemInicio.text = "Pronto para jogar";
        
        btnStart.interactable = true;
        Iniciar();
    }

    IEnumerator ieMostrarPalavra()
    {
        yield return new WaitForSeconds(0.01f);
        foreach (var item in listaPrefabLetras)
        {
            if (item.GetOculta())
            {
                item.MostrarLetra();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void MostrarMensagem(string msg)
    {
        print(msg);
        txtMensagem.text = msg;
    }

    public void MostrarErro()
    {
        mostrandoParte++;
        partesCorpo[mostrandoParte].SetActive(true);
        if (mostrandoParte == partesCorpo.Length - 1)
        {
            running = false;
            score = 0;
            print("Acabou o jogo");
            MostrarMensagem("Acabou o jogo. Não foi dessa vez.");
            somPerdeu.Play();
            StartCoroutine(ieMostrarPalavra());
            SalvarRecord();
        }
    }

    void ConfereHifen()
    {
        foreach (var item in listaPrefabLetras)
        {
            if (item.GetLetra() == '-')
            {
                print("Conferindo traco");
                ConfereLetra('-');
            }
            if (item.GetLetra() == ' ')
            {
                print("Conferindo espaço");
                ConfereLetra(' ');
            }
        }
    }

    private void ChecaBotaoDica()
    {
        List<Letra> letrasFaltantes = listaPrefabLetras.Where(x => x.GetOculta()).ToList();
        if (letrasFaltantes.Count <= 3 || dicasRestantes <= 0 || !running)
        {
            btnDica.SetActive(false);
        }
        else
        {
            btnDica.SetActive(true);
        }

        print($"{letrasFaltantes.Count} ... {dicasRestantes <= 0} ... {!running}");

    }

    public void onclick_mostrarDica()
    {
        dicasRestantes--;

        List<Letra> letrasFaltantes = listaPrefabLetras.Where(x => x.GetOculta()).ToList();
        if (letrasFaltantes.Count > 3 && dicasRestantes >= 0)
        {
            score -= 8;
            somDica.Play();
            int sorteioIndex = Random.Range(0, letrasFaltantes.Count);
            Letra sorteio = letrasFaltantes[sorteioIndex];
            ConfereLetra(sorteio.GetLetra());
            AtualizaBotaoDica();
        }
        else
        {
            MostrarMensagem("Sem dica para mostrar");
        }

        ChecaBotaoDica();

    }

    public void onclick_restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Iniciar()
    {
        listaPrefabLetras = new List<Letra>();

        foreach (var item in partesCorpo)
        {
            item.SetActive(false);
        }

        for (int i = 0; i < palavra.Length; i++)
        {
            GameObject copiaLetra = Instantiate(prefabLetra, contentLetras.position, Quaternion.identity, contentLetras);
            Letra letraComponent = copiaLetra.GetComponent<Letra>();
            listaPrefabLetras.Add(letraComponent);
            letraComponent.SetLetra(palavra[i]);
        }

        

        //ConfereHifen();

        txtDica.text = "DICA: " + dica.Trim();


        painelJogo.SetActive(true);
        painelMenu.SetActive(false);
        running = true;
        ChecaBotaoDica();
        StartCoroutine(ieTimer());
    }

    public void onclick_iniciarJogo()
    {

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            txtMensagemInicio.text = "Verifique a conexão com a internet";
        }
        else
        {
            StartCoroutine(ieSortearPalavra());
            //ieSortearPalavraTeste();
        }

    }

    public string removerAcentos(string texto)
    {
        string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
        string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

        for (int i = 0; i < comAcentos.Length; i++)
        {
            texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
        }
        return texto;
    }
}

[System.Serializable]
public class Palavra
{
    public string Descricao;
    public string Significado;


    public Palavra(string descricao, string significado)
    {
        Descricao = descricao;
        Significado = significado;
    }
}