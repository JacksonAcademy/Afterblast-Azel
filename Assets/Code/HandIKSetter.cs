using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandIKSetter : MonoBehaviour
{
     public Transform currentHandTarget;
    public GunManager gunManager;
    public int handSide;
    private void Awake()
    {
        if (gameObject.name.Contains("Right"))
            handSide = 1;
        else if(gameObject.name.Contains("Left"))
            handSide = -1;
    }

    private void Update()
    {
        if (currentHandTarget)
        {
            transform.position = currentHandTarget.position;
            transform.rotation = currentHandTarget.rotation;
        }
    }
    public void SetIK(Gun gun)
    {
        if(gun != null)
        {
            if (handSide == 1)
                currentHandTarget = gun.rightHandPoint;
            else
                currentHandTarget = gun.leftHandPoint;
        }
        else
        {
            currentHandTarget = null;
        }

    }
}
