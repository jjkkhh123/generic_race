using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;



public class GeneticAlgorithm : MonoBehaviour
{
    public GameObject carPrefab;//GameObject�� ����Ƽ�� �⺻ Ŭ������ ������ ��� ������Ʈ�� ��Ÿ��
    //Prefab�� ����Ƽ���� �̸� ����� ���� ������Ʈ�� �����ϱ� ���� ���ø�
    public int populationSize = 16; //�� ����� ���� ��
    private List<GameObject> cars = new List<GameObject>(); //Prefab�� �̿�� ������Ʈ���� ����Ʈ ����
    private int generation = 0;//�ʱ� ���� 0

    public CameraMove cameraMove;

    private float waitTime = 2f;
    private float elapsedTime = 0f;
    private bool waiting = true;
    public static float[] bestHandlingHistory = new float[1000]; //

    void Start()
    {
        if (cameraMove == null)
        {
            cameraMove = Camera.main.GetComponent<CameraMove>();
        }

        if (cameraMove == null)
        {
            Debug.LogError("CameraMove component is not assigned or found on the main camera!");
        }

        Debug.Log("Initializing population...");
        InitializePopulation();
    }

    void Update()
    {
        if (waiting)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitTime)
            {
                waiting = false;
                Debug.Log("���� �ð� ����! ���� ������ üũ ����");
            }
            return;
        }

        if (AllCarsStopped())
        {
            Debug.Log("���� �˰��� ����!");
            List<GameObject> bestCars = FindBestCars(); // ���� 4�� ���� ã��
            CreateNextGeneration(bestCars); // ���ο� ���� ����
        }
    }

    bool AllCarsStopped()
    {
        foreach (GameObject car in cars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null && !carMove.isStopped)
            {
                return false; // �ϳ��� ������ �ʾҴٸ� ��� ����
            }
        }
        Debug.Log("��� ������ ����. ���� ���� ���� ����!");
        return true; // ��� ������ ����ٸ� true ��ȯ
    }

    void InitializePopulation()
    {
        cars.Clear(); // ���� ���� ����Ʈ �ʱ�ȭ

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(-6f, -6.1f, 0f);
            GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            CarMove newCarMove = car.GetComponent<CarMove>();

            if (newCarMove != null)
            {
                float[] individualHandlingHistory = GenerateHandlingHistory(); // ��ü���� �ڵ鸵 ������ ����
                newCarMove.SetHandlingHistory(individualHandlingHistory);
            }

            cars.Add(car);
        }

        if (cameraMove != null)
        {
            cameraMove.SetCarList(cars.ToArray()); // �ùٸ��� �޼��� ȣ��
        }
        else
        {
            Debug.LogError("CameraMove is not assigned, unable to initialize camera.");
        }
    }

    float[] GenerateHandlingHistory()
    {
        float[] handlingHistory = new float[1000];

        for (int i = 0; i < handlingHistory.Length; i++)
        {
            handlingHistory[i] = Random.Range(-1000f, 1000f); // ���� �ڵ鸵 ������ ����
        }

        Debug.Log("Generated Handling History: " + string.Join(", ", handlingHistory));
        return handlingHistory; // ���� ������ ����� ���� ������ ��ȯ
    }

    // ���� 4�� ������ ��ȯ�ϴ� �޼���� ����
    List<GameObject> FindBestCars()
    {
        // ������ ������ Ʃ�÷� ��� ����Ʈ ����
        List<(GameObject car, int score)> carScores = new List<(GameObject, int)>();

        foreach (GameObject car in cars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null)
            {
                carScores.Add((car, carMove.GetScore()));
            }
        }

        // ���� ���� �������� ���� �� ���� 4�� ����
        var topCars = carScores
            .OrderByDescending(cs => cs.score)
            .Take(4)
            .Select(cs => cs.car)
            .ToList();

        Debug.Log("���� 4�� ������ ����: " + string.Join(", ", topCars.Select(c => c.GetComponent<CarMove>().GetScore())));
        return topCars;
    }

    
    // bestCar -> bestCars(List<GameObject>)�� ����
    void CreateNextGeneration(List<GameObject> bestCars)
    {
        Debug.Log("���� ���� ���� ��...");
        foreach (GameObject car in cars)
        {
            if (car != null)
            {
                Destroy(car);
            }
        }

        cars.Clear();

        List<GameObject> newCars = new List<GameObject>();

        // ���� 4�� ������ �ڵ鸵 �����͸� ������� ���ο� ���� ����
        List<float[]> baseHandlingHistories = new List<float[]>();
        foreach (var car in bestCars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null)
            {
                float[] copy = new float[carMove.handlingHistory.Length];
                Array.Copy(carMove.handlingHistory, copy, copy.Length);
                baseHandlingHistories.Add(copy);
            }
        }

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(-6f, -6.1f, 0f);
            GameObject newCar = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            CarMove newCarMove = newCar.GetComponent<CarMove>();

            if (newCarMove != null)
            {
                // ���� 4�� �� �������� �ϳ� ����
                float[] parentHandling = baseHandlingHistories.Count > 0
                    ? baseHandlingHistories[Random.Range(0, baseHandlingHistories.Count)]
                    : GenerateHandlingHistory();

                float[] mutatedHandlingHistory = new float[parentHandling.Length];
                Array.Copy(parentHandling, mutatedHandlingHistory, mutatedHandlingHistory.Length);

                for (int j = 0; j < mutatedHandlingHistory.Length; j++)
                {
                    float mutationChance = Random.Range(0f, 1f);
                    if (mutationChance < 0.1f)
                    {
                        mutatedHandlingHistory[j] += Random.Range(-200f, 200f);
                    }
                }

                Debug.Log($"���� {i}�� ���̵� �ڵ鸵 ������: " + string.Join(", ", mutatedHandlingHistory));
                newCarMove.SetHandlingHistory(mutatedHandlingHistory);
            }

            newCars.Add(newCar);
        }

        cars = newCars;
    }



}