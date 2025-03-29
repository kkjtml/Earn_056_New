using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;
using TMPro;

public class LoginManagerScipt : MonoBehaviour
{
    public TMP_InputField userNameInputField;
    public TMP_InputField CoderoomInputField; 
    public TMP_Dropdown textureSelect;
    public GameObject loginPanel;
    public GameObject leaveButton;
    public GameObject scorePanel;
    public List<GameObject> spawnPoint = new List<GameObject>(); //เพิ่ม List Spawnpoint
    public List<uint> AlternatePlayerPrefebs; //เพิ่ม PlayerPrefabHash

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
        string userName = userNameInputField.GetComponent<TMP_InputField>().text;
        string Coderoom = CoderoomInputField.GetComponent<TMP_InputField>().text;
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
            string clientData = System.Text.Encoding.ASCII.GetString(connectionData, 0, byteLength); //ชื่อผู้เล่น, รหัสห้อง, การเลือก texture ถูกแปลงจากไบต์เป็นข้อความ ASCII
            string[] ClientDataAndCode = clientData.Split("/"); //แยกข้อมูลที่ส่งโดยใช้ตัวแบ่ง /
            int TextureSelect = int.Parse(ClientDataAndCode[2]); //texture
            string hostData = userNameInputField.GetComponent<TMP_InputField>().text; 
            string CoderoomHost = CoderoomInputField.GetComponent<TMP_InputField>().text; 
            isApproved = approveConnection(ClientDataAndCode, hostData, CoderoomHost);
            response.PlayerPrefabHash = AlternatePlayerPrefebs[TextureSelect]; //ตั้งค่า PlayerPrefabHash ตาม texture ที่เลือก
        }
        else
        {
            if (NetworkManager.Singleton.IsHost)
            {
                response.PlayerPrefabHash = AlternatePlayerPrefebs[SelectTexture()];
            }
        }

        response.Approved = isApproved;
        response.CreatePlayerObject = true; //สร้างตัวละครที่ผู้เล่นเลือก
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        SpawnLocation(clientId, response);
        response.Reason = "Some reason for not approving the client";
        response.Pending = false;
    }

    public bool approveConnection(string[] ClientDataAndCode, string hostData, string CoderoomHost)
    {
        bool isApproved = false;
        string clientData = ClientDataAndCode[0]; //ชื่อผู้เล่น
        string CoderoomClient = ClientDataAndCode[1]; //รหัสห้อง
        string Texture = ClientDataAndCode[2];

        Debug.Log("HostName = " + hostData);
        Debug.Log("ClientName = " + clientData);
        Debug.Log("Host Coderoom " + CoderoomHost);
        Debug.Log("Client Coderoom " + CoderoomClient);

        bool approveName = System.String.Equals(clientData.Trim(), hostData.Trim()) ? false : true; 
        bool approveCode = System.String.Equals(CoderoomClient.Trim(), CoderoomHost.Trim()) ? true : false; 

        Debug.Log(approveName);
        Debug.Log(approveCode);

        if (approveCode == true && approveName == true)
        {
            isApproved = true; 
        }
        else if (approveCode == true && approveName == false)
        {
            isApproved = false; 
        }
        else
        {
            isApproved = false; 
        }

        return isApproved;
    }

    private void SpawnLocation(ulong clientID, NetworkManager.ConnectionApprovalResponse response)
    {
        GameObject spawnPoint = Spawn();
        response.Position = spawnPoint.transform.position;
        response.Rotation = Quaternion.Euler(0f, 225f, 0f);
    }

    private GameObject Spawn() 
    {
        int random = Random.Range(0, spawnPoint.Count); //เลือกสุ่มเกิดในตำแหน่งที่วางไว้
        return spawnPoint[random];
    }

    public int SelectTexture()
    {
        return textureSelect.value; //เลือก texture ตามที่ผู้เล่นต้องการ
    }
}