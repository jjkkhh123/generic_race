using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.UI;

public class GeneticAlgorithm : MonoBehaviour
{
    public GameObject carPrefab;
    public int populationSize = 16;
    private List<GameObject> cars = new List<GameObject>();
    private int generation = 1;
    public Text generationText;
    public CameraMove cameraMove;

    private float waitTime = 2f;
    private float elapsedTime = 0f;
    private bool waiting = true;

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
        UpdateUI();
    }

    void Update()
    {
        if (waiting)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitTime)
            {
                waiting = false;
                Debug.Log("지연 시간 종료! 차량 움직임 체크 시작");
            }
            return;
        }

        if (AllCarsStopped())
        {
            Debug.Log("유전 알고리즘 실행!");
            List<GameObject> bestCars = FindBestCars();
            CreateNextGeneration(bestCars);
        }
    }

    void UpdateUI()
    {
        if (generationText != null)
        {
            generationText.text = $"Generation: {generation}";
        }
    }

    bool AllCarsStopped()
    {
        foreach (GameObject car in cars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null && !carMove.isStopped)
            {
                return false;
            }
        }
        Debug.Log("모든 차량이 멈춤. 다음 세대 생성 시작!");
        return true;
    }

    void InitializePopulation()
    {
        cars.Clear();

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(-6f, -6.1f, 0f);
            GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            CarMove newCarMove = car.GetComponent<CarMove>();

            cars.Add(car);
        }

        if (cameraMove != null)
        {
            cameraMove.SetCarList(cars.ToArray());
        }
        else
        {
            Debug.LogError("CameraMove is not assigned, unable to initialize camera.");
        }
    }

    List<GameObject> FindBestCars()
    {
        List<(GameObject car, int score)> carScores = new List<(GameObject, int)>();

        foreach (GameObject car in cars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null)
            {
                carScores.Add((car, carMove.GetScore()));
            }
        }

        var topCars = carScores
            .OrderByDescending(cs => cs.score)
            .Take(4)
            .Select(cs => cs.car)
            .ToList();

        Debug.Log("상위 4개 차량의 점수: " + string.Join(", ", topCars.Select(c => c.GetComponent<CarMove>().GetScore())));
        return topCars;
    }

    void CreateNextGeneration(List<GameObject> bestCars)
    {
        generation++;
        Debug.Log("다음 세대 생성 중...");
        UpdateUI();

        foreach (GameObject car in cars)
        {
            if (car != null)
            {
                Destroy(car);
            }
        }
        cars.Clear();

        List<GameObject> newCars = new List<GameObject>();

        List<Neural_Network> baseNeuralNetworks = new List<Neural_Network>();
        foreach (var car in bestCars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null)
            {
                Neural_Network copiedNet = new Neural_Network();

                // 올바르게 가중치를 복사
                Array.Copy(carMove.neuralNet.inputToHiddenWeights, copiedNet.inputToHiddenWeights, carMove.neuralNet.inputToHiddenWeights.Length);
                Array.Copy(carMove.neuralNet.hiddenToOutputWeights, copiedNet.hiddenToOutputWeights, carMove.neuralNet.hiddenToOutputWeights.Length);

                baseNeuralNetworks.Add(copiedNet);
            }
        }

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(-6f, -6.1f, 0f);
            GameObject newCar = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            CarMove newCarMove = newCar.GetComponent<CarMove>();

            if (newCarMove != null)
            {
                Neural_Network parentNet = baseNeuralNetworks.Count > 0
                    ? baseNeuralNetworks[Random.Range(0, baseNeuralNetworks.Count)]
                    : new Neural_Network();

                // 변이를 강화하여 세대가 지나면서 학습이 이루어지도록 함
                parentNet.Mutate(0.2f);
                newCarMove.neuralNet = parentNet;
            }
            newCars.Add(newCar);
        }

        cars = newCars;
    }
}