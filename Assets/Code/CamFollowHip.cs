using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowHip : MonoBehaviour
{
    public Transform hipBone;

    public Vector3 offset;
    public float smoothSpeed;

    public bool followX, followY, followZ;
    private void Awake()
    {
        offset = transform.localPosition;
    }
    private void LateUpdate()
    {
        Vector3 wantedPosition = hipBone.position + offset;
        if (!followX)
            wantedPosition.x = transform.position.x;
        if (!followY)
            wantedPosition.y = transform.position.y;
        if (!followZ)
            wantedPosition.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, wantedPosition, smoothSpeed * Time.deltaTime);
    }
}
