using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExtraPhysics: MonoBehaviour
{
    void Start()
    {
        Physics.IgnoreLayerCollision(9, 10); // Ignore player-pedestrian collision
    }
}
