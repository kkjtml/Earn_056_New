using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField userNameInput;
    public TMP_InputField coderoomInput;
    public TMP_Dropdown color;
    public GameObject loginPanel;
    public GameObject leaveButton;
    public GameObject scorePanel;
    public List<GameObject> spawn = new List<GameObject>();
    public List<uint> AlternatePlayerPrefebs;

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HanddleClientDisconnect;
        SetUIVisable(false);
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HanddleClientDisconnect;
    }

    private void SetUIVisable(bool isUserLogin)
    {
        if (isUserLogin)
        {
            loginPanel.SetActive(false);
            leaveButton.SetActive(true);
            scorePanel.SetActive(true);
        }
        else
        {
            loginPanel.SetActive(true);
            leaveButton.SetActive(true);
            scorePanel.SetActive(false);
        }
    }

    public void Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        Debug.Log("Start Host");
    }

    public void Client()
    {
        string userName = userNameInput.GetComponent<TMP_InputField>().text;
        string Coderoom = coderoomInput.GetComponent<TMP_InputField>().text;
        int Character = SelectTexture();

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(userName + "/" + Coderoom + "/" + Character);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Start Client");
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SetUIVisable(false);
    }

    public void HandleServerStarted()
    {
        Debug.Log("HandleServerStarted");
    }

    public void HandleClientConnected(ulong clientId)
    {
        Debug.Log("ClientID = " + clientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetUIVisable(true);
        }
    }

    public void HanddleClientDisconnect(ulong clientID)
    {
        Debug.Log("HandleClientDisconnect clientID = " + clientID);
        if (NetworkManager.Singleton.IsHost) { }
        else if (NetworkManager.Singleton.IsClient) { Leave(); }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var clientId = request.ClientNetworkId;
        var connectionData = request.Payload;
        int byteLength = connectionData.Length;
        Debug.Log("byte length = " + byteLength);
        bool isApproved = false;

        if (byteLength > 0)
        {
            string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength);
            string[] ClientDataAndCode = clientData.Split("/");
            int Color = int.Parse(ClientDataAndCode[2]);
            string hostData = userNameInput.GetComponent<TMP_InputField>().text;
            string CoderoomFromHost = coderoomInput.GetComponent<TMP_InputField>().text;
            isApproved = approveConnection(ClientDataAndCode, hostData, CoderoomFromHost);
            response.PlayerPrefabHash = AlternatePlayerPrefebs[Color];
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                response.PlayerPrefabHash = AlternatePlayerPrefebs[SelectTexture()];
            }
        }

        response.Approved = isApproved;
        response.CreatePlayerObject = true;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        SpawnLocation(clientId, response);
        response.Reason = "Some reason for not approving the client";
        response.Pending = false;
    }

    public bool approveConnection(string[] ClientDataAndCode, string hostData, string CoderoomHost)
    {
        string clientData = ClientDataAndCode[0];
        string CoderoomClient = ClientDataAndCode[1];

        bool approveName = System.String.Equals(clientData.Trim(), hostData.Trim()) ? false : true;
        bool approveCode = System.String.Equals(CoderoomClient.Trim(), CoderoomHost.Trim()) ? true : false;

        Debug.Log(approveName);
        Debug.Log(approveCode);

        return approveCode && approveName;
    }

    private void SpawnLocation(ulong clientID, NetworkManager.ConnectionApprovalResponse response)
    {
        GameObject spawnPoint = Spawn();
        response.Position = spawnPoint.transform.position;
        response.Rotation = Quaternion.Euler(0f, 225f, 0f);
    }

    private GameObject Spawn()
    {
        int random = Random.Range(0, spawn.Count);
        return spawn[random];
    }

    public int SelectTexture()
    {
        return color.value;
    }
}