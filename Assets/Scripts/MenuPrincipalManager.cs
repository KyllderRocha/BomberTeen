using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoLevel;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelGameOver;

    public static string MenuAtivo;

    private void Start()
    {
        MenuAtivo ??= "MenuInicial";
        AlterarMenu();
        Debug.Log(MenuAtivo);
    }

    public void Jogar()
    {
        SceneManager.LoadScene(nomeDoLevel);
    }

    public void AbrirOpcoes()
    {
        MenuAtivo = "Opcoes";
        AlterarMenu();
    }

    public void VoltarMenuPrincipal()
    {
        MenuAtivo = "MenuInicial";
        AlterarMenu();
    }

    public void AlterarMenu()
    {
        painelMenuInicial.SetActive(MenuAtivo == "MenuInicial");
        painelOpcoes.SetActive(MenuAtivo == "Opcoes");
        painelGameOver.SetActive(MenuAtivo == "GameOver");
    }

    public void SairJogo()
    {
        Debug.Log("Sair do Jogo");
        Application.Quit();
    }
}
