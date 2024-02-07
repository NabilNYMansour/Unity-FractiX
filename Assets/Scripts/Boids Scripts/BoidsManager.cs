using UnityEngine;

/// <summary>
/// Datum will be the struct that is used to communicate with the compute shader. The buffer will be read/write so
/// when reading the data, the gpu will expect an input of position for the first vector, and the forward vector of the boid
/// for the second vector. The output will be the combined boid rule for the first vector, and terrain hit dis and how many boids were sensed.
/// </summary>
public struct Datum
{
    public Vector3 fstVector; // input is pos, output is combined boid rule
    public Vector3 sndVector; // input is forward vector, output is Vector3(0, terrain hit distance, boids sensed)
}

public class BoidsManager : MonoBehaviour
{
    //======|Public params|======//
    [Header("Camera and boid prefab")]
    public GameObject boidPrefab;
    public Transform cam;

    [Header("Instantiation Params")]
    public int boidsMaxNumber = 1000;
    public float instantiationMinRadius = 750;
    public float instantiationMaxRadius = 950;
    public float instantiationDelay = 0.1f; // will make a new boid every X seconds
    public int batchCount = 5;
    public float batchSpawnRadius = 25f;

    [Header("Boids Params")]
    [Range(0f, 360f)]
    public float ViewAngle = 270f;
    public float ViewDistance = 100;
    public float boidSpeed = 25;
    public float boidRotSpeed = 0.01f;

    [Header("Compute Shader")]
    public ComputeShader computeShader;

    //======|Shader private params|======//
    private Datum[] data;
    private ComputeBuffer buffer;
    private static int datumSize = 2 * sizeof(float) * 3;

    //======|Other|======//
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        data = new Datum[boidsMaxNumber + batchCount];
    }

    /// <summary>
    /// Updates the data array that will be used in the compute shader
    /// </summary>
    void UpdateDataStructs()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            // input
            Datum datum = new Datum();
            datum.fstVector = child.position;
            datum.sndVector = child.forward;

            data[i] = datum;

            i++;
        }
    }

    /// <summary>
    /// Boid comp shader dispatcher.
    /// </summary>
    void CompShaderUpdate()
    {
        UpdateDataStructs();

        buffer = new ComputeBuffer(data.Length, datumSize);
        buffer.SetData(data); // Set input (position and forward vector)

        computeShader.SetBuffer(0, "data", buffer);
        computeShader.SetInt("_dataLength", data.Length);
        computeShader.SetFloat("_viewAngle", ViewAngle);
        computeShader.SetFloat("_viewDistance", ViewDistance);
        computeShader.SetInt("_scene", 0); // use first scene

        computeShader.Dispatch(0, Mathf.CeilToInt(data.Length / 10), 1, 1);
        buffer.GetData(data);

        int i = 0;
        foreach (Transform child in transform) // update data for each child
        {
            BoidController childBoidBehaviour = child.GetComponent<BoidController>();
            childBoidBehaviour.combinedBoidRule = data[i].fstVector;
            childBoidBehaviour.distanceToCamera = data[i].sndVector.x;
            childBoidBehaviour.terrainDistance = data[i].sndVector.y;
            childBoidBehaviour.boidsSensed = (int)data[i].sndVector.z;

            ++i;
        }

        buffer.Dispose();
    }

    /// <summary>
    /// Instantiates a new boid given its parameters.
    /// </summary>
    private void InstantiateNewCollectable(GameObject boidPrefab, Vector3 pos, Quaternion rot, float boidSpeed, float boidRotSpeed)
    {
        GameObject collectable = Instantiate(boidPrefab, pos, rot, this.transform);
        BoidController collectableController = collectable.GetComponent<BoidController>();

        collectableController.manager = this;
        collectableController.boidMovementSpeed = boidSpeed;
        collectableController.boidRotationSpeed = boidRotSpeed;
    }

    /// <summary>
    /// Will generate a random point around a cone of vision of the camera between two distances.
    /// </summary>
    /// <param name="minDistance">Minimum random point distance</param>
    /// <param name="maxDistance">Maximum random point distance</param>
    /// <param name="coneAngle">The angle of the cone</param>
    /// <returns>A random point infront of the camera</returns>
    Vector3 GenerateRandomPointCone(float minDistance, float maxDistance, float coneAngle)
    {
        // Calculate a random distance within the specified range
        float randomDistance = Random.Range(minDistance, maxDistance);

        // Calculate a random angle within the cone's range
        float randomAngleX = Random.Range(-coneAngle / 2f, coneAngle / 2f);
        float randomAngleY = Random.Range(-coneAngle / 2f, coneAngle / 2f);

        // Calculate the offset from the player based on the random distance and angle
        Vector3 offset = cam.forward * randomDistance;

        // Rotate the offset based on the random angle
        offset = Quaternion.Euler(randomAngleX, randomAngleY, 0f) * offset;

        return offset + cam.position;
    }

    void FixedUpdate()
    {
        if (transform.childCount < boidsMaxNumber && Time.time - startTime >= instantiationDelay)
        {
            Vector3 randomPointAroundPlayer = GenerateRandomPointCone(instantiationMinRadius, instantiationMaxRadius, 90);
            for (int i = 0; i < batchCount; ++i)
            {
                InstantiateNewCollectable(boidPrefab, randomPointAroundPlayer + Random.insideUnitSphere * batchSpawnRadius, Random.rotation, boidSpeed, boidRotSpeed);
            }

            // Reset the timer for the next second
            startTime = Time.time;
        }
        if (transform.childCount > 0) { CompShaderUpdate(); }
    }
}
