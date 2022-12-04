using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 moveDirection;
    float jumpDirection;
    public float moveSpeed = 2;
    public float maxForwardSpeed = 8;
    public float turnaround = 100;
    public float jumpSpeed = 3000f;
    float desiredSpeed;
    float forwardSpeed;

    const float groundAccel = 5;
    const float groundDecel = 25;

    Animator anim;

    Rigidbody rb;

    bool IsMoveInput
    {
        get { return !Mathf.Approximately(moveDirection.sqrMagnitude, 0f); }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OJump(InputAction.CallbackContext context)
    {
        jumpDirection = context.ReadValue<float>();
    }

    void Move(Vector2 direction)
    {
        float turnAmount = moveDirection.x;
        float fDirection = direction.y;
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        desiredSpeed = direction.magnitude * maxForwardSpeed * Mathf.Sign(fDirection);
        float acceleration = IsMoveInput ? groundAccel : groundDecel;

        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * Time.deltaTime);

        anim.SetFloat("ForwardSpeed", forwardSpeed);

        transform.Rotate(0, turnAmount * turnaround * Time.deltaTime, 0);

        //transform.Translate(direction.x * moveSpeed * Time.deltaTime, 0, direction.y * moveSpeed * Time.deltaTime);
    }

    bool ReadyJump = false;
    void Jump(float Direction)
    {
        if(Direction > 0)
        {
            anim.SetBool("ReadyJump", true);
            ReadyJump = true;
        }
        else if(ReadyJump)
        {

            anim.SetBool("Launch", true);

        }
    }

    public void Launch()
    {
        rb.AddForce(0, jumpSpeed, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move(moveDirection);
        Jump(jumpDirection);
    }
}
