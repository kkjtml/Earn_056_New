using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority; 
public class PlayerSpawnerScript : NetworkBehaviour
{
    //MainPlayerScript mainPlayer;
    public Behaviour[] scripts;
    private Renderer[] renderers;
    private ClientNetworkTransform clientNetworkTransform;

    void Start()
    {
        //mainPlayer = gameObject.GetComponent<MainPlayerScript>();
        renderers = GetComponentsInChildren<Renderer>();
        clientNetworkTransform = GetComponent<ClientNetworkTransform>(); 
    }
    void SetPlayerState(bool state)
    {
        foreach (var script in scripts) { script.enabled = state; }
        foreach (var renderer in renderers) { renderer.enabled = state; }
    }

    private Vector3 GetRandomPos()
    {
        Vector3 randPos = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        return randPos;
    }
    public void Respawn()
    {
        RespawnServerRpc();
    }
    [ServerRpc]
    void RespawnServerRpc()
    {
        Vector3 pos = GetRandomPos();
        RespawnClientRpc(pos);
    }
    [ClientRpc]
    void RespawnClientRpc(Vector3 spawnPos)
    {
        StartCoroutine(RespawnCoroutine(spawnPos));
    }
    IEnumerator RespawnCoroutine(Vector3 spawnPos)
    {
        SetPlayerState(false);

        if (clientNetworkTransform != null)
        {
            clientNetworkTransform.Interpolate = false; //ปิด Interpolation
        }

        transform.position = spawnPos;
        yield return new WaitForSeconds(2f); //รอ 2 วินาที

        if (clientNetworkTransform != null)
        {
            clientNetworkTransform.Interpolate = true; //เปิด Interpolation
        }

        SetPlayerState(true);
    }
}
