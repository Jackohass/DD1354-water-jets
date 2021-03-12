using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.VelocityOverLifetimeModule VOLM;
    public float linVolicty = 0;
    public float zOrb = 0;
    public float radial = 0;
    private ParticleSystem.MinMaxCurve psMMC;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        VOLM = ps.velocityOverLifetime;
        psMMC = new ParticleSystem.MinMaxCurve(linVolicty);
        psMMC.mode = ParticleSystemCurveMode.TwoConstants;
        psMMC.constantMin = -linVolicty;
        psMMC.constantMax = linVolicty;
        VOLM.x = psMMC;
        VOLM.y = psMMC;
    }

    // Update is called once per frame
    void Update()
    {
        VOLM.orbitalZ = zOrb;
        VOLM.radial = radial;
        psMMC.constantMin = -linVolicty;
        psMMC.constantMax = linVolicty;
        VOLM.x = psMMC;
        VOLM.y = psMMC;
    }
}
