using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TulipFlowerScript : NetworkBehaviour
{
    public TulipFlowerSpawnerScript tulipflowerSpawner;
    public GameObject effectPrefab;
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.tag == "player") 
        {
            ulong networkObjectId = GetComponent<NetworkObject>().NetworkObjectId; 
            SpawnEffect(); 
            tulipflowerSpawner.DestroyServerRpc(networkObjectId);
        }
    }
    
    void SpawnEffect()
    {
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}

