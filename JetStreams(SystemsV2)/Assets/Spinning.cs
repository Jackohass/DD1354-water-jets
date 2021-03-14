using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinning : MonoBehaviour
{
    //The amount of degrees it speeds each secound
    public float xRotationalSpeed = 50;
    public float yRotationalSpeed = 50;
    public float zRotationalSpeed = 50;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(xRotationalSpeed * Time.deltaTime, yRotationalSpeed * Time.deltaTime, zRotationalSpeed * Time.deltaTime); //rotates 50 degrees per second around z axis
    }
}
