using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]


/* The script that controls the car */
public class CarController : MonoBehaviour
{
    /* The car's starting position and rotation */
    private Vector3 StartPosition, StartRotation, LastPosition, Input;
    private float TotalDistanceTraveled, AverageSpeed;

    private NeuralNetwork Network;

    /* Sensors that go: diagonally right, forward, and diagonally left */
    private float SensorA, SensorB, SensorC;

    [Range(-1f, 1f)]
    public float Acceleration, Turning;

    /* To check if the car hasn't moved in a long time, so we would have to reset it */
    public float TimeSinceStart = 0f;

    /* How well the car has done in this generation 
       this is calculated by the range it went and how fast it got there */
    [Header("Fitness")]
    public float OverallFitness;
    /* How important the distance travelled is */
    public float DistanceMultiplier = 1.4f;
    /* How important the speed is */
    public float SpeedMultiplier = 0.2f;
    /* How important it is to stay at the middle of the track */
    public float SensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int HIDDEN_LAYERS = 1;
    public int NEURONS = 10;

    public void Awake(){
        StartPosition = transform.position;
        StartRotation = transform.eulerAngles;
        Network = GetComponent<NeuralNetwork>();

        /* Testing */
        // Network.InitNetwork(HIDDEN_LAYERS, NEURONS);
    }

    /* Reset a car with a given network */
    public void ResetWithNetwork(NeuralNetwork network){
        Network = network;
        Reset();
    }

    /* Would be called whenever we want to reset the car */
    public void Reset(){

        Network.InitNetwork(HIDDEN_LAYERS, NEURONS);

        TimeSinceStart = 0f;
        TotalDistanceTraveled = 0f;
        AverageSpeed = 0f;
        LastPosition = StartPosition;
        OverallFitness = 0f;
        transform.position = StartPosition;
        transform.eulerAngles = StartRotation;
    }

    /* Would be called whenever the car hit something */
    private void OnCollisionEnter(Collision collision){
        Death();
    }

    /* Gain the sensor's input */
    private void InputSensors(){
        Vector3 DirectionA = (transform.forward + transform.right);
        Vector3 DirectionB = (transform.forward);
        Vector3 DirectionC = (transform.forward - transform.right);
        int NormalizationValue = 15;

        Ray r = new Ray(transform.position, DirectionA);
        RaycastHit hit;


        /* Fire the ray as sensor A */
        if (Physics.Raycast(r, out hit)){
            SensorA = hit.distance / NormalizationValue;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            print("Sensor A: " + SensorA);
        }

        r.direction = DirectionB;
        /* Fire the ray as sensor B */
        if (Physics.Raycast(r, out hit)){
            SensorB = hit.distance / NormalizationValue;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            print("Sensor B: " + SensorB);
        }

        r.direction = DirectionC;
        /* Fire the ray as sensor C */
        if (Physics.Raycast(r, out hit)){
            SensorC = hit.distance / NormalizationValue;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            print("Sensor C: " + SensorC);
        }
    }

    /* Calulate the car's fitness */
    private void CalculateFitness(){
        /* Sum up the distance every frame */
        TotalDistanceTraveled += Vector3.Distance(transform.position, LastPosition);
        AverageSpeed = TotalDistanceTraveled / TimeSinceStart;
        
        OverallFitness = (TotalDistanceTraveled * DistanceMultiplier) + (AverageSpeed * SpeedMultiplier) + ( ((SensorA + SensorB + SensorC) / 3) * SensorMultiplier);

        /* if it's been 20 seconds and the fitness is low, reset */
        if (TimeSinceStart > 20 && OverallFitness < 40){
            Death();
        }

        /* the car has finished the track */
        if (OverallFitness >= 1000){
            Death();
        }
    }

    /* Move the car */
    public void MoveCar(float VerticalMovement, float HorizontalMovement){
        /* Gradually accelerate and move angles */
        Input = Vector3.Lerp(Vector3.zero, new Vector3(0,0,VerticalMovement * 11.4f), 0.02f); 
        Input = transform.TransformDirection(Input);

        transform.position += Input;
        transform.eulerAngles += new Vector3(0, HorizontalMovement * 90 * 0.02f, 0);
    }

    /* Happens every frame */
    private void FixedUpdate(){
        InputSensors();
        LastPosition = transform.position;

        (Acceleration, Turning) = Network.RunNetwork(SensorA, SensorB, SensorC);

        MoveCar(Acceleration, Turning);
        TimeSinceStart += Time.deltaTime;

        CalculateFitness();
    }

    /* Call the genetic algorithm's death function */
    private void Death(){
        GameObject.FindObjectOfType<GeneticController>().Death(OverallFitness, Network);
    }
}