using UnityEngine;

public class BoidController : MonoBehaviour
{
    //======|Public params|======//
    [Header("Boid Manager")]
    public BoidsManager manager;

    [Header("Boid speed and rotation params")]
    public float boidMovementSpeed = 10f;
    public float boidRotationSpeed = 0.05f;

    [Header("Spawn and death particle system")]
    public ParticleSystem particle;

    [Header("Boid behaviour params")]
    public Vector3 combinedBoidRule = Vector3.zero;
    public float distanceToCamera = 1e20f;
    public float terrainDistance = 1e20f;
    public int boidsSensed = 0;

    //======|Private params|======//
    private float rotationTime = 1.5f;
    private Quaternion currentRotation;
    private Quaternion nextRotation;

    /// <summary>
    /// Emits particles and scales them according to the input value.
    /// </summary>
    /// <param name="scale"></param>
    void EmitParticles(float scale)
    {
        ParticleSystem particles = Instantiate(particle, transform.position, transform.rotation);
        particles.transform.localScale = transform.localScale * scale;
        particles.Play();
        Destroy(particles.gameObject, particles.main.duration);
    }

    /// <summary>
    /// Applies boid behaviour on this game obeject.
    /// </summary>
    void BoidBehaviour()
    {
        // update rotation
        if (rotationTime < 1f)
        {
            rotationTime += boidRotationSpeed;
        }
        else if (rotationTime > 1f)
        {
            currentRotation = transform.rotation;
            nextRotation = combinedBoidRule.magnitude > 0.5f ? Quaternion.LookRotation((combinedBoidRule + Random.onUnitSphere / 10)) : Random.rotation;
            rotationTime = 0f;
        }
        transform.rotation = Quaternion.Slerp(currentRotation, nextRotation, rotationTime);

        this.transform.position += this.transform.forward * boidMovementSpeed * Time.deltaTime;
    }

    void Start()
    {
        EmitParticles(1f);
        currentRotation = this.transform.rotation;
    }

    private void FixedUpdate()
    {
        BoidBehaviour(); // apply boid behaviour
        if (distanceToCamera > 2000f) // if boid is outside visible area
        {
            Destroy(this.gameObject); // destroy boid
        }
        else if (terrainDistance < 0f) // if boid hits fractal terrain
        {
            EmitParticles(3f); // emit larger particles
            Destroy(this.gameObject); // and distroy boid
        }
    }
}
