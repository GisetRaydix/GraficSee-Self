using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    #region Variable declaration
    [SerializeField,Range(-3,3)] public float TrasformX = 0;
    [SerializeField,Range(0,2.5f)] public float TrasformY = 1.38f;
    [SerializeField,Range(-0.5f,-3.5f)] public float TrasformZ = -0.5f;
    [SerializeField, Range(-20, 30)] public float RotationX = 0;
    [SerializeField, Range(-25, 25)] public float RotationY = 0;
    [SerializeField, Range(-180, 180)] public float RotationZ = 0f;
    #endregion

    void Update()
    {
        CameraValue();
    }

    private void OnValidate()
    {
        CameraValue();
    }

    private void CameraValue()
    {
        this.gameObject.transform.position = new Vector3(TrasformX, TrasformY, TrasformZ);
        this.gameObject.transform.rotation = Quaternion.Euler(RotationX, RotationY, RotationZ);
    }
}
