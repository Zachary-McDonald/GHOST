using UnityEngine;

public class playermove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float Speed = 10;
    public float RotationSpeed = 10;
    public CharacterController controller;
    private Vector3 velocity;
    public float gravity = -9.81f;

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("No Animator found on this object!");
        }
        else
        {
            Debug.Log("Animator found: " + animator.runtimeAnimatorController);
        }
    }
    void Update()

    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.forward;

        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.W))

        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.right;
        }

        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }
        controller.Move(movement * Speed * Time.deltaTime);


        //rotate character

        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                RotationSpeed * Time.deltaTime
            );
        }


        //update idle and run
        if (animator != null)
        {
            animator.SetFloat("Speed", movement.magnitude);
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    

}
