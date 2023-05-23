
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class Player : NetworkBehaviour
    {
        NetworkVariable<int> PlayerColor = new NetworkVariable<int>();
        MeshRenderer mr;


        // [SerializeField]
        // List<Material> playerColors;

        void Start()
        {
        }

        public override void OnNetworkSpawn()
        {
            Initialize();

            if (IsOwner)
            {
                // Para que non se espaneen no mesmo punto
                SetStartPositionServerRpc();
            }
        }

        void Initialize()
        {
            // PlayerColor.Value = 0;
            mr = GetComponent<MeshRenderer>();

            // Dev
            var ngo = GetComponent<NetworkObject>();
            string uid = ngo.NetworkObjectId.ToString();

            if (ngo.IsOwnedByServer)
            {
                gameObject.name = $"HostPlayer_{uid}";
            }
            else if (ngo.IsOwner)
            {
                gameObject.name = $"LocalPlayer_{uid}";
            }
            else
            {
                gameObject.name = "Net_Player_" + uid;
            }

            Debug.Log($"{gameObject.name}.Player");
            Debug.Log($"\t IsLocalPlayer: {ngo.IsLocalPlayer}");
            Debug.Log($"\t IsOwner: {ngo.IsOwner}");
            Debug.Log($"\t IsOwnedByServer: {ngo.IsOwnedByServer}");
            // --- end Dev
        }

        public void Move()
        {
            // Debug.Log("");
        }

        public void Print()
        {
            Debug.Log($"{gameObject.name}.Print");
            PrintServerRpc();
        }
        // void InitRandomPosition() { SubmitRandomPositionRequestServerRpc(); }

        [ServerRpc]
        void PrintServerRpc()
        {
            Debug.Log($"{gameObject.name}.PrintServerRpc");
        }


        [ServerRpc]
        void SetStartPositionServerRpc(ServerRpcParams rpcParams = default)
        {
            // Posición aleatoria no taboleiro
            // No espazo centra no que o xogador aínda non ten equipo
            transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 1f, Random.Range(-4.5f, 5f));
            PlayerColor.Value = 0;
            // mr.material = GameManager.instance.playerColors[0];

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
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input W | Input Up arrow");

                MoveServerRpc(Vector3.forward);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input D | Input Right arrow");

                MoveServerRpc(Vector3.right);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input S | Input Down arrow");

                MoveServerRpc(Vector3.back);
            }

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Debug.Log($"{gameObject.name}.Player.Update");
                // Debug.Log($"\t Input A | Input Left arrow");

                MoveServerRpc(Vector3.left);
            }
            if (mr.material.color != GameManager.instance.playerColors[PlayerColor.Value].color)
            {
                // Debug.Log($"{gameObject.name}.HelloWorldPlayer.Update Material");

                mr.material = GameManager.instance.playerColors[PlayerColor.Value];
            }
        }
    }
}