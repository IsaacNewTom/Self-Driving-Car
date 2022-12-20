using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
The script that controls the car!
*/


public class CarController : MonoBehaviour
{
    /* The car's starting position and rotation */
    private Vector3 StartPosition, StartRotation, LastPosition;
    private float TotalDistanceTraveled, AverageSpeed;
    private float SensorA, SensorB, SensorC;

    [Range(-1f, 1f)]
    public float acceleration, tuning;

    /* To check if the car hasn't moved in a long time, so we would have to reset it */
    public float TimeSinceStart = 0f;

    /* How well the car has done in this generation 
       this is calculated by the range it went and how fast it got there */
    [Header("Fitness")]
    public float OverallFitness;
    public float DistanceMultiplier = 1.4f;
    public float AverageSpeedMultiplier = 0.2f;

    

}
