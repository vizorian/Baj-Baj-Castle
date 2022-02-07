using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Actor
{
    protected override void Update()
    {
        ProcessInputs();
        LookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition), actorType);
        FindAndSetInteractable();
        if (interactionObject != null)
            interactionObject.SendMessage("OnCollide", _boxCollider);
    }

    void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Processes the incoming inputs
    /// </summary>
    private void ProcessInputs()
    {
        // Getting inputs
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");


        // Recalculating the movement direction
        moveDelta = new Vector2(x, y).normalized;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {

        }
        if (Input.GetKeyDown(KeyCode.E) && interactionObject != null)
        {
            interactionObject.SendMessage("OnInteraction");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {

        }
    }

    private void FindAndSetInteractable()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Interactable");

        List<GameObject> interactableObjects = new List<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (Vector3.Distance(transform.position, obj.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)) <= InteractionRange)
            {
                interactableObjects.Add(obj);
            }
            else
            {
                interactableObjects.Remove(obj);
            }
        }

        if (interactableObjects.Count == 0)
        {
            interactionObject = null;
            return;
        }

        foreach (GameObject obj in interactableObjects)
        {

            if (interactionObject != null)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < Vector3.Distance(transform.position, interactionObject.transform.position))
                {
                    interactionObject = obj;
                }
            }
            else
            {
                interactionObject = obj;
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        if (interactionObject == null) Gizmos.color = Color.yellow;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, InteractionRange);
    }
}
