using UnityEngine;

[RequireComponent(typeof(SDFSphereCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class MaterialChanger : MonoBehaviour
{
    public Material blueMat;
    public Material redMat;

    public ParticleSystem particlesPrefab;

    private SDFSphereCollider sdfCollider;
    private MeshRenderer meshRenderer;

    void Start()
    {
        sdfCollider = GetComponent<SDFSphereCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void EmitParticles(Material mat)
    {
        ParticleSystem particles = Instantiate(particlesPrefab, transform.position, transform.rotation);
        particles.GetComponent<Renderer>().material = mat;
        particles.Play();
        Destroy(particles.gameObject, particles.main.duration);
    }

    void FixedUpdate()
    {
        if (sdfCollider.isCollidingWithSDF)
        {
            meshRenderer.material = redMat;
        } else
        {
            meshRenderer.material = blueMat;
        }

        if (sdfCollider.enteredCollisionWithSDF)
        {
            EmitParticles(redMat);
        } else if (sdfCollider.exitedCollisionWithSDF)
        {
            EmitParticles(blueMat);
        }
    }
}
