using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandIKSetter : MonoBehaviour
{
     public Transform currentHandTarget;

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
        if (GunManager.instance.equippedGun != null)
        {
            if (handSide == 1)
            {
                currentHandTarget = GunManager.instance.equippedGun.rightHandPoint;
            }
            else
            {
                currentHandTarget = GunManager.instance.equippedGun.leftHandPoint;
            }

        }
        if(currentHandTarget)
        {
            transform.position = currentHandTarget.position;
            transform.rotation = currentHandTarget.rotation;
        }

    }
}
