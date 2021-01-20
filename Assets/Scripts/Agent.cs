using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
    private CharacterController controller;
    private Camera camera;
    private RenderTexture visionTarget;
    private Texture2D vision;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        camera = GetComponent<Camera>();
        visionTarget = new RenderTexture(new RenderTextureDescriptor(84, 84));
        vision = new Texture2D(visionTarget.width, visionTarget.height);
        camera.targetTexture = visionTarget;

        // Move agent right above terrain
        var collider = GetComponent<CapsuleCollider>();
        Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hitinfo);
        transform.position = hitinfo.point + new Vector3(0, collider.height / 2.0f, 0);
    }

    public string GetObservation()
    {
        //// Get nearby apples
        //var colliders = Physics.OverlapSphere(transform.position, 5f);
        //foreach(var collider in colliders)
        //{
        //    if (collider.tag.Equals("Apple"))
        //    {
        //        Debug.Log("Found apple");
        //    }
        //}

        // Read pixels from camera's render target texture
        RenderTexture.active = visionTarget;
        vision.ReadPixels(new Rect(0, 0, visionTarget.width, visionTarget.height), 0, 0);
        vision.Apply();
        var pixels = vision.GetPixels();

        // Save data as string
        var stringBuilder = new StringBuilder();
        foreach(var pixel in pixels)
        {
            stringBuilder.Append(pixel);
        }
        return stringBuilder.ToString();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag.Equals("Apple"))
        {
            Destroy(collider.gameObject);
        }
    }

    public void PerformAction(string action)
    {
        if(action.Equals(Constants.AGENT_ACTION_MOVE_FORWARD))
        {
            Move(transform.forward * 10.0f);
        }
    }

    public void Move(Vector3 movement)
    {
        controller.Move(movement * Time.deltaTime);
        ApplyGravity();
    }

    public void Rotate(float angle)
    {
        var prevAngle = transform.rotation.eulerAngles;
        transform.rotation.eulerAngles.Set(prevAngle.x, prevAngle.y + angle, prevAngle.z);
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            controller.Move(Physics.gravity * Time.deltaTime);
        }
    }
}
