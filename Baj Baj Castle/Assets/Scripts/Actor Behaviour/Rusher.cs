using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rusher : Actor
{
    private void Update()
    {
        if (target == null)
        {
            FindAndSetTarget();
        }
        else
        {
            CalculateMovement();
        }
    }

    private void FixedUpdate()
    {
        Move();

        if (target == null)
        {

        }
        else
        {
            LookAt(target.transform.position, ActorType);
        }
    }

    private void FindAndSetTarget()
    {
        target = null;
        var potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        foreach (var t in potentialTargets)
        {
            if (Vector3.Distance(transform.position, t.transform.position) <= ViewRange)
            {
                target = t;
                return;
            }
        }

        potentialTargets = GameObject.FindGameObjectsWithTag("Ally");

        foreach (var t in potentialTargets)
        {
            if (Vector3.Distance(transform.position, t.transform.position) <= ViewRange)
            {
                target = t;
                return;
            }
        }
    }

    private protected void OnDrawGizmos()
    {
        if (target == null) Gizmos.color = Color.yellow;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ViewRange);
    }
}
