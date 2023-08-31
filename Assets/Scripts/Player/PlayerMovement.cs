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
        [SerializeField] private float rotateSpeed;
        [SerializeField] private float grounderOffset;
        [SerializeField] private float grounderRadius;

        private Rigidbody rb;
        private float xAxis;
        private float zAxis;
        private readonly Collider[] _ground = new Collider[1];

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        
        void Update()
        {
            RotatePlayer();
        }

        void FixedUpdate()
        {
            MovePlayer();
            JumpPlayer();
        }

        private void JumpPlayer()
        {
            if (Input.GetKeyDown(KeyCode.Space) & GroundCheck())
            {
                animator.SetTrigger("Jumping");
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

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0;
            right.y = 0;
            forward = forward.normalized;
            right = right.normalized;
            Vector3 forwardRelative = forward * zAxis;
            Vector3 rightRelative = right * xAxis;
            Vector3 cameraRelative = (forwardRelative + rightRelative) * moveSpeed * Time.fixedDeltaTime;
            transform.Translate(cameraRelative, Space.World);
        }

        private bool GroundCheck()
        {
            //return (Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, groundCheckLayerMask));
            return Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, grounderOffset), grounderRadius, _ground, groundCheckLayerMask) > 0;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
        }

        private void RotatePlayer()
        {
            transform.rotation = new Quaternion(transform.rotation.x, Camera.main.transform.rotation.y, transform.rotation.z, transform.rotation.w);
        }
    }
}
