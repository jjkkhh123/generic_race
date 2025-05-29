using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;



public class GeneticAlgorithm : MonoBehaviour
{
    public GameObject carPrefab;//GameObject는 유니티의 기본 클래스로 게임의 모든 오브젝트를 나타냄
    //Prefab은 유니티에서 미리 만들어 놓은 오브젝트를 재사용하기 위한 템플릿
    public int populationSize = 16; //한 세대당 차량 수
    private List<GameObject> cars = new List<GameObject>(); //Prefab에 이용될 오브젝트들의 리스트 선언
    private int generation = 0;//초기 세대 0

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
                Debug.Log("지연 시간 종료! 차량 움직임 체크 시작");
            }
            return;
        }

        if (AllCarsStopped())
        {
            Debug.Log("유전 알고리즘 실행!");
            List<GameObject> bestCars = FindBestCars(); // 상위 4개 차량 찾기
            CreateNextGeneration(bestCars); // 새로운 세대 생성
        }
    }

    bool AllCarsStopped()
    {
        foreach (GameObject car in cars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null && !carMove.isStopped)
            {
                return false; // 하나라도 멈추지 않았다면 계속 진행
            }
        }
        Debug.Log("모든 차량이 멈춤. 다음 세대 생성 시작!");
        return true; // 모든 차량이 멈췄다면 true 반환
    }

    void InitializePopulation()
    {
        cars.Clear(); // 기존 차량 리스트 초기화

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(-6f, -6.1f, 0f);
            GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            CarMove newCarMove = car.GetComponent<CarMove>();

            if (newCarMove != null)
            {
                float[] individualHandlingHistory = GenerateHandlingHistory(); // 개체마다 핸들링 데이터 생성
                newCarMove.SetHandlingHistory(individualHandlingHistory);
            }

            cars.Add(car);
        }

        if (cameraMove != null)
        {
            cameraMove.SetCarList(cars.ToArray()); // 올바르게 메서드 호출
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
            handlingHistory[i] = Random.Range(-1000f, 1000f); // 랜덤 핸들링 데이터 생성
        }

        Debug.Log("Generated Handling History: " + string.Join(", ", handlingHistory));
        return handlingHistory; // 개별 차량에 적용될 유전 데이터 반환
    }

    // 상위 4개 차량을 반환하는 메서드로 변경
    List<GameObject> FindBestCars()
    {
        // 차량과 점수를 튜플로 묶어서 리스트 생성
        List<(GameObject car, int score)> carScores = new List<(GameObject, int)>();

        foreach (GameObject car in cars)
        {
            CarMove carMove = car.GetComponent<CarMove>();
            if (carMove != null)
            {
                carScores.Add((car, carMove.GetScore()));
            }
        }

        // 점수 기준 내림차순 정렬 후 상위 4개 선택
        var topCars = carScores
            .OrderByDescending(cs => cs.score)
            .Take(4)
            .Select(cs => cs.car)
            .ToList();

        Debug.Log("상위 4개 차량의 점수: " + string.Join(", ", topCars.Select(c => c.GetComponent<CarMove>().GetScore())));
        return topCars;
    }

    
    // bestCar -> bestCars(List<GameObject>)로 변경
    void CreateNextGeneration(List<GameObject> bestCars)
    {
        Debug.Log("다음 세대 생성 중...");
        foreach (GameObject car in cars)
        {
            if (car != null)
            {
                Destroy(car);
            }
        }

        cars.Clear();

        List<GameObject> newCars = new List<GameObject>();

        // 상위 4개 차량의 핸들링 데이터를 기반으로 새로운 세대 생성
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
                // 상위 4개 중 랜덤으로 하나 선택
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

                Debug.Log($"차량 {i}의 변이된 핸들링 데이터: " + string.Join(", ", mutatedHandlingHistory));
                newCarMove.SetHandlingHistory(mutatedHandlingHistory);
            }

            newCars.Add(newCar);
        }

        cars = newCars;
    }



}