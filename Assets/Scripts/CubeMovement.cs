using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float dampen;
    public GameObject teleporter;

    private Vector3 initPos;
    private bool inMovement = false;
    private bool inReturn = false;
    private float minVel = 0.005f;
    private float initSpeed = 0;
    private Rigidbody rb;
    public BoxSelect bsScript;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initPos = this.transform.position;
    }
    async void delayInReturn()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        Vector3 start_pos = this.transform.position;
        Vector3 end_pos = teleporter.transform.position;
        rb.velocity = (end_pos - start_pos).normalized * initSpeed;
        inReturn = true;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = rb.velocity;
        if (!bsScript.isSelected(transform.gameObject))
        {
            if (!inMovement && !inReturn && velocity != Vector3.zero)
            {
                initSpeed = velocity.magnitude;
                inMovement = true;
            }
            else if (inMovement && velocity.magnitude < minVel)
            {
                rb.velocity = Vector3.zero;
                inMovement = false;
                delayInReturn();
            }
            else if (!inMovement && !inReturn && velocity.magnitude >= minVel)
            {
                inMovement = true;
            }
            if (inMovement)
            {
                rb.velocity = dampen * rb.velocity;
            }
            if (inReturn && (teleporter.transform.position - transform.position).magnitude < 0.18f)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                this.transform.rotation = Quaternion.identity;
                this.transform.position = initPos;
                inReturn = false;
            }
            else if (inReturn)
            {
                rb.velocity = (teleporter.transform.position - transform.position).normalized;
            }
        }
    }
}
