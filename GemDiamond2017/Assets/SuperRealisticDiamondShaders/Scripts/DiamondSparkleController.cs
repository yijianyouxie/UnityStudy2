using UnityEngine;

[ExecuteInEditMode]
public class DiamondSparkleController : MonoBehaviour
{
    public ParticleSystem particleSystem;
    ParticleSystem.Particle[] particles;

    private Mesh diamondMesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    public MeshRenderer diamondRenderer;

    private Transform camTr;
    public bool useDotNV = true;
    public float dotThreshold = 0.5f;
    public int maxParticleNum = 10;

    public bool useRandom = true;

    void Start()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        if(null != particleSystem)
        {
            particleSystem.transform.localScale = Vector3.one;

            var main = particleSystem.main;
            main.loop = false;
            main.startLifetime = Mathf.Infinity;
            main.startSpeed = 0.1f;
            main.maxParticles = maxParticleNum;
            var emission = particleSystem.emission;
            emission.rateOverTime = 10000f;
        }


        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];

        //这里会实例化一个mesh
        diamondMesh = diamondRenderer.GetComponent<MeshFilter>().sharedMesh;
        vertices = diamondMesh.vertices;
        normals = diamondMesh.normals;

        camTr = Camera.main.transform;

        EmitParticlesOnDiamondSurface();
    }

    private void FixedUpdate()
    {
        EmitParticlesOnDiamondSurface();
    }

    void EmitParticlesOnDiamondSurface()
    {

        int numParticlesAlive = particleSystem.GetParticles(particles);
        //Debug.LogError("====numParticlesAlive:" + numParticlesAlive + " arrLength:" + particles.Length + " vertices:" + vertices.Length);
        int index = 0;
        int veticesLenth = vertices.Length;
        //由于要节省粒子，所以顶点数要多余粒子数，循环数根据顶点数来
        for (int i = 0; i < veticesLenth; i++)
        {
            int randomVertexIndex = useRandom ? Random.Range(0, vertices.Length) : i;
            if(index >= maxParticleNum)
            {
                continue;
            }

            Vector3 worldPosition = diamondRenderer.transform.TransformPoint(vertices[randomVertexIndex]);
            if (useDotNV)
            {
                var normal = normals[randomVertexIndex];
                Vector3 worldNormal = diamondRenderer.transform.TransformDirection(normal);
                var vertDir = camTr.position - worldPosition;
                var dotNV = Vector3.Dot(worldNormal, Vector3.Normalize(vertDir));
                //Debug.LogError("=======dotNV:" + dotNV);
                if (dotNV > dotThreshold)
                {
                    particles[index].position = worldPosition;
                    //particles[i].velocity = normals[randomVertexIndex] * particleSystem.main.startSpeed.constant;

                    index++;
                }
            }else
            {
                particles[i].position = worldPosition;
                //particles[i].velocity = normals[randomVertexIndex] * particleSystem.main.startSpeed.constant;

                index++;
            }
        }
        //隐藏多余粒子
        for (int i = index; i < maxParticleNum; i++)
        {
            particles[i].position = new Vector3(10000f, 0f, 0f);
            particles[i].velocity = new Vector3(0f, 0f, 0f);
            
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}
