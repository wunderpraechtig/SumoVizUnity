using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointerConfigurer : MonoBehaviour
{
    private LaserPointer laserPointer;
    [SerializeField] private LaserPointer.LaserBeamBehavior beamBeahvior = LaserPointer.LaserBeamBehavior.OnWhenHitTarget;

    void Awake()
    {
        laserPointer = this.GetComponent<LaserPointer>();
        laserPointer.laserBeamBehavior = beamBeahvior;
    }
}
