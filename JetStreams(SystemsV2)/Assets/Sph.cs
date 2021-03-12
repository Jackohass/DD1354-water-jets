using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sph : MonoBehaviour
{
    public GameObject prefab;
    public float radius = 1;
    public float mass = 0.5f;
    public float restdensity = 1;
    public float viscos = 1;
    public float drag = 0.1f;

    public bool wallsUp;
    public int num = 10;
    public int perRow = 5;
    public List<GameObject> walls = new List<GameObject>();

    //The smoothing radius.
    public float smoothingRadius = 1.0f;
    public Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);
    private float gravityMult = 2.0f;
    private float gas = 2000.0f;
    public float deltaT = 0.0008f;
    public float damping = -0.5f;

    private Particle[] particles;
    public ParticleCollider[] colliders;
    private bool clearing;

    // Start is called before the first frame update
    void Start()
    {
        intialize();
        //Update();
    }

    private void intialize()
    {
        particles = new Particle[num];
        //colliders = new ParticleCollider[num];
        Debug.Log("num: " + num);

        for (int i = 0; i < num; i++)
        {
            float x = (i % perRow) + Random.Range(-0.1f, 0.1f);
            float y = (2*radius) + (float)((i/perRow)/perRow)*1.1f;
            float z = ((i/perRow) % perRow) + Random.Range(-0.1f, 0.1f);

            GameObject go_i = Instantiate(prefab);
            Particle particle_i = go_i.AddComponent<Particle>();
            Debug.Log(i);
            particles[i] = particle_i;
            //colliders[i] = go_i.GetComponent<ParticleCollider>();

            go_i.transform.localScale = Vector3.one * radius;
            go_i.transform.position = new Vector3(x, y, z);

            particles[i].go = go_i;
            particles[i].pos = go_i.transform.position;
        }
    }

    private float ParticleDensity(Particle particle, float distance)
    {
        if (distance < smoothingRadius)
        {
            return particle.density += mass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f)))
                * Mathf.Pow(smoothingRadius - distance, 3.0f);
        }
        return particle.density;
    }

    private void CalculateForces()
    {
        for(int i = 0; i < particles.Length; i++)
        {
            if (clearing) return;

            for (int j = 0; j < particles.Length; j++)
            {
                Vector3 direction = particles[j].pos - particles[i].pos;
                float distance = direction.magnitude;

                particles[i].density = ParticleDensity(particles[i], distance);
                particles[i].pressure = gas * (particles[i].density - restdensity);
            }
        }
    }

    private Vector3 ParticlePressure(Particle particle, Particle nextParticle, Vector3 direction, float distance)
    {
        if(distance < smoothingRadius)
        {
            return -1 * (direction.normalized) * mass * (particle.pressure + nextParticle.pressure) / (2.0f * nextParticle.density)
                * (-45.0f / (Mathf.Pow(smoothingRadius, 6.0f))) * Mathf.Pow(smoothingRadius - distance, 2.0f);
        }
        return Vector3.zero;
    }

    private Vector3 ParticleViscos(Particle particle, Particle nextParticle, float distance)
    {
        if (distance < smoothingRadius)
        {
            return viscos * mass * (nextParticle.vel - particle.vel) / nextParticle.density * (45.0f * (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * (smoothingRadius - distance);
        }
        return Vector3.zero;
    }

    private void ParticleMovement()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (clearing) return;

            Vector3 fPressure = Vector3.zero;
            Vector3 fViscos = Vector3.zero;

            for (int j = 0; j < particles.Length; j++)
            {
                if (i == j) continue;

                Vector3 direction = particles[j].pos - particles[i].pos;
                float distance = direction.magnitude;

                fPressure += ParticlePressure(particles[i], particles[j], direction, distance);
                fViscos += ParticleViscos(particles[i], particles[j], distance);
            }

            Vector3 fGravity = gravity * particles[i].density * gravityMult;

            particles[i].combF = fPressure + fViscos + fGravity;
            particles[i].vel += deltaT * (particles[i].combF) / particles[i].density;
            particles[i].pos += deltaT * (particles[i].vel);
            particles[i].go.transform.position = particles[i].pos;
        }
    }

    private static bool Collision(ParticleCollider collider, Vector3 pos, float radius, 
        out Vector3 penetrationNorm, out Vector3 penetrationPos, out float penetrationLen)
    {
        Vector3 colliderProj = collider.pos - pos;

        penetrationNorm = Vector3.Cross(collider.right, collider.up);
        penetrationPos = collider.pos - colliderProj;
        penetrationLen = Mathf.Abs(Vector3.Dot(colliderProj, penetrationNorm)) - (radius / 2.0f);

        return penetrationLen < 0.0f
            && Mathf.Abs(Vector3.Dot(colliderProj, collider.right)) < collider.scale.x
            && Mathf.Abs(Vector3.Dot(colliderProj, collider.up)) < collider.scale.y;
    }

    private Vector3 DampenVelocity(ParticleCollider collider, Vector3 vel, Vector3 penetrationNorm, float drag)
    {
        Vector3 newVel = Vector3.Dot(vel, penetrationNorm) * penetrationNorm * damping + Vector3.Dot(vel, collider.right) *
            collider.right * drag + Vector3.Dot(vel, collider.up) * collider.up * drag;

        return Vector3.Dot(newVel, Vector3.forward) * Vector3.forward + Vector3.Dot(newVel, Vector3.right) * Vector3.right
            + Vector3.Dot(newVel, Vector3.up) * Vector3.up;
    }

    private void CalculateCollision()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            for (int j = i; j < colliders.Length; j++)
            {
                if (clearing || colliders.Length == 0) return;

                Vector3 penetrationNorm;
                Vector3 penetrationPos;

                float penetrationLen;
                if (Collision(colliders[j], particles[i].pos, radius, out penetrationNorm, out penetrationPos, out penetrationLen))
                {
                    particles[i].vel = DampenVelocity(colliders[j], particles[i].vel, penetrationNorm, 1.0f - drag);
                    particles[i].pos = penetrationPos - penetrationNorm * Mathf.Abs(penetrationLen);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateForces();
        ParticleMovement();
        CalculateCollision();
    }
}
