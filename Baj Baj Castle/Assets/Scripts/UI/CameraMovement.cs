using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float boundX = 0.1f;
    public float boundY = 0.5f;

    private void Start()
    {
        if (target == null)
        {
            target = GameObject.Find("Player").transform;
        }
    }

    void LateUpdate()
    {
        Vector3 delta = Vector3.zero;

        // Check if in bounds on X axis
        float deltaX = target.position.x - transform.position.x;
        if (deltaX > boundX || deltaX < -boundX)
        {
            if (transform.position.x < target.position.x)
            {
                delta.x = deltaX - boundX;
            }
            else
            {
                delta.x = deltaX + boundX;
            }
        }

        // Check if in bounds on Y axis
        float deltaY = target.position.y - transform.position.y;
        if (deltaY > boundY || deltaY < -boundY)
        {
            if (transform.position.y < target.position.y)
            {
                delta.y = deltaY - boundY;
            }
            else
            {
                delta.y = deltaY + boundY;
            }
        }

        transform.position += delta;

        //delta = new Vector3(deltaX, deltaY);

        //transform.Translate(delta * cameraSpeed * Time.deltaTime);
    }
}
