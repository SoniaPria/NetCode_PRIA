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

    void Start()
    {
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetStartPositionServerRpc();
        }
    }

    public void MoveNeutral()
    {
        Debug.Log($"{gameObject.name}.MoveNeutral");

        Vector3 tmpPosition = transform.position;
        tmpPosition.x = Random.Range(-1.5f, 1.5f);
        transform.position = tmpPosition;
    }

    [ClientRpc]
    public void MoveNeutralClientRpc(ClientRpcParams rpcParams = default)
    {
        MoveNeutral();
    }

    [ServerRpc]
    public void MoveNeutralServerRpc(ServerRpcParams rpcParams = default)
    {
        MoveNeutral();
    }


    [ServerRpc]
    public void SetTeemColorServerRpc(int zone = 0, ServerRpcParams clientRpcParams = default)
    {
        PlayerColor.Value = zone;
    }


    [ServerRpc]
    void SetStartPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        // Posición aleatoria no taboleiro
        // No espazo centra no que o xogador aínda non ten equipo
        transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 1f, Random.Range(-4.5f, 5f));
    }

    [ServerRpc]
    void MoveServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        // Posición enviada por Input de Player
        // Transform se propaga en rede sen Network variable
        transform.position += direction;
    }



    void Update()
    {
        if (!IsOwner) { return; }

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

        if (transform.position.x < -1.5f)
        {
            Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Vermello");
        }

        else if (transform.position.x > 1.5f)
        {
            Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Azul");
        }

        else
        {
            Debug.Log($"{gameObject.name} X = {transform.position.x} Equipo Neutro");
        }
    }
}