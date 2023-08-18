using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Player.PlayerMovement
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpForce;
        [SerializeField] private Animator animator;
        [SerializeField] private LayerMask groundCheckLayerMask;
        [SerializeField] private Vector3 boxSize;
        [SerializeField] private float maxDistance;

        private Rigidbody rb;
        private float xAxis;
        private float zAxis;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        
        void Update()
        {

        }

        void FixedUpdate()
        {
            MovePlayer();
            JumpPlayer();
        }

        private void JumpPlayer()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("true");
                rb.AddForce(Vector3.up * jumpForce);
            }
        }

        private void MovePlayer()
        {
            xAxis = Input.GetAxis("Horizontal");
            zAxis = Input.GetAxis("Vertical");

            if (xAxis > 0 || xAxis < 0 || zAxis > 0 || zAxis < 0)
            {
                animator.SetTrigger("Walking");
            }
            else
            {
                animator.SetTrigger("Stopping");
            }

            rb.velocity = new Vector3(xAxis * moveSpeed, rb.velocity.y, zAxis * moveSpeed);
        }

        private bool GroundCheck()
        {
            return (Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, groundCheckLayerMask));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
        }
    }
}
