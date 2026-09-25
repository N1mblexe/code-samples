using System.Threading;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool grounded;
    private static int count = 0;

    void Update()
    {
        grounded = IsGrounded();
    }
    public static bool IsGrounded()
    {
        return (count == 0)? false : true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            count++;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            count--;
    }
}
