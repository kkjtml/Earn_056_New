using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Threading;
using Unity.Collections;

public class MainPlayerMovement : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;

    public TMP_Text namePrefab;
    private TMP_Text nameLabel;
    private NetworkVariable<int> postX = new NetworkVariable<int>(0,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<NetworkString> playerNameA = new NetworkVariable<NetworkString>(
    new NetworkString { info = "Player" }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<NetworkString> playerNameB = new NetworkVariable<NetworkString>(
        new NetworkString { info = "Player" }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public Renderer eyerender;
    private MaterialPropertyBlock materialPropertyBlock;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    public Texture defaultEyeTexture; //ตาเริ่มต้น
    public Texture changeEyeTexture; //ตาที่เปลี่ยน

    public NetworkVariable<int> eyeTextureStatus = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server); 

    private LoginManagerScipt loginManager;

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

    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(namePrefab, Vector3.zero, Quaternion.identity);
        nameLabel.transform.SetParent(canvas.transform);

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

        base.OnNetworkSpawn();
        UpdateEyeTexture(eyeTextureStatus.Value);

        if (IsOwner)
        {
            loginManager = GameObject.FindObjectOfType<LoginManagerScipt>();
            if (loginManager != null)
            {
                string name = loginManager.userNameInputField.text;

                if (IsOwnedByServer)
                    SetPlayerNameServerRpc(name, true);
                else
                    SetPlayerNameServerRpc(name, false);

                UpdateEyeTexture(eyeTextureStatus.Value);
            }
        }
    }

    private void OnEnable()
    {
        if (nameLabel != null)
            nameLabel.enabled = true;

        eyeTextureStatus.OnValueChanged += (previous, current) => UpdateEyeTexture(current);
    }

    private void OnDisable()
    {
        if (nameLabel != null)
            nameLabel.enabled = false;

        eyeTextureStatus.OnValueChanged += (previous, current) => UpdateEyeTexture(current);
    }

    private void Update()
    {
        Vector3 nameLabelPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.0f, 0)); 
        nameLabel.text = gameObject.name;
        nameLabel.transform.position = nameLabelPos;

        if (IsOwner)
        {
            postX.Value = (int)System.Math.Ceiling(transform.position.x);

            if (Input.GetKeyDown(KeyCode.F))
            {
                RequestToggleEyeTextureServerRpc();
            }
        }

        UpdatePlayerInfo();
    }

    private void FixedUpdate()
    {
        // if (IsOwner)
        // {
        //     float translation = Input.GetAxis("Vertical") * speed;
        //     translation *= Time.deltaTime;
        //     rb.MovePosition(rb.position + this.transform.forward * translation);

        //     float rotation = Input.GetAxis("Horizontal");
        //     if (rotation != 0)
        //     {
        //         rotation *= rotationSpeed;
        //         Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
        //         rb.MoveRotation(rb.rotation * turn);
        //     }
        //     else
        //     {
        //         rb.angularVelocity = Vector3.zero;
        //     }
        // }
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

    [ServerRpc(RequireOwnership = false)]
    private void RequestToggleEyeTextureServerRpc(ServerRpcParams rpcParams = default) //เปลี่ยน texture ตา
    {
        eyeTextureStatus.Value = (eyeTextureStatus.Value == 0) ? 1 : 0;
    }

    private void UpdateEyeTexture(int status) 
    {
        if (eyerender != null)
        {
            if (materialPropertyBlock== null)
                materialPropertyBlock = new MaterialPropertyBlock();

            Texture newTexture = (status == 1) ? changeEyeTexture : defaultEyeTexture; //ถ้า 1 จะเปลี่ยน texture ตา 
            materialPropertyBlock.SetTexture(MainTex, newTexture);
            eyerender.SetPropertyBlock(materialPropertyBlock);
        }
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
}