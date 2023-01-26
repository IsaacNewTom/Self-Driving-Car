using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{
    /* Three input nodes, 1 for each sensor */
    public Matrix<float> InputLayer = Matrix<float>.Build.Dense(1, 3);

    /* A list of matrices, so we could have multiple hidden layers */
    public List<Matrix<float>> HiddenLayers = new List<Matrix<float>>();
    
    /* Two output nodes, the acceleration and turning */
    public Matrix<float> OutputLayer = Matrix<float>.Build.Dense(1, 2);

    /* A list of matrices, so we could have a weight matrix for each later */
    public List<Matrix<float>> Weights = new List<Matrix<float>>();

    /* List of biases */
    public List<float> Biases = new List<float>();

    public float Fitness;

    private float Sigmoid(float x){
        return (1 / (1 + Mathf.Exp(-x)));
    }

    /* Generate random weight values between -1 and 1 */
    public void RandomizeWeights(){
        for (int i = 0; i < Weights.Count; i++){
            for (int x = 0; x < Weights[i].RowCount; x++){
                for (int y = 0; y < Weights[i].ColumnCount; y++){
                    Weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    /* Creates a neural network */
    public void InitNetwork(int HiddenLayerCount, int HiddenNeuronsCount){
        InputLayer.Clear();
        HiddenLayers.Clear();
        OutputLayer.Clear();
        Weights.Clear();
        Biases.Clear();

        for (int i = 0; i < HiddenLayerCount + 1; i++)
        {
            /* Create the hidden layer and add it to the hidden layers list */
            Matrix<float> Layer = Matrix<float>.Build.Dense(1, HiddenLayerCount);
            HiddenLayers.Add(Layer);

            /* Generate a random bias between -1 and 1 */
            Biases.Add(Random.Range(-1f, 1f));


            /* When we generate the weights between the input layer and the first hidden layer */
            if (i == 0){
                /* Dimensions are 3xHiddenNeuronsCount since we have 3 sensors */
                Matrix<float> InputToHidden1 = Matrix<float>.Build.Dense(3, HiddenNeuronsCount);
                Weights.Add(InputToHidden1);
            }
            
            /* The dimensions are like that since we have the same amount of hidden neurons inside each layer */
            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(HiddenNeuronsCount, HiddenNeuronsCount);
            Weights.Add(HiddenToHidden);
        }

        /* Create the output layer's weights */
        Matrix<float> OutputWeights = Matrix<float>.Build.Dense(HiddenNeuronsCount, 2);
        Weights.Add(OutputWeights);
        
        Biases.Add(Random.Range(-1f, 1f));

        /* Randomise the weight values */
        RandomizeWeights();
    }

    /* Runs the neural network */
    public (float, float) RunNetwork(float InputA, float InputB, float InputC){

        InputLayer[0, 0] = InputA;
        InputLayer[0, 1] = InputB;
        InputLayer[0, 2] = InputC;

        /* The Tanh activation function (since its return value is between -1 and 1) */
        InputLayer = InputLayer.PointwiseTanh();

        HiddenLayers[0] = ((InputLayer  * Weights[0]) + Biases[0]).PointwiseTanh();

        for (int i = 1; i< HiddenLayers.Count; i++){
            HiddenLayers[i] = ((HiddenLayers[i-1] * Weights[i]) + Biases[i]).PointwiseTanh();
        }

        OutputLayer = ((HiddenLayers[HiddenLayers.Count - 1] * Weights[Weights.Count - 1]) + Biases[Biases.Count - 1]).PointwiseTanh();

        /* return the outputs - the acceleration can't be negative so it's passed to a sigmoid */
        /* We cast the second value to a float, since Math.Tanh returns a double */
        return (Sigmoid(OutputLayer[0, 0]), (float)Math.Tanh(OutputLayer[0, 1]));
    }

    
    /* Creates the hidden layer of a given neural network (used for copying a network) */
    public void CreateHiddenLayers(int HiddenLayerCount, int HiddenNeuronsCount){
        InputLayer.Clear();
        HiddenLayers.Clear();
        OutputLayer.Clear();

        for (int i = 0; i < HiddenLayerCount + 1; i++)
        {
            Matrix<float> NewHiddenLayer = Matrix<float>.Build.Dense(1, HiddenNeuronsCount);
            HiddenLayers.Add(NewHiddenLayer);
        }
    }

    /* Create a copy of the network, so it wouldn't just be a value and a reference */
    public NeuralNetwork CopyAndInitNetwork(int HiddenLayerCount, int HiddenNeuronsCount){
        NeuralNetwork NewNetwork = new NeuralNetwork();

        
        List<Matrix<float>> NewWeights = new List<Matrix<float>>();

        for (int i = 0; i < this.Weights.Count; i++)
        {
            Matrix<float> CurrentWeight = Matrix<float>.Build.Dense(Weights[i].RowCount, Weights[i].ColumnCount);

            for (int x = 0; x < CurrentWeight.RowCount; x++)
            {
                for (int y = 0; y < CurrentWeight.ColumnCount; y++)
                {
                    CurrentWeight[x, y] = Weights[i][x, y];
                }
            }

            NewWeights.Add(CurrentWeight);
        }

        List<float> NewBiases = new List<float>();

        NewBiases.AddRange(Biases);

        NewNetwork.Weights = NewWeights;
        NewNetwork.Biases = NewBiases;

        NewNetwork.CreateHiddenLayers(HiddenLayerCount, HiddenNeuronsCount);

        return NewNetwork;
    }
}