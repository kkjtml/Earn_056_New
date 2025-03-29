using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class HPPlayerScript : NetworkBehaviour
{
    TMP_Text p1Text;
    TMP_Text p2Text;
    MainPlayerMovement mainPlayer;
    public NetworkVariable<int> hpP1 = new NetworkVariable<int>(5,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> hpP2 = new NetworkVariable<int>(5,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    void Start()
    {
        p1Text = GameObject.Find("P1HPText (TMP)").GetComponent<TMP_Text>();
        p2Text = GameObject.Find("P2HPText (TMP)").GetComponent<TMP_Text>();
        mainPlayer = GetComponent<MainPlayerMovement>();
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
    }

    void Update()
    {
        UpdatePlayerNameAndScore();
    }

    private void UpdatePlayerNameAndScore()
    {
        if (IsOwnedByServer)
        {
            p1Text.text = $"{mainPlayer.playerNameA.Value} : {hpP1.Value}";
        }
        else
        {
            p2Text.text = $"{mainPlayer.playerNameB.Value} : {hpP2.Value}";
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsLocalPlayer) return;

        if (collision.gameObject.tag == "DeathZone")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value--;
                if (hpP1.Value <= 0)
                {
                    IsDeath();
                }
            }
            else
            {
                hpP2.Value--;
                if (hpP2.Value <= 0)
                {
                    IsDeath();
                }
            }
            gameObject.GetComponent<PlayerSpawnerScript>().Respawn();
        }
        else if (collision.gameObject.tag == "Alient")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value--;
                if (hpP1.Value <= 0) 
                {
                    IsDeath();
                }
            }
            else
            {
                hpP2.Value--;
                if (hpP2.Value <= 0)
                {
                    IsDeath();
                }
            }
        }
        else if (collision.gameObject.tag == "Tulip")
        {
            if (IsOwnedByServer)
            {
                hpP1.Value++;
            }
            else
            {
                hpP2.Value++;
            }
        }
    }
    private void IsDeath()
    {
        ownerNetworkAnimationScript.SetTrigger("Death"); 
    }

}
