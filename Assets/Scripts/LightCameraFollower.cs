using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightCameraFollower : MonoBehaviour
{
    public GameObject toFollow;
    private Camera lightCam;

    private void Start()
    {
        lightCam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        this.transform.position = toFollow.transform.position - lightCam.transform.forward * 25;
    }
}
