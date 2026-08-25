using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Wrapper para facilitar as chamadas do Unity Netcode for GameObjects (NGO).
/// </summary>
public class BomberNetworkManager : MonoBehaviour
{
    public static BomberNetworkManager instance;
    private string nickname = "Player";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void LeaveRoom()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
    }

    public void ChangeNickname(string newNick)
    {
        nickname = newNick;
    }

    public string GetNickname()
    {
        return nickname;
    }

    public int GetPlayerCount()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            return NetworkManager.Singleton.ConnectedClientsIds.Count;
        }
        return 1;
    }
}
