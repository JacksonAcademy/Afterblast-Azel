using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowHip : MonoBehaviour
{
    public Transform hipBone;

    public Vector3 offset;
    public float smoothSpeed;

    private void Awake()
    {
        offset = transform.localPosition;
    }
    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, hipBone.position + offset, smoothSpeed * Time.deltaTime);
    }
}
