using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Arrow : MonoBehaviour
{

    [SerializeField]
    private Rigidbody rb;

    private bool didHit;

    private void Start()
    {
        rb.isKinematic=true;
        didHit=false;
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
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
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
