using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody2D))]
public class Floats : MonoBehaviour
{
    private Rigidbody2D rb;
    public float Bouyancy = 10;
    [Tooltip("This is used to change the center of mass of objects. This makes objects float 'Deeper'")]
    public float VerticalOffset = 0;
    private EdgeCollider2D edge;
    System.Collections.Generic.List<Vector2> edgePoints = new();
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        DoFloat();
    }

    private void DoFloat()
    {
        if (WaterSplash.Instance == null || !WaterSplash.Instance.isActiveAndEnabled)
        {
            return;
        }
        if (edge == null)
        {
            WaterSplash wave = WaterSplash.Instance;
            edge = wave.GetComponent<EdgeCollider2D>();
        }
        edge.GetPoints(edgePoints);
        float middleY = edgePoints.Sum(x => x.y + edge.transform.position.y) / edgePoints.Count;
        float totalForce = 0;
        float verticalPos = VerticalOffset + transform.position.y;
        if (verticalPos >= middleY)
        {
            var overlaping = edgePoints.Select(x => new Vector2(edge.transform.position.x, edge.transform.position.y) + x).Where(x => rb.OverlapPoint(x)).ToArray();
            if (overlaping.Length == 0)
            {
                return;
            }
            float power = Mathf.Clamp((middleY - verticalPos) * Bouyancy, 0, Bouyancy);
            power *= 4 / Mathf.Min(4, overlaping.Length);

            foreach (var contact in overlaping)
            {
                float penetration = (contact - rb.ClosestPoint(contact + Vector2.down * 50)).magnitude / Bouyancy;
                //if (rb.angularVelocity < 2)
                //{
                //    rb.AddForceAtPosition(Vector3.up * totalForce * penetration, overlaping[0]);
                //}
                totalForce += power * penetration;
            }
            float torque = Mathf.Clamp(rb.rotation * Mathf.Deg2Rad, -1, 1);
            Debug.Log(new { torque, rb.rotation });
            rb.AddTorque(-torque);
        }
        else
        {
            totalForce = Bouyancy;
        }
        totalForce = Mathf.Min(Bouyancy, totalForce);
        if (rb.linearVelocityY > 1)
        {
            return;
        }
        rb.AddForce(Vector3.up * totalForce);
    }

    float originalAngularDamping = 0;
    float originalLinearDamping = 0;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wave"))
        {
            originalAngularDamping = rb.angularDamping;
            originalLinearDamping = rb.linearDamping;
            rb.linearDamping = 1;
            rb.angularDamping = 8;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wave"))
        {
            rb.angularDamping = originalAngularDamping;
            rb.linearDamping = originalLinearDamping;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
    }
}
