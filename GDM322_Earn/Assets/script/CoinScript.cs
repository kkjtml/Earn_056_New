using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoinScript : NetworkBehaviour
{
    public CoinSpawnerScript coinSpawner;
    public GameObject effectPrefab;
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.tag == "player") //เมื่อเหรียญชน player
        {
            ulong networkObjectId = GetComponent<NetworkObject>().NetworkObjectId; //ดึง NetworkObjectId ของเหรียญ
            SpawnEffect(); //เกิดเอฟเฟค
            coinSpawner.DestroyServerRpc(networkObjectId); //ทำลายเหรียญ
        }
    }
    
    void SpawnEffect()
    {
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();
    }
}

