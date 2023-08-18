using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Hey developer!
/// If you have any questions, come chat with me on my Discord: https://discord.gg/GqeHHnhHpz
/// If you enjoy the controller, make sure you give the video a thumbs up: https://youtu.be/rJECT58CQHs
/// Have fun!
///
/// Love,
/// Tarodev
/// </summary>
public class PlayerController3d : MonoBehaviour {
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _anim;
    private FrameInputs _inputs;

    private void Update() {
        GatherInputs();

        HandleGrounding();

        HandleWalking();

        HandleJumping();
    }

    #region Inputs
    [SerializeField] private GameObject playerCamViewport;
    private void GatherInputs() {
        _inputs.RawX = (int) Input.GetAxisRaw("Horizontal");
        _inputs.RawZ = (int) Input.GetAxisRaw("Vertical");
        _inputs.X = Input.GetAxis("Horizontal");
        _inputs.Z = Input.GetAxis("Vertical");

        _dir = new Vector3(_inputs.X, 0, _inputs.Z);

        // Set look direction only if dir is not zero, to avoid snapping back to original
        if (_dir != Vector3.zero) { _anim.transform.forward = _dir; playerCamViewport.transform.forward = _dir; }

        //_anim.SetInteger("RawZ", _inputs.RawZ);
    }

    #endregion

    #region Detection

    [Header("Detection")] [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;
    [SerializeField] private float _wallCheckOffset = 0.5f, _wallCheckRadius = 0.38f;
    private bool _isAgainstWall, _pushingWall;
    public bool IsGrounded;
    public static event Action OnTouchedGround;

    private readonly Collider[] _ground = new Collider[1];
    private readonly Collider[] _wall = new Collider[1];

    private void HandleGrounding() {
        // Grounder
        var grounded = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, _grounderOffset), _grounderRadius, _ground, _groundMask) > 0;

        if (!IsGrounded && grounded) {
            IsGrounded = true;
            _hasJumped = false;
            _currentMovementLerpSpeed = 100;
            _anim.SetBool("Grounded", true);
            OnTouchedGround?.Invoke();
        }
        else if (IsGrounded && !grounded) {
            IsGrounded = false;
            _anim.SetBool("Grounded", false);
            transform.SetParent(null);
        }

        // Wall detection
        _isAgainstWall = Physics.OverlapSphereNonAlloc(WallDetectPosition, _wallCheckRadius, _wall, _groundMask) > 0;
        _pushingWall = _isAgainstWall && _inputs.X < 0;
    }

    private Vector3 WallDetectPosition => _anim.transform.position + Vector3.up + _anim.transform.forward * _wallCheckOffset;


    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        // Grounder
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, _grounderOffset), _grounderRadius);

        // Wall
        Gizmos.DrawWireSphere(WallDetectPosition, _wallCheckRadius);
    }

    #endregion

    #region Walking

    [Header("Walking")] [SerializeField] private float _walkSpeed = 8;
    [SerializeField] private float _acceleration = 2;
    [SerializeField] private float _maxWalkingPenalty = 0.5f;
    [SerializeField] private float _currentMovementLerpSpeed = 100;
    private float _currentWalkingPenalty;

    private Vector3 _dir;

    /// <summary>
    /// I'm sure this section could use a big refactor
    /// </summary>
    private void HandleWalking() {
        _currentMovementLerpSpeed = Mathf.MoveTowards(_currentMovementLerpSpeed, 100, _wallJumpMovementLerp * Time.deltaTime);

        var normalizedDir = _dir.normalized;

        // Slowly increase max speed
        if (_dir != Vector3.zero) _currentWalkingPenalty += _acceleration * Time.deltaTime;
        else _currentWalkingPenalty -= _acceleration * Time.deltaTime;
        _currentWalkingPenalty = Mathf.Clamp(_currentWalkingPenalty, _maxWalkingPenalty, 1);

        // Set current y vel and add walking penalty
        var targetVel = new Vector3(normalizedDir.x, _rb.velocity.y, normalizedDir.z) * _currentWalkingPenalty * _walkSpeed;

        // Set vel
        var idealVel = new Vector3(targetVel.x, _rb.velocity.y, targetVel.z);

        _rb.velocity = Vector3.MoveTowards(_rb.velocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);

        //_anim.SetBool("Walking", _dir != Vector3.zero && IsGrounded);
    }

    #endregion

    #region Jumping

    [Header("Jumping")] [SerializeField] private float _jumpForce = 15;
    [SerializeField] private float _fallMultiplier = 7;
    [SerializeField] private float _jumpVelocityFalloff = 8;
    [SerializeField] private float _wallJumpMovementLerp = 20;
    [SerializeField] private float _coyoteTime = 0.3f;
    [SerializeField] private bool _enableDoubleJump = true;
    private float _timeLeftGrounded = -10;
    private bool _hasJumped;
    private bool _hasDoubleJumped;

    private void HandleJumping() {
        if (Input.GetButtonDown("Jump")) {
            if (!IsGrounded) {
                //_timeLastWallJumped = Time.time;
                _currentMovementLerpSpeed = _wallJumpMovementLerp;

                //if (GetWallHit(out var wallHit)) ExecuteJump(new Vector3(wallHit.normal.x * _jumpForce, _jumpForce, wallHit.normal.z * _jumpForce)); // Wall jump
            }
            else if (IsGrounded || Time.time < _timeLeftGrounded + _coyoteTime || _enableDoubleJump && !_hasDoubleJumped) {
                if (!_hasJumped || _hasJumped && !_hasDoubleJumped) ExecuteJump(new Vector2(_rb.velocity.x, _jumpForce), _hasJumped); // Ground jump
            }
        }

        void ExecuteJump(Vector3 dir, bool doubleJump = false) {
            _rb.velocity = dir;
            //_anim.SetTrigger(doubleJump ? "DoubleJump" : "Jump");
            _hasDoubleJumped = doubleJump;
            _hasJumped = true;
        }

        // Fall faster and allow small jumps. _jumpVelocityFalloff is the point at which we start adding extra gravity. Using 0 causes floating
        if (_rb.velocity.y < _jumpVelocityFalloff || _rb.velocity.y > 0 && !Input.GetButton("Fire2"))
            _rb.velocity += _fallMultiplier * Physics.gravity.y * Vector3.up * Time.deltaTime;
    }

    #endregion

    #region Impacts


    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Death")) ExecuteDeath();
        //if (collision.relativeVelocity.magnitude > _minImpactForce && IsGrounded) _impactParticles.Play();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Death")) ExecuteDeath();
    }

    private void ExecuteDeath() {
        //Instantiate(_deathExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    #endregion

    private struct FrameInputs {
        public float X, Z;
        public int RawX, RawZ;
    }
}