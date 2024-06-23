using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update() // update works better than LateUpdate, but It should be done in LateUpdate...
    {
        if (!mainCam)
            mainCam = Camera.main;

        transform.forward = mainCam.transform.forward;
        //transform.rotation = Quaternion.LookRotation( transform.position - cam.position );
    }

}
