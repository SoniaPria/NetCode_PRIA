using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    // Nº de equipos
    [SerializeField] const int MAX_TEAMS = 2;
    // Máximo de players por equipo
    [SerializeField] const int MAX_TEAM_PLAYERS = 2;

    // Lista de xogadores por equipo
    // o index 0 é a zona neutra, os seguintes son os nº de equipo
    static List<int> playersTeam = new List<int>();

    public List<int> PlayersTeam
    {
        get { return playersTeam; }
        set { playersTeam = value; }
    }

    public int MaxTeamPlayers { get { return MAX_TEAM_PLAYERS; } }


    void OnEnable()
    {
        // Simple Singleton
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        instance = this;

        // Inicialización de equipos, MAX_TEAMS + Neutral
        // e xogadores por equipo (ningún)
        for (int i = 0; i <= MAX_TEAMS; i++)
        {
            // Inicialización de players por equipo
            playersTeam.Add(0);
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SubmitNeutralZone();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    static void SubmitNeutralZone()
    {
        if (GUILayout.Button((NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            ? "Todos a Inicio" : "Mover a inicio"))
        {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid)
                        .GetComponent<Player>()
                        .MoveNeutral();
                }
            }
            else
            {
                NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()
                    .GetComponent<Player>()
                    .MoveNeutralServerRpc();
            }
        }
    }
}