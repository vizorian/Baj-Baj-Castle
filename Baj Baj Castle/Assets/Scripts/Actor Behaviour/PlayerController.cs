using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : Actor
{
    private protected void Update()
    {
        ProcessInputs();
        LookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition), actorType);
        
        
        FindInteractable();
        if (interactionObject != null)
            interactionObject.SendMessage("OnCollide", _boxCollider);
    }

    private void FixedUpdate()
    {
        Move();
        _hand.UpdateCenterPosition(transform.position);
        _hand.LookTowards(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    /// <summary>
    /// Processes the incoming inputs
    /// </summary>
    private void ProcessInputs()
    {
        // Getting inputs
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        float scrollWheelDelta = Input.GetAxisRaw("Mouse ScrollWheel");

        // Recalculating the movement direction
        moveDelta = new Vector2(x, y).normalized;

        // Left click
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(InventorySystem.Instance.selectedItem != null)
            {
                Debug.Log($"You just used {InventorySystem.Instance.selectedItem.Data.DisplayName}");
            }
        }

        // Interaction button
        if (Input.GetKeyDown(KeyCode.E) && interactionObject != null)
        {
            interactionObject.SendMessage("OnInteraction");
        }

        // Drop button
        if (Input.GetKeyDown(KeyCode.G))
        {
            InventorySystem.Instance.Drop();
        }

        // Reload button
        if (Input.GetKeyDown(KeyCode.R))
        {

        }

        // Scroll wheel
        if (scrollWheelDelta != 0)
        {
            if(scrollWheelDelta > 0)
                InventorySystem.Instance.Next();
            else
                InventorySystem.Instance.Previous();
        }
    }

    private void FindInteractable()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Interactable");

        // Set interactionObject to first closest or null
        interactionObject = objects.ToList()
            .Where(o => Vector3.Distance(transform.position, o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)) <= InteractionRange)
            .OrderBy(o => Vector3.Distance(transform.position, o.GetComponent<BoxCollider2D>().ClosestPoint(transform.position)))
            .FirstOrDefault();
    }

    private protected void OnDrawGizmos()
    {
        //if (interactionObject == null) Gizmos.color = Color.yellow;
        //else Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, InteractionRange);

        Gizmos.DrawWireSphere(transform.position, HandRange);
    }
}
