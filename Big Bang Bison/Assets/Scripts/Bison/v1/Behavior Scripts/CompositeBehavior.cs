﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Herd/Behavior/Composite")]
public class CompositeBehavior : HerdBehavior
{
    public HerdBehavior[] behaviors;
    public float[] weights;

    public override Vector3 CalculateMove(HerdAgent agent, List<Transform> context, Herd herd)
    {
        // handle data mismatch
        if (behaviors.Length != weights.Length)
        {
            Debug.LogError("Data missmatch in " + name, this);
            return Vector3.zero;
        }

        // set up move
        Vector3 move = Vector3.zero;

        // iterate through behaviors
        for (int i = 0; i < behaviors.Length; i++)
        {
            Vector3 partialMove = behaviors[i].CalculateMove(agent, context, herd) * weights[i];

            if (partialMove != Vector3.zero)
            {
                if (partialMove.sqrMagnitude > weights[i] * weights[i])
                {
                    partialMove.Normalize();
                    partialMove *= weights[i];
                }

                move += partialMove;
            }
        }

        return move;
    }
}