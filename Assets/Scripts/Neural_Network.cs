using UnityEngine;
using System; // System.Math를 사용하기 위해 추가

public class Neural_Network
{
    public float[,] weights;
    private int inputSize = 5;
    private int outputSize = 2;

    public Neural_Network()
    {
        weights = new float[inputSize, outputSize];
        for (int i = 0; i < inputSize; i++)
        {
            for (int j = 0; j < outputSize; j++)
            {
                weights[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
    }

    public float[] Predict(float[] sensorInputs)
    {
        float[] outputs = new float[outputSize];

        for (int j = 0; j < outputSize; j++)
        {
            outputs[j] = 0;

            for (int i = 0; i < inputSize; i++)
            {
                outputs[j] += sensorInputs[i] * weights[i, j];
            }

            // `System.Math.Tanh()` 사용
            outputs[j] = (float)Math.Tanh(outputs[j]);
        }

        outputs[0] *= 100f;
        outputs[1] = Mathf.Max(outputs[1] * 5f, 1f);

        return outputs;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < inputSize; i++)
        {
            for (int j = 0; j < outputSize; j++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < mutationRate)
                {
                    weights[i, j] += UnityEngine.Random.Range(-0.5f, 0.5f);
                }
            }
        }
    }
}