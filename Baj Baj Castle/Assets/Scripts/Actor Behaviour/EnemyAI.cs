using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : Actor
{
    private bool isAngered = false;

    private protected void Update()
    {
        if(target == null)
            FindAndSetTarget();
        else
        {
            LookAt(target.transform.position, actorType);
            CalculatePath();
            Move();
        }
    }

    private void FixedUpdate()
    {
        _hand.UpdateCenterPosition(transform.position);
        if(target != null)
            _hand.LookTowards(target.transform.position);
    }

    private void CalculatePath()
    {
    }   

    private void FindAndSetTarget()
    {
        var potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        foreach (var t in potentialTargets)
        {
            if(Vector3.Distance(transform.position, t.transform.position) <= ViewRange)
            {
                target = t;
                isAngered = true;
                return;
            }
        }

        potentialTargets = GameObject.FindGameObjectsWithTag("Ally");

        foreach (var t in potentialTargets)
        {
            if (Vector3.Distance(transform.position, t.transform.position) <= ViewRange)
            {
                target = t;
                isAngered = true;
                return;
            }
        }
    }

    private protected void OnDrawGizmos()
    {
        //if (target == null) Gizmos.color = Color.yellow;
        //else Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, ViewRange);
    }
}
