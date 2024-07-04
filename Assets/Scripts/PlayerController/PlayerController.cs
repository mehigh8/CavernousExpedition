using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public enum MovementState
    {
        WALKING,
        SPRINTING,
        AIR
    }

    [System.Serializable]
    public enum Food
    {
        APPLE,
        WATER
    }

    [Header("Player Movement Config")]
    public float minWalkSpeed;
    public float maxWalkSpeed;
    public float sprintMultiplier;
    public float movementMultiplier;
    public float groundDrag;
    public float airDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float airDownwardForce;
    public float slopeDownwardForce;
    [Header("Hunger Config")]
    public float hungerDecreaseRate;
    public float appleIncreaseValue;
    public float waterIncreaseValue;
    [Header("Ground Check")]
    public float rayLength;
    public float slopeRayLength;
    public LayerMask groundLayer;
    [Header("References")]
    public Transform orientation;
    public Slider hungerBar;
    public AudioSource audioSource;
    [Space]
    public MovementState movementState;

    private float movementSpeed;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    private bool isGrounded;
    private bool canJump;
    private bool exitingSlope;

    private RaycastHit slopeHit;

    private float hungerLevel = 1f;
    [SerializeField] private float walkSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        canJump = true;
        movementSpeed = maxWalkSpeed;

        audioSource.Pause();
    }

    void Update()
    {
        hungerBar.value = hungerLevel;

        isGrounded = CheckGround();

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (GameManager.GetInstance().pauseManager.isPaused)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canJump && isGrounded && !GameManager.GetInstance().pauseManager.isPaused)
        {
            canJump = false;
            Jump();
            Invoke("ResetJump", jumpCooldown);
        }

        LimitVelocity();

        HandleState();

        ApplyDrag();

        audioSource.volume = PlayerPrefs.GetFloat("sfx");
        audioSource.pitch = 0.5f + hungerLevel / (movementState == MovementState.WALKING ? 2 : 1);
        if (rb.velocity.magnitude > 1f && movementState != MovementState.AIR)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    void FixedUpdate()
    {
        CheckHunger();
        Move();
    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeDirection() * movementSpeed * movementMultiplier, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * slopeDownwardForce, ForceMode.Force);
        }
        else
        {
            if (isGrounded)
            {
                rb.AddForce(moveDirection.normalized * movementSpeed * movementMultiplier, ForceMode.Force);
            }
            else
            {
                rb.AddForce(moveDirection.normalized * movementSpeed * movementMultiplier * airMultiplier, ForceMode.Force);
                // Downward force
                rb.AddForce(Vector3.down * airDownwardForce, ForceMode.Force);
            }
        }

        rb.useGravity = !OnSlope();
    }

    private void ApplyDrag()
    {
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = airDrag;
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(jumpForce * transform.up, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;

        exitingSlope = false;
    }

    private void LimitVelocity()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > movementSpeed)
                rb.velocity = rb.velocity.normalized * movementSpeed;
        }
        else
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (flatVelocity.magnitude > movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }

    private bool CheckGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, rayLength, groundLayer)
            || Physics.Raycast(transform.position + Vector3.forward, Vector3.down, rayLength, groundLayer)
            || Physics.Raycast(transform.position + Vector3.back, Vector3.down, rayLength, groundLayer)
            || Physics.Raycast(transform.position + Vector3.right, Vector3.down, rayLength, groundLayer)
            || Physics.Raycast(transform.position + Vector3.left, Vector3.down, rayLength, groundLayer);
    }

    private void HandleState()
    {
        if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = walkSpeed * sprintMultiplier;
            movementState = MovementState.SPRINTING;
        }
        else if (isGrounded)
        {
            movementSpeed = walkSpeed;
            movementState = MovementState.WALKING;
        }
        else
        {
            movementState = MovementState.AIR;
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeRayLength))
        {
            return Vector3.Angle(Vector3.up, slopeHit.normal) > 0.01f;
        }

        return false;
    }

    private Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void CheckHunger()
    {
        if (GameManager.GetInstance().pauseManager.isPaused)
            return;

        hungerLevel -= hungerDecreaseRate;
        if (hungerLevel < 0)
            hungerLevel = 0;

        walkSpeed = minWalkSpeed + (maxWalkSpeed - minWalkSpeed) * hungerLevel;
    }

    public void Consume(Food food)
    {
        if (food == Food.APPLE)
            hungerLevel += appleIncreaseValue;

        if (food == Food.WATER)
            hungerLevel += waterIncreaseValue;

        if (hungerLevel > 1f)
            hungerLevel = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * rayLength);
        Gizmos.DrawRay(transform.position + Vector3.forward, Vector3.down * rayLength);
        Gizmos.DrawRay(transform.position + Vector3.back, Vector3.down * rayLength);
        Gizmos.DrawRay(transform.position + Vector3.right, Vector3.down * rayLength);
        Gizmos.DrawRay(transform.position + Vector3.left, Vector3.down * rayLength);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * slopeRayLength);
    }
}
