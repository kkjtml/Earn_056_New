using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Threading;
using Unity.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;
    public NetworkVariable<bool> isEyeRed = new NetworkVariable<bool>(false,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public TMP_Text namePrefab;
    private TMP_Text nameLabel;
    private NetworkVariable<int> postX = new NetworkVariable<int>(0,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
    new NetworkString { info = "Player" }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player" }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private LoginManager loginManager;

    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }

        public override string ToString()
        {
            return info.ToString();
        }

        public static implicit operator NetworkString(string v) =>
            new NetworkString() { info = new FixedString32Bytes(v) };
    }

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (nameLabel != null)
            nameLabel.enabled = true;
    }

    private void OnDisable()
    {
        if (nameLabel != null)
            nameLabel.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity);
        nameLabel.transform.SetParent(canvas.transform);
        base.OnNetworkSpawn();

        postX.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log("OwnerID = " + OwnerClientId + " : post x = " + postX.Value);
        };

        playerNameA.OnValueChanged += (NetworkString previousValue, NetworkString newValue) =>
        {
            Debug.Log("OwerId = " + OwnerClientId + " : Old name = " + previousValue.info + " : New name = " + newValue.info);
        };

        playerNameB.OnValueChanged += (NetworkString previousValue, NetworkString newValue) =>
        {
            Debug.Log("OwerId = " + OwnerClientId + " : Old name = " + previousValue.info + " : New name = " + newValue.info);
        };

        if (IsOwner)
        {
            loginManager = GameObject.FindObjectOfType<LoginManager>();
            if (loginManager != null)
            {
                string name = loginManager.userNameInput.text;

                if (IsOwnedByServer)
                    SetPlayerNameServerRpc(name, true);
                else
                    SetPlayerNameServerRpc(name, false);
            }
        }
    }

    private void Update()
    {
        // if (IsOwner)
        // {
        //     postX.Value = (int)System.Math.Ceiling(transform.position.x);

        //     if (Input.GetKeyDown(KeyCode.F))
        //     {
        //         RequestChangeEyeColorServerRpc();
        //     }
        // }

        if (IsOwner && Input.GetKeyDown(KeyCode.F))
        {
            RequestChangeEyeColorServerRpc();
        }

        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.0f, 0));
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;
        UpdatePlayerInfo();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string newName, bool isPlayerA)
    {
        if (isPlayerA)
            playerNameA.Value = new NetworkString { info = new FixedString32Bytes(newName) };
        else
            playerNameB.Value = new NetworkString { info = new FixedString32Bytes(newName) };

        UpdatePlayerNameClientRpc();
    }

    [ClientRpc]
    private void UpdatePlayerNameClientRpc()
    {
        nameLabel.text = (IsOwnedByServer) ? playerNameA.Value.ToString() : playerNameB.Value.ToString();
    }

    private void UpdatePlayerInfo()
    {
        if (IsOwnedByServer)
        {
            nameLabel.text = playerNameA.Value.ToString();
        }
        else
        {
            nameLabel.text = playerNameB.Value.ToString();
        }
    }

    public override void OnDestroy()
    {
        if (nameLabel != null) { Destroy(nameLabel.gameObject); }
        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestChangeEyeColorServerRpc()
    {
        isEyeRed.Value = !isEyeRed.Value; // สลับสถานะระหว่างแดง/ปกติ
        UpdateEyeColorClientRpc(isEyeRed.Value);
    }

    [ClientRpc]
    private void UpdateEyeColorClientRpc(bool makeRed)
    {
        ChangeEyeColor(makeRed);
    }

    private void ChangeEyeColor(bool makeRed)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.name.ToLower().Contains("eye")) // ตรวจสอบชื่อ Material ว่ามีคำว่า "eye"
                {
                    mat.color = makeRed ? Color.red : Color.white;
                }
            }
        }
    }
}