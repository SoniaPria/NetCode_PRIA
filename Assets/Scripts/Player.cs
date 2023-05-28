using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    NetworkVariable<int> PlayerColor = new NetworkVariable<int>();
    NetworkVariable<bool> PlayerCanMove = new NetworkVariable<bool>();
    NetworkVariable<int> PlayerTeam = new NetworkVariable<int>();

    [SerializeField]
    List<Material> playerColors;

    MeshRenderer mr;

    // Propiedade pública para o GameManager
    public int NwPlayerTeam { get { return PlayerTeam.Value; } }

    // void Start() { }

    public override void OnNetworkSpawn()
    {
        Initialize();

        if (IsOwner)
        {
            InitializeServerRpc();
        }
        else
        {
            mr.material = playerColors[PlayerColor.Value];
        }
    }

    public override void OnNetworkDespawn()
    {
        // Des-suscripción ao Delegate cando o player pecha sesión
        PlayerColor.OnValueChanged -= OnColorChanged;
        PlayerTeam.OnValueChanged -= OnTeamChanged;
    }

    // --- Delegates
    public void OnColorChanged(int previous, int current)
    {
        // Debug.Log($"{gameObject.name}.OnColorChanged: Previous: {previous} | Current: {current}");
        mr.material = playerColors[PlayerColor.Value];
    }

    public void OnTeamChanged(int previous, int current)
    {
        GameManager.instance.CheckPlayersMovePermission(previous, current);
    }


    // --- Metodos locais

    void Initialize()
    {
        // Agregamos o player espaneado ao equipo 0 (zona neutra)
        GameManager.instance.PlayersTeam[0]++;

        // Get Components
        mr = GetComponent<MeshRenderer>();

        // Suscripción a Delegates
        PlayerColor.OnValueChanged += OnColorChanged;
        PlayerTeam.OnValueChanged += OnTeamChanged;
    }

    public void MoveNeutral()
    {
        // A posición sincronízase en local
        // Co compoñente Network Transform

        float f2 = 0.1f;
        Vector3 tmpPosition = transform.position;
        tmpPosition.x = Random.Range(-1.5f, 1.5f + f2);
        transform.position = tmpPosition;
    }

    int GetRandomTeamColor(int rdmMmin, int rdmMax)
    {
        // Debug.Log($"{gameObject.name}.Player.SetRandomColor");

        int rdmColor;
        bool takenColor = false;

        do
        {
            rdmColor = Random.Range(rdmMmin, rdmMax + 1);
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var player = NetworkManager.Singleton.SpawnManager
                    .GetPlayerNetworkObject(uid).GetComponent<Player>();

                takenColor = false;

                if (rdmColor == player.PlayerColor.Value)
                {
                    takenColor = true;
                    break;
                }
            }
        } while (takenColor);

        return rdmColor;
    }



    // --- ClientRpc's

    [ClientRpc]
    public void SetPlayerCanMoveClientRpc(bool canMove, ClientRpcParams clientRpcParams = default)
    {
        PlayerCanMove.Value = canMove;
    }

    // --- ServerRpc's

    [ServerRpc]
    void InitializeServerRpc(ServerRpcParams rpcParams = default)
    {
        // Initialize Network Variables
        PlayerColor.Value = 0;
        PlayerTeam.Value = 0;
        PlayerCanMove.Value = true;

        // Posición aleatoria no taboleiro
        // No espazo central no que os xogadores están no equipo 0

        float f2 = 0.1f;
        transform.position = new Vector3(
            Random.Range(-1.5f, 1.5f + f2),
            1f,
            Random.Range(-4.5f, 4.5f + f2)
        );
    }

    [ServerRpc]
    void MoveServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        // Posición enviada por Input de Player
        transform.position += direction;

        if (transform.position.x < -1.5f)
        {
            // Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Vermello");
            PlayerTeam.Value = 1;
            PlayerColor.Value = GetRandomTeamColor(1, 3);
        }

        else if (transform.position.x > 1.5f)
        {
            // Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Azul");
            PlayerTeam.Value = 2;
            PlayerColor.Value = GetRandomTeamColor(4, 6);
        }

        else
        {
            // Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Neutro");
            PlayerTeam.Value = 0;
            PlayerColor.Value = 0;
        }
    }

    [ServerRpc]
    public void MoveNeutralServerRpc(ServerRpcParams rpcParams = default)
    {
        MoveNeutral();
    }


    void Update()
    {
        // Debug.Log($"{gameObject.name} CanMove? {PlayerCanMove.Value}");

        if (IsOwner && PlayerCanMove.Value)
        {
            if (Input.GetKeyDown(KeyCode.M)) { MoveNeutralServerRpc(); }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveServerRpc(Vector3.forward);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveServerRpc(Vector3.right);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveServerRpc(Vector3.back);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveServerRpc(Vector3.left);
            }
        }
    }
}