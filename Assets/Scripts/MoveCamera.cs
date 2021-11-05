using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public float moveSpeed = 0.1f;
    public float lookSpeed = 3;
    private Vector2 rotation = Vector2.zero;
    private Dictionary<KeyCode, Vector3> moves = new Dictionary<KeyCode, Vector3>()
    {
        {KeyCode.A, -Vector3.right},
        {KeyCode.D, Vector3.right},

        {KeyCode.W, Vector3.forward},
        {KeyCode.S, -Vector3.forward},

        {KeyCode.E, Vector3.up},
        {KeyCode.Q, -Vector3.up},
    };
    void move(Vector3 scale)
    {
        transform.position += transform.right * scale.x + transform.forward * scale.z + transform.up * scale.y;
    }
    public void Look() // Look rotation (UP down is Camera) (Left right is Transform rotation)
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, -89f, 89f);
        transform.eulerAngles = new Vector2(rotation.x, rotation.y) * lookSpeed;
    }
    void Update()
    {
        foreach (var bind in moves)
        {
            if (Input.GetKey(bind.Key))
            {
                move(bind.Value * moveSpeed * Time.deltaTime);
            }
        }
        if (Input.GetMouseButton(1))
        {
            Look();
        }
    }
}
