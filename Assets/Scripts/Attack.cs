using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
    public float start;
    public float length;

    public float hitstun;
    public float hitStop; // also called hitPause

    public Vector2 hitAni;
    public Vector3 knockback;

    public Vector3 hitboxPos;
    public Vector3 hitboxScale;

    public float cancelWindow;
}

