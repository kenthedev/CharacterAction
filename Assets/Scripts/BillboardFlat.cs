using System.Collections;
using UnityEngine;

// Attach to as a component to have the game object always face the Camera
public class BillboardFlat : MonoBehaviour
{
    public bool reverse;

    public Vector3 rotMin;
    public Vector3 rotMax;

    public bool strikeAngle;
    [UnityEngine.HideInInspector]
    Vector3 rotationOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        rotationOffset = new Vector3(
            Random.Range(rotMin.x, rotMax.x), 
            Random.Range(rotMin.y, rotMax.y), 
            Random.Range(rotMin.z, rotMin.z));
    }

    // Update is called once per frame
    void Update()
    {
        if (reverse)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * -Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
        else
        {
            transform.LookAt
                (transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
        transform.Rotate(rotationOffset);
    }
}
