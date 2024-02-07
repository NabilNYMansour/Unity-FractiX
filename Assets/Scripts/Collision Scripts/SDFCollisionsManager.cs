using UnityEngine;

public struct SphereColliderDatum
{
    public Vector3 position;
    public float radius;
    public float isColliding;
}

/// <summary>
/// Handles the collisions of gameobjects with the SDF terrain.
/// </summary>
public class SDFCollisionsManager : MonoBehaviour
{
    [Header("Compute Shader Params")]
    public ComputeShader computeShader;
    public int scene = 0;

    private SDFSphereCollider[] colliders;
    private SphereColliderDatum[] data;
    private ComputeBuffer buffer;
    private static int datumSize = sizeof(float) * 5;

    void Start()
    {
        colliders = FindObjectsOfType<SDFSphereCollider>();
        data = new SphereColliderDatum[colliders.Length];
    }

    void UpdateData()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            SphereColliderDatum datum;
            datum.position = colliders[i].transform.position;
            datum.radius = colliders[i].radius;
            datum.isColliding = colliders[i].isCollidingWithSDF ? 1f : -1f;
            data[i] = datum;
        }
    }


    void CompShaderUpdate()
    {
        buffer = new ComputeBuffer(data.Length, datumSize);
        buffer.SetData(data);

        computeShader.SetBuffer(0, "data", buffer);
        computeShader.SetInt("_scene", scene);

        computeShader.Dispatch(0, Mathf.CeilToInt(data.Length / 64f), 1, 1);
        buffer.GetData(data);

        for (int i = 0; i < colliders.Length; i++)
        {
            bool collisionCheck = data[i].isColliding > 0f;
            colliders[i].enteredCollisionWithSDF = !colliders[i].isCollidingWithSDF && collisionCheck;
            colliders[i].exitedCollisionWithSDF = colliders[i].isCollidingWithSDF && !collisionCheck;
            colliders[i].isCollidingWithSDF = collisionCheck;
        }

        buffer.Dispose();
    }

    void FixedUpdate()
    {
        UpdateData();
        CompShaderUpdate();
    }
}
