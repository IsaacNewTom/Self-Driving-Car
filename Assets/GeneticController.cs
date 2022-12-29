using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class GeneticController : MonoBehaviour
{
    [Header("References")]
    public CarController Controller;

    [Header("Controls")]
    /* Number of networks in our population */
    public int InitialPopulationCount = 85;

    [Range(0.0f, 1.0f)]
    /* The chance that the network will be randomized */
    public float MutationRate = 0.055f;

    [Header("Crossover Controls")]
    /* how many from the best and worst networks would be chosen */
    public int BestAgentSelection = 8;
    public int WorstAgentSelection = 3;
    /* How many networks we want to crossover/breed */
    public int NumberToCrossover;

    /* holds the networks that had been selected */
    private List<int> GenePool = new List<int>();

    private int NaturallySelectedCount;

    /* The population is just an array of networks */
    private NeuralNetwork[] Population;

    [Header("Public View")]
    public int CurrentGeneration;
    public int CurrentGenome = 0;


    /* Reset the current car with a specific network */
    private void ResetToCurrentGenome(){
        Controller.ResetWithNetwork(Population[CurrentGenome]);
    }

    /* Randomize the networks in the population starting from the index to the end */
    private void RandomizePopulationValues(NeuralNetwork[] NewPopulation, int StartingIndex){
        while (StartingIndex < InitialPopulationCount)
        {   
            /* Iterate over the population from the starting index to the end and create a network */
            NewPopulation[StartingIndex] = new NeuralNetwork();
            NewPopulation[StartingIndex].InitNetwork(Controller.HIDDEN_LAYERS, Controller.NEURONS);
            StartingIndex++;
        }
    }

    /* Create the population */
    private void CreatePopulation(){
        /* Randomize the population */
        Population = new NeuralNetwork[InitialPopulationCount];
        RandomizePopulationValues(Population, 0);
        ResetToCurrentGenome();
    }

    public void Start(){
        CreatePopulation();
    }

    /* Called when a car dies */
    public void Death(float fitness, NeuralNetwork network){
        
        if (CurrentGeneration < Population.Length - 1){
            Population[CurrentGenome].Fitness = fitness;
            CurrentGenome++;
            ResetToCurrentGenome();
        }
        else{
            //Repopulate the population
        }
    }

}
