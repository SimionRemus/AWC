using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Arrow : MonoBehaviour
{

    [SerializeField]
    private Rigidbody rb;

    private bool didHit;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        rb.isKinematic=true;
        // rb.centerOfMass=new Vector3(0f,0f,0.86f);
        didHit=false;
        // torque=new Vector3(0,5,0);
    }

    public void Fly(Vector3 force)
    {
        rb.isKinematic=false;
        rb.AddForce(force,ForceMode.Impulse);
        //while(!didHit)
        StartCoroutine(LookAt());
        transform.SetParent(null);
    }


    private IEnumerator LookAt()
    {
        while (!didHit)
        {
            PositionTrack();
            transform.LookAt(transform.position + rb.velocity);
            yield return null;
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if(didHit) return;
        
        if(other.collider.CompareTag("Terrain")||other.collider.CompareTag("Player")) //use this to check what was hit, current condition is always true;
        {
            didHit=true;
            StopCoroutine(LookAt());
            rb.isKinematic=true;
            // rb.velocity=Vector3.zero;
            // rb.angularVelocity=Vector3.zero;
            // transform.SetParent(other.transform);
        }
        
    }

    private void PositionTrack()
    {
        Debug.DrawLine(transform.position, transform.position + rb.velocity.normalized, Color.red, 300f,false);
    }
}
