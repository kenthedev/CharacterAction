using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEngine : MonoBehaviour
{
    public string horizontalAxis = "RightStickX"; // defined in Unity Editor: Edit > Project Settings > Input
    public string verticalAxis = "RightStickY";

    public GameObject rig;
    public GameObject transformTarget;
    public GameObject lookTarget;
    public Vector3 transformOffset;
    public GameObject myCamera;
    public float orbH;
    public float orbV;
    public float orbVMin = -89f;
    public float orbVMax = 89f;
    public float rotSpeed = 2;
    public float rotBoost = 0f;
    public float rotAccel = 0.075f;
    public float rotBoostMax = 5;
    public float orbSpeed = 0.5f;
    public float targetOrbH;
    public float targetOrbV;
    public float orbitHelp = 1.05f;

    public float camDistance = 8f;

    public float deadzone = 0.5f;
    public float invertX = 1;
    public float invertY = 1;

    public Vector3 translateSpeed = new Vector3(0.75f, 0.075f, 0.75f); // x and z should be the same generally as this is the horizontal plane

    public bool focusTarget;

    public Vector3 lookEuler;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        transformTarget = GameEngine.GetMainCharacterObject();
    }

    // Update is called once per frame
    void Update()
    {
        transformTarget = GameEngine.GetMainCharacterObject();

        rig.transform.localPosition = new Vector3(0, 0, -camDistance);
        OrbitView();
        Translate();
        SettleCameras();
    }


    void SettleCameras()
    {
        myCamera.transform.localPosition += (Vector3.zero - myCamera.transform.localPosition) * 0.25f;
    }

    void Translate()
    {
        if (transformTarget == null) { return; }
        transform.position += Vector3.Scale((transformTarget.transform.position +
            (Quaternion.LookRotation(new Vector3(transform.forward.x, 0f, transform.forward.z), Vector3.up) * transformOffset) - transform.position), translateSpeed);
    }

    void OrbitView()
    {
        if (Input.GetAxisRaw(horizontalAxis) > deadzone)
        {
            targetOrbH += (rotBoost + rotSpeed) * invertX;
            rotBoost += rotAccel;
        }
        if (Input.GetAxisRaw(horizontalAxis) < -deadzone)
        {
            targetOrbH += -(rotBoost + rotSpeed) * invertX;
            rotBoost += rotAccel;
        }
        if (Input.GetAxisRaw(verticalAxis) > deadzone) // DOWN
        {
            targetOrbV += (rotBoost + rotSpeed) * invertY;
            rotBoost += rotAccel;
        }
        if (Input.GetAxisRaw(verticalAxis) < -deadzone) // UP
        {
            targetOrbV += -(rotBoost + rotSpeed) * invertY;
            rotBoost += rotAccel;
        }

        if (Input.GetAxisRaw(horizontalAxis) < deadzone &&
            Input.GetAxisRaw(horizontalAxis) > -deadzone &&
            Input.GetAxisRaw(verticalAxis) < deadzone && 
            Input.GetAxisRaw(verticalAxis) > -deadzone) 
        {
            rotBoost = 0;
        }

        if (rotBoost > rotBoostMax) { rotBoost = rotBoostMax; }

        targetOrbV = Mathf.Clamp(targetOrbV, orbVMin, orbVMax);

        if (!focusTarget) { targetOrbH -= LookAtOffset() * orbitHelp * Mathf.Lerp(1f, 0.025f, Mathf.InverseLerp(0f, 90f, Mathf.Abs(orbV))); } // * 180f / Mathf.PI;

        //Ease orbiting
        orbH += (targetOrbH - orbH) * orbSpeed;
        orbV += (targetOrbV - orbV) * orbSpeed;
        transform.rotation = Quaternion.Euler(orbV, orbH, 0);

        if (focusTarget) { transform.rotation = Quaternion.LookRotation(lookTarget.transform.position - transform.position, Vector3.up) * Quaternion.Euler(orbV, 0, 0); }
    }

    public float LookAtOffset()
    {
        if(transformTarget == null) { return 0f; }
        if (rig == null) { return 0f; }
        float lookAtOffset = 0f;

        Vector3 offsetLook = Quaternion.LookRotation(new Vector3(transform.forward.x, 0f, transform.forward.z), Vector3.up) * transformOffset;
        Vector2 currentLook = new Vector2(transform.position.x, transform.position.z) - new Vector2(rig.transform.position.x, rig.transform.position.z) - new Vector2(offsetLook.x, offsetLook.z);
        Vector2 newLook = new Vector2(transformTarget.transform.position.x, transformTarget.transform.position.z) - new Vector2(rig.transform.position.x, rig.transform.position.z);
        Vector3 cross = Vector3.Cross(currentLook, newLook);
        lookAtOffset = Vector2.Angle(currentLook, newLook);
        if (cross.z > 0) { return lookAtOffset; }
        else { return -lookAtOffset; }
    }
}
