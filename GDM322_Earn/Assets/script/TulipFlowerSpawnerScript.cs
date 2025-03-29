using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TulipFlowerSpawnerScript : NetworkBehaviour
{
    public GameObject tulipflowerPrefab;
    private List<GameObject> spawnedtulipflower = new List<GameObject>();

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.X))
        {
            SpawnTulipFlowerServerRpc(OwnerClientId);
        }
    }

    [ServerRpc]
    void SpawnTulipFlowerServerRpc(ulong clientId)
    {
        Vector3 spawnPos = transform.position + (transform.forward * -1.5f) + (transform.up * 0.8f);
        Quaternion spawnRot = transform.rotation;
        GameObject tulipflower = Instantiate(tulipflowerPrefab, spawnPos, spawnRot);
        spawnedtulipflower.Add(tulipflower);
        tulipflower.GetComponent<TulipFlowerScript>().tulipflowerSpawner = this;
        tulipflower.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectId)
    {
        GameObject toDestory = findtulipflowerFromNetworkId(networkObjectId);
        if (toDestory == null) return;

        toDestory.GetComponent<NetworkObject>().Despawn();
        spawnedtulipflower.Remove(toDestory);
        Destroy(toDestory);
    }

    private GameObject findtulipflowerFromNetworkId(ulong networkObjectId)
    {
        foreach (GameObject tulipflower in spawnedtulipflower)
        {
            ulong tulipflowerId = tulipflower.GetComponent<NetworkObject>().NetworkObjectId;
            Debug.Log("coinId  " + tulipflowerId);
            if (tulipflowerId == networkObjectId)
            {
                return tulipflower;
            }
        }
        return null;
    }
}

