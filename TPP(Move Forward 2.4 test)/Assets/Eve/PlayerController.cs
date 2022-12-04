using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Vector2 moveDirection;
    Vector2 lookDirection;
    float jumpDirection;

    public float moveSpeed = 2f;
    public float turnSpeed = 100f;
    public float maxForwardSpeed = 8f;
    public float jumpSpeed = 30000f;

    float desiredSpeed;
    float forwardSpeed;

    const float groundAccel = 5f;
    const float groundDecel = 25f;

    Animator anim;
    Rigidbody rb;

    bool onGround = true;

    public Transform hand;
    public Transform hip;
    public Transform weapon;

    public void PickupGun()
    {
        weapon.SetParent(hand);
        weapon.localPosition = new Vector3(-0.001f, 0.0534f, 0.0346f);
        weapon.localRotation = Quaternion.Euler(-69.457f, -171.64f, -85.833f);
        weapon.localScale = new Vector3(1, 1, 1);
    }
    public void PutdownGun()
    {
        weapon.SetParent(hip);
        weapon.localPosition = new Vector3(-0.09215199f, -0.05758657f, -0.06340759f);
        weapon.localRotation = Quaternion.Euler(-84.388f, -196.219f, -13.657f);
        weapon.localScale = new Vector3(1, 1, 1);
    }

    public bool IsMoveInput { get { return !Mathf.Approximately(moveDirection.sqrMagnitude, 0f); } }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    
    public void OnJump(InputAction.CallbackContext context)
    {
        jumpDirection = context.ReadValue<float>();
    }

    
    public void OnArmed(InputAction.CallbackContext context)
    {
        anim.SetBool("Armed", !anim.GetBool("Armed"));
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if((int)context.ReadValue<float>() == 1 && anim.GetBool("Armed"))
        anim.SetTrigger("Fire");
    }

    
    public void OnLook(InputAction.CallbackContext context)
    {
        lookDirection = context.ReadValue<Vector2>();
    }

    public void Move(Vector2 direction)
    {
        float turnAmount = direction.x;
        float fDirection = direction.y;
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();
        desiredSpeed = direction.sqrMagnitude * maxForwardSpeed * Mathf.Sign(fDirection);
        float acceleration = IsMoveInput ? groundAccel : groundDecel;
        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * Time.deltaTime);
        anim.SetFloat("ForwardSpeed", forwardSpeed);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        
        //transform.Translate(direction.x * moveSpeed * Time.deltaTime, 0, direction.y * moveSpeed * Time.deltaTime);
    }

    bool readyJump = false;
    float jumpEffort = 0f;
    public void Jump(float direction)
    {
        if(direction > 0)
        {
            anim.SetBool("ReadyJump", true);
            readyJump = true;
            jumpEffort += Time.deltaTime;
        }else if(readyJump)
        {
            anim.SetBool("Launch", true);
            anim.SetBool("ReadyJump", false);
            readyJump = false;
        }
        Debug.Log("Jump Effort :" + jumpEffort);
    }

    public void Launch()
    {
        rb.AddForce(0, jumpSpeed * Mathf.Clamp(jumpEffort,1,3), 0);
        anim.SetBool("Launch", false);
    }

    public void Land()
    {
        anim.SetBool("Land", false);
        anim.SetBool("Launch", false);
        anim.applyRootMotion = true;
        jumpEffort = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();
    }

    float groundRayDist = 2f;

    public Transform spine;

    Vector2 lastLookDirection;

    float xSensitivity = 0.5f;
    float ySensitivity = 0.5f;

    public void LateUpdate()
    {
        lastLookDirection += new Vector2(-lookDirection.y * ySensitivity, lookDirection.x * xSensitivity);
        lastLookDirection.x = Mathf.Clamp(lastLookDirection.x, -30, 30);
        lastLookDirection.y = Mathf.Clamp(lastLookDirection.y, -30, 60);
        spine.localEulerAngles = lastLookDirection;
    }

    // Update is called once per frame
    void Update()
    {
        Move(moveDirection);
        Jump(jumpDirection);

        RaycastHit laserHit;


        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * groundRayDist * 0.5f,-Vector3.up);
        if (Physics.Raycast(ray, out hit, groundRayDist))
        {
            if (!onGround)
            {
                anim.SetBool("Land", true);
                anim.SetBool("Falling", false);
                anim.SetFloat("LandingVelocity", rb.velocity.magnitude);
                onGround = true;
            }
        }else
        {
            anim.SetBool("Falling", true);
            anim.SetBool("Land", false);
            anim.applyRootMotion = false;
            onGround = false;
        }
        Debug.DrawRay(transform.position + Vector3.up * groundRayDist * 0.5f, -Vector3.up * groundRayDist, Color.red);

    }
}
