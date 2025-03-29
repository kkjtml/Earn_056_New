using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerJump : NetworkBehaviour
{
    public float jumpForce = 10f;
    private Rigidbody rb;
    private bool isFloor;
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space) && isFloor)
        {
            Jump();
        }
    }

    void Jump()
    {
        ownerNetworkAnimationScript.SetTrigger("Jump");
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isFloor = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isFloor = false;
        }
    }
}
