using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class GeneticController : MonoBehaviour
{
    [Header("References")]
    public CarController carController;

    [Header("Controls")]
    /* Number of networks in our population */
    public int InitialPopulationCount = 85;

    [Range(0.0f, 1.0f)]
    /* The chance that the network will be randomized */
    public float MutationRate = 0.055f;

    [Header("Crossover Controls")]
    /* how many from the best and worst networks would be chosen */
    public int BestNetworksSelection = 8;
    public int WorstNetworksSelection = 3;
    /* How many networks we want to crossover */
    public int NumberToCrossover;

    /* holds the networks that had been selected to crossover */
    private List<int> GenePool = new List<int>();

    private int NaturallySelectedCount;

    /* The population is just an array of networks */
    private NeuralNetwork[] Population;

    /* Debugging */
    [Header("Public View")]
    public int CurrentGeneration;
    public int CurrentGenome = 0;


    /* Reset the current car with a specific network */
    private void ResetToCurrentGenome()
    {
        carController.ResetWithNetwork(Population[CurrentGenome]);
    }

    /* Randomize the networks in the population starting from the index to the end */
    private void RandomizePopulationValues(NeuralNetwork[] NewPopulation, int StartingIndex){
        GameObject myObject = new GameObject();

        for (int i = StartingIndex; i < InitialPopulationCount; i++)
        {   
            /* Iterate over the population from the starting index to the end and create a network */
            NewPopulation[i] = myObject.AddComponent<NeuralNetwork>();
            // NewPopulation[StartingIndex] = new NeuralNetwork();
            NewPopulation[i].InitNetwork(carController.HIDDEN_LAYERS, carController.NEURONS);
        }
    }

    /* Create the population */
    private void CreatePopulation(){
        Debug.Log("at create population");
        /* Create the population */
        Population = new NeuralNetwork[InitialPopulationCount];
        Debug.Log("created population array");
        /* Randomize the population */
        RandomizePopulationValues(Population, 0);
        /* Assign the first genome to the current car */
        ResetToCurrentGenome();
    }

    /* Sort the population by their fitness */
    private void SortPopulation(){
        for (int i = 0; i < Population.Length; i++){
            for (int j = 0; j < Population.Length; j++){
                if (Population[i].Fitness < Population[j].Fitness)
                {
                    NeuralNetwork temp = Population[i];
                    Population[i] = Population[j];
                    Population[j] = temp;
                }
            }
        }
    }

    /* pick some of the best and worst networks the population */
    private NeuralNetwork[] PickBestAndWorstPopulations(){ 
        NeuralNetwork[] NewPopulation = new NeuralNetwork[InitialPopulationCount];

        /* pick the top networks that had done good */
        for (int i = 0; i < BestNetworksSelection; i++){
            NewPopulation[NaturallySelectedCount] = Population[i].CopyAndInitNetwork(carController.HIDDEN_LAYERS, carController.NEURONS);
            NaturallySelectedCount++;

            /* How many times this current network to the genepool */
            int TimesToAdd = Mathf.RoundToInt(Population[i].Fitness * 10);

            for (int j = 0; j < TimesToAdd; j++){
                GenePool.Add(i);
            }
        }

        /* pick the worst networks */
        for (int i = 0; i < WorstNetworksSelection; i++)
        {
            int Last = Population.Length - 1;
            Last -= i;

            int TimesToAdd = Mathf.RoundToInt(Population[i].Fitness * 10);

            for (int j = 0; j < TimesToAdd; j++){
                GenePool.Add(i);
            }
        }

        return NewPopulation;
    }

    /* Crossover between the networks */
    private void Crossover(NeuralNetwork[] NewPopulation){
            /* Create children */
        NeuralNetwork ChildA = new NeuralNetwork();
        NeuralNetwork ChildB = new NeuralNetwork();

        for (int i = 0; i < NumberToCrossover; i += 2)
        {
            /* The first parent */
            int ParentA = i;
            /* The second parent */
            int ParentB = i + 1;

            /* Make sure the gene pool is not empty */
            if (GenePool.Count >= 1){
                for (int j = 0; j < 100; j++)
                {
                    /* A random network */
                    ParentA = GenePool[Random.Range(0, GenePool.Count)];
                    ParentB = GenePool[Random.Range(0, GenePool.Count)];

                    if (ParentA != ParentB)
                        break;
                }
            }


            ChildA.InitNetwork(carController.HIDDEN_LAYERS, carController.NEURONS);
            ChildB.InitNetwork(carController.HIDDEN_LAYERS, carController.NEURONS);

            ChildA.Fitness = 0;
            ChildB.Fitness = 0;

            /* Crossover the weights */
            for (int w = 0; w < ChildA.Weights.Count; w++)
            {
                /* A 50% chance of each child having its respective parent weights, or otherwise */
                if (Random.Range(0.0f, 1.0f) < 0.5f){
                    ChildA.Weights[w] = Population[ParentA].Weights[w];
                    ChildB.Weights[w] = Population[ParentB].Weights[w];
                }
                
                else
                {
                    ChildB.Weights[w] = Population[ParentA].Weights[w];
                    ChildA.Weights[w] = Population[ParentB].Weights[w];
                }
            }

            /* Crossover the biases */
            for (int b = 0; b < ChildA.Biases.Count; b++)
            {
                /* A 50% chance of each child having its respective parent biases, or otherwise */
                if (Random.Range(0.0f, 1.0f) < 0.5f){
                    ChildA.Biases[b] = Population[ParentA].Biases[b];
                    ChildB.Biases[b] = Population[ParentB].Biases[b];
                }
                
                else
                {
                    ChildB.Biases[b] = Population[ParentA].Biases[b];
                    ChildA.Biases[b] = Population[ParentB].Biases[b];
                }
            }

            NewPopulation[NaturallySelectedCount] = ChildA;
            NaturallySelectedCount++;
            NewPopulation[NaturallySelectedCount] = ChildB;
            NaturallySelectedCount++;
        }
    }

    /* Receives a weights matrix and mutates it */
    Matrix<float> MutateWeights(Matrix<float> WeightsMatrix){

        Matrix<float> NewMatrix = WeightsMatrix;
        /* Generate a random amount of weights to mutate  */
        int RandomAmountToMutate = Random.Range(1, (WeightsMatrix.RowCount * WeightsMatrix.ColumnCount) / 7);

        for (int i = 0; i < RandomAmountToMutate; i++){
            int RandomColumn = Random.Range(0, NewMatrix.ColumnCount);
            int RandomRow = Random.Range(0, NewMatrix.RowCount);

            /* The value should be between between -1 and 1, so we clamp the value */
             NewMatrix[RandomRow, RandomColumn] = Mathf.Clamp(NewMatrix[RandomRow, RandomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return NewMatrix;
    }

    /* Mutate after crossover (completely change the weights) */
    private void Mutate(NeuralNetwork[] NewPopulation){

        for (int i = 0; i < NaturallySelectedCount; i++)
        {
            for (int j = 0; j < NewPopulation[i].Weights.Count; j++)
            {
                
                if (Random.Range(0.0f, 1.0f) < MutationRate){
                    NewPopulation[i].Weights[j] = MutateWeights(NewPopulation[i].Weights[j]);
                }
            }
        }
    }

    private void RePopulate(){
        GenePool.Clear();
        CurrentGeneration++;
        NaturallySelectedCount = 0;
        SortPopulation();

        /* Sort the population by the fitness */
        NeuralNetwork[] NewPopulation = PickBestAndWorstPopulations();
        Crossover(NewPopulation);
        Mutate(NewPopulation);

        RandomizePopulationValues(NewPopulation, NaturallySelectedCount);

        Population = NewPopulation;
        CurrentGenome = 0;
        ResetToCurrentGenome();
    }

    public void Start(){
        Debug.Log("starting, creating population ");
        CreatePopulation();
    }

    /* Called when a car dies */
    public void Death(float fitness, NeuralNetwork network){
        
        if (CurrentGenome < Population.Length - 1){
            Population[CurrentGenome].Fitness = fitness;
            CurrentGenome++;
            ResetToCurrentGenome();
        }
        else{
            RePopulate();
        }
    }

}
