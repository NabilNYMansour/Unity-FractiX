using UnityEngine;

public class SDFSphereCollider : MonoBehaviour
{
    public bool enteredCollisionWithSDF;
    public bool isCollidingWithSDF;
    public bool exitedCollisionWithSDF;
    public float radius;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
