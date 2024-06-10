using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAt : MonoBehaviour
{
    public Transform objectToLookAt;
    public int maxRotate;
    public int minRotate;

    public void LateUpdate()
    {
        transform.LookAt(objectToLookAt);
        transform.localEulerAngles = new Vector3(ClampAngle(transform.localEulerAngles.x, minRotate, maxRotate), 0, 0);
    }
    float ClampAngle(float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }
}
