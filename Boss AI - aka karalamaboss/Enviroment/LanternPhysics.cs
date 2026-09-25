using System;
using UnityEngine;

public class LanternPhysics : MonoBehaviour
{
    private String m_PlayerTag = "Player";
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (rb.isKinematic)
            return;

        if (collision.tag == "Lantern")
        {
            Rigidbody2D otherRb = collision.attachedRigidbody;
            if (otherRb == null)
                return;

            Vector2 relative = otherRb.velocity - rb.velocity;

            Vector2 soft = new Vector2(
                Soft(relative.x),
                Soft(relative.y)
            );

            rb.velocity += soft;
            otherRb.velocity -= soft;
        }
        Rigidbody2D collisionRb = collision.GetComponent<Rigidbody2D>();

        if (collisionRb == null)
            return;

        print((collisionRb.velocity.x));
        rb.velocity = rb.velocity + new Vector2(Formula(collisionRb.velocity.x), Formula(collisionRb.velocity.y));

    }

    private float Soft(float x)
    {
        return Mathf.Log(Mathf.Abs(x) + 1, 2) * Mathf.Sign(x) * 2f;
    }


    private float Formula(float x)
    {
        return Mathf.Log(Mathf.Abs(x) + 1, 2) * Mathf.Sign(x) * 2;
    }

}
