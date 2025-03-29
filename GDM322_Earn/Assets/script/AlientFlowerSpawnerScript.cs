using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AlientFlowerSpawnerScript : NetworkBehaviour
{
    public GameObject alientflowerPrefab;
    private List<GameObject> spawnedalientflower = new List<GameObject>();
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    void Start()
    {
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ownerNetworkAnimationScript.SetTrigger("Puttingdown");
            SpawnalientflowerServerRpc(OwnerClientId);
        }
    }

    [ServerRpc]
    void SpawnalientflowerServerRpc(ulong clientId)
    {
        Vector3 spawnPos = transform.position + (transform.forward * 1.8f) + (transform.up * 0.8f);
        Quaternion spawnRot = transform.rotation;
        GameObject alientflower = Instantiate(alientflowerPrefab, spawnPos, spawnRot);
        spawnedalientflower.Add(alientflower);
        alientflower.GetComponent<AlientFlowerScript>(). alientflowerSpawner = this;
        alientflower.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectId)
    {
        GameObject toDestory = findalientflowerFromNetworkId(networkObjectId);
        if (toDestory == null) return;

        toDestory.GetComponent<NetworkObject>().Despawn();
        spawnedalientflower.Remove(toDestory);
        Destroy(toDestory);
    }

    private GameObject findalientflowerFromNetworkId(ulong networkObjectId)
    {
        foreach (GameObject alientflower in spawnedalientflower)
        {
            ulong alientflowerId = alientflower.GetComponent<NetworkObject>().NetworkObjectId;
            if (alientflowerId == networkObjectId)
            {
                return alientflower;
            }
        }
        return null;
    }
}
