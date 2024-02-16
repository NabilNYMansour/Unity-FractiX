using UnityEngine;

public class SDFSphereCollider : MonoBehaviour
{
    [HideInInspector]
    public bool enteredCollisionWithSDF;
    [HideInInspector]
    public bool isCollidingWithSDF;
    [HideInInspector]
    public bool exitedCollisionWithSDF;
    public float radius;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
