using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    public sound footstep;
    public void APlayFootstep(float volumeMultiplier)
    {
        footstep.Play(transform.position, 1, null, volumeMultiplier);
    }
}
