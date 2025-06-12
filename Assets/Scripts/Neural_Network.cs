using UnityEngine;
using System;

public class Neural_Network
{
    public float[,] inputToHiddenWeights;  // �Է��� �� ������ ����ġ
    public float[,] hiddenToOutputWeights; // ������ �� ����� ����ġ
    private int inputSize = 5;  // ù ��° ���̾� (���� �Է�)
    private int hiddenSize = 3; // �� ��° ���̾� (�߰� ������)
    private int outputSize = 2; // ����� (Handling, Speed)

    public Neural_Network()
    {
        inputToHiddenWeights = new float[inputSize, hiddenSize];
        hiddenToOutputWeights = new float[hiddenSize, outputSize];

        for (int i = 0; i < inputSize; i++)
        {
            for (int j = 0; j < hiddenSize; j++)
            {
                inputToHiddenWeights[i, j] = UnityEngine.Random.Range(-0.2f, 0.2f);
            }
        }

        for (int i = 0; i < hiddenSize; i++)
        {
            for (int j = 0; j < outputSize; j++)
            {
                hiddenToOutputWeights[i, j] = UnityEngine.Random.Range(-0.2f, 0.2f);
            }
        }
    }

    public float[] Predict(float[] sensorInputs)
    {
        float[] hiddenLayer = new float[hiddenSize];
        float[] outputs = new float[outputSize];

        // �Է��� �� ������
        for (int j = 0; j < hiddenSize; j++)
        {
            hiddenLayer[j] = 0;
            for (int i = 0; i < inputSize; i++)
            {
                hiddenLayer[j] += sensorInputs[i] * inputToHiddenWeights[i, j];
            }
            hiddenLayer[j] = (float)Math.Tanh(hiddenLayer[j]);
        }

        // ������ �� �����
        for (int j = 0; j < outputSize; j++)
        {
            outputs[j] = 0;
            for (int i = 0; i < hiddenSize; i++)
            {
                outputs[j] += hiddenLayer[i] * hiddenToOutputWeights[i, j];
            }
            outputs[j] = (float)Math.Tanh(outputs[j]);
        }

        // �ﰢ ���� ��� �߰� (�� �ΰ��ϰ� �����ϵ���)
        outputs[0] = Mathf.Clamp(outputs[0] * 70f, -40f, 40f); // ȸ�� ���� ����
        outputs[1] = Mathf.Max(outputs[1] * 5f, 1f); // �ӵ� ����

        return outputs;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < inputSize; i++)
        {
            for (int j = 0; j < hiddenSize; j++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < mutationRate)
                {
                    inputToHiddenWeights[i, j] += UnityEngine.Random.Range(-0.3f, 0.3f);
                }
            }
        }

        for (int i = 0; i < hiddenSize; i++)
        {
            for (int j = 0; j < outputSize; j++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < mutationRate)
                {
                    hiddenToOutputWeights[i, j] += UnityEngine.Random.Range(-0.3f, 0.3f);
                }
            }
        }
    }
}