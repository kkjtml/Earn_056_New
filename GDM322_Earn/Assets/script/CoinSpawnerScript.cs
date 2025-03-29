using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoinSpawnerScript : NetworkBehaviour
{
    public GameObject coinPrefab;
    private List<GameObject> spawnedCoin = new List<GameObject>(); 

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.X))
        {
            SpawnHeartServerRpc(OwnerClientId);
        }
    }

    [ServerRpc]
    void SpawnHeartServerRpc(ulong clientId) //สร้างเหรียญ
    {
        Vector3 spawnPos = transform.position + (transform.forward * -1.5f) + (transform.up * 0.8f);
        Quaternion spawnRot = transform.rotation;
        GameObject coin = Instantiate(coinPrefab, spawnPos, spawnRot);
        spawnedCoin.Add(coin);
        coin.GetComponent<CoinScript>().coinSpawner = this;
        coin.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectId) //ทำลายเหรียญ
    {
        GameObject toDestory = findCoinFromNetworkId(networkObjectId);
        if (toDestory == null) return;

        toDestory.GetComponent<NetworkObject>().Despawn();
        spawnedCoin.Remove(toDestory);
        Destroy(toDestory);
    }

    private GameObject findCoinFromNetworkId(ulong networkObjectId) //หา id เหรียญที่จะทำลาย ถ้าตรงกับที่ส่งมาก็จะทำลาย
    {
        foreach (GameObject coin in spawnedCoin)
        {
            ulong coinId = coin.GetComponent<NetworkObject>().NetworkObjectId;
            Debug.Log("coinId  " + coinId);
            if (coinId == networkObjectId)
            {
                return coin;
            }
        }
        return null;
    }
}

