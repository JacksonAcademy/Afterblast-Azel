using GameKit.Dependencies.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public AnimationCurve curve1, curve2;

    float t = 0f;
    public float speed;
    private Vector3 startPos;
    private Vector3 startRot;
    public float rotMultiplier;
    public Vector3 posMultiplier;
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.eulerAngles;
    }
    void Update()
    {
        //...clever code

        t += Time.deltaTime * speed;
        float x = Shake(10f, 0, curve1);
        float y = Shake(10f, 150, curve1);
        float r = Shake(10f, 500, curve2);

        //apply this shake to whatever you want, it's 1d right now but can be easily expanded to 2d
        transform.position = startPos + (new Vector3(x, y).Multiply(posMultiplier));
        transform.eulerAngles = startRot + new Vector3(0, 0, r*rotMultiplier);
    }

    float Shake(float shakeDamper, float offset, AnimationCurve curve)
    {
        return Mathf.PerlinNoise(t / shakeDamper, 0f) * curve.Evaluate(t + offset);
    }
}
