using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float Speed = 5;
    void Update()

    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.forward;

        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }
        transform.position += movement.normalized * Speed * Time.deltaTime;
    }
}