using System;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Wrapper para facilitar as chamadas do Unity Netcode for GameObjects (NGO).
/// Gerencia Host, Client, IP de conexão e detecção inteligente de instâncias locais.
/// </summary>
public class BomberNetworkManager : MonoBehaviour
{
    public static BomberNetworkManager instance;
    private string nickname = "Player";
    private string targetIp = "127.0.0.1";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        targetIp = PlayerPrefs.GetString("TargetIP", "127.0.0.1");
    }

    public void HostGame()
    {
        // Se já houver um Host nesta mesma máquina na porta 7777, entra automaticamente como Cliente
        if (IsHostRunningLocally())
        {
            Debug.LogWarning("[BomberTeen] Porta 7777 já em uso nesta máquina. Conectando como Cliente à sala existente...");
            JoinGame("127.0.0.1");
            return;
        }

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            ConfigureTransport("127.0.0.1", "0.0.0.0");
            NetworkManager.Singleton.StartHost();
        }
    }

    public void JoinGame(string ip = null)
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            if (!string.IsNullOrWhiteSpace(ip))
            {
                targetIp = ip.Trim();
                PlayerPrefs.SetString("TargetIP", targetIp);
            }

            ConfigureTransport(targetIp, null);
            NetworkManager.Singleton.StartClient();
        }
    }

    private void ConfigureTransport(string connectAddress, string listenAddress)
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                if (string.IsNullOrEmpty(listenAddress))
                {
                    transport.SetConnectionData(connectAddress, 7777);
                }
                else
                {
                    transport.SetConnectionData(connectAddress, 7777, listenAddress);
                }
            }
        }
    }

    public void LeaveRoom()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    public void ChangeNickname(string newNick)
    {
        if (!string.IsNullOrWhiteSpace(newNick))
        {
            nickname = newNick.Trim();
        }
    }

    public string GetNickname()
    {
        return nickname;
    }

    public string GetTargetIp()
    {
        return targetIp;
    }

    public void SetTargetIp(string ip)
    {
        if (!string.IsNullOrWhiteSpace(ip))
        {
            targetIp = ip.Trim();
            PlayerPrefs.SetString("TargetIP", targetIp);
        }
    }

    public int GetPlayerCount()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            return NetworkManager.Singleton.ConnectedClientsIds.Count;
        }
        return 1;
    }

    /// <summary>
    /// Verifica se a porta 7777 UDP já está ocupada nesta máquina (o que indica que outro Host local está rodando).
    /// </summary>
    public bool IsHostRunningLocally(int port = 7777)
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.ExclusiveAddressUse = true;
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Close();
                return false; // Conseguiu bindar -> porta livre -> nenhum Host local
            }
        }
        catch (SocketException)
        {
            return true; // Falhou ao bindar -> porta em uso -> Host local ativo!
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Obtém o IP da máquina na rede local (LAN/Wi-Fi) para exibir na tela do Host.
    /// </summary>
    public static string GetLocalIPAddress()
    {
        try
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                if (socket.LocalEndPoint is IPEndPoint endPoint)
                {
                    return endPoint.Address.ToString();
                }
            }
        }
        catch
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
        }
        return "127.0.0.1";
    }
}
