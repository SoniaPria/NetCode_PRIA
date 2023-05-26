using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    NetworkVariable<int> PlayerColor = new NetworkVariable<int>();

    [SerializeField]
    List<Material> playerColors;

    MeshRenderer mr;

    // void Start() { }

    public override void OnNetworkSpawn()
    {
        // Delegate que publica se una NetworkVariable muda de valor
        // Suscripción
        PlayerColor.OnValueChanged += OnColorChanged;

        mr = GetComponent<MeshRenderer>();

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
    }

    // --- Delegates
    public void OnColorChanged(int previous, int current)
    {
        // Debug.Log($"{gameObject.name}.OnColorChanged: Previous: {previous} | Current: {current}");
        mr.material = playerColors[PlayerColor.Value];
    }


    // --- Metodos locais

    public void MoveNeutral()
    {
        // A posición sincronízase en local
        // Co compoñente Network Transform

        float f2 = 0.1f;
        Vector3 tmpPosition = transform.position;
        tmpPosition.x = Random.Range(-1.5f, 1.5f + f2);
        transform.position = tmpPosition;
    }


    // --- ServerRpc's

    [ServerRpc]
    void InitializeServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayerColor.Value = 0;

        float f2 = 0.1f;

        // Posición aleatoria no taboleiro
        // No espazo central no que os xogadores non están en ningún equipo

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
            PlayerColor.Value = 1;
        }

        else if (transform.position.x > 1.5f)
        {
            // Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Azul");
            PlayerColor.Value = 2;
        }

        else
        {
            // Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Neutro");
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
        if (IsOwner)
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