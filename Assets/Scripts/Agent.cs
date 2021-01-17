using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private CharacterController controller;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Move agent right above terrain
        var collider = GetComponent<CapsuleCollider>();
        Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hitinfo);
        transform.position = hitinfo.point + new Vector3(0, collider.height / 2.0f, 0);
    }

    public void Move(Vector3 movement)
    {
        controller.Move(movement * Time.deltaTime);
        if(!controller.isGrounded)
        {
            controller.Move(Physics.gravity * Time.deltaTime);
        }
    }
}
