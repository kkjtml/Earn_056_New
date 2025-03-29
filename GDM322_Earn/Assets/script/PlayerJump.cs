using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerJump : NetworkBehaviour
{
    public float jumpForce = 7f; // กำหนดแรงกระโดด
    private Rigidbody rb;
    private bool isGrounded;
    private OwnerNetworkAnimationScript ownerNetworkAnimationScript;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ownerNetworkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void Jump()
    {
        ownerNetworkAnimationScript.SetTrigger("Jump"); //ถ้ากด spacebar จะเล่นแอนิเมชันกระโดด
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        // ตรวจสอบว่า Player ยืนบนพื้น
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // เมื่อไม่ได้อยู่บนพื้น ให้ isGrounded = false
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
