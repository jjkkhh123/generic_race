using UnityEngine;
using System;

public class CarMove : MonoBehaviour
{
    public bool isStopped = false;
    public int handling = 100;
    private float moveSpeed = 3f;

    private int nextCheckpointID = 0;
    private int score = 0;
    private int checkp_score = 1000;
    private int frameIndex = 0;
    private float minSpeed = 1f; // 최소 속도 설정
    public Neural_Network neuralNet; // 신경망 추가
    public Sensor[] sensors; // 차량에 부착된 센서들

    void Start()
    {
        neuralNet = new Neural_Network(); // 신경망 인스턴스 생성

        // 센서 자동 검색 및 초기화
        sensors = GetComponentsInChildren<Sensor>();

        if (sensors == null || sensors.Length == 0)
        {
            Debug.LogError("센서가 올바르게 할당되지 않았습니다! 차량에 Sensor 컴포넌트가 있는지 확인하세요.");
        }
    }

    void Update()
    {
        if (!isStopped)
        {
            if (sensors == null || sensors.Length == 0)
            {
                Debug.LogError("센서 배열이 초기화되지 않았습니다! 차량이 센서를 포함하고 있는지 확인하세요.");
                return;
            }

            float[] sensorValues = new float[sensors.Length];

            for (int i = 0; i < sensors.Length; i++)
            {
                sensorValues[i] = sensors[i].Output; // 센서값 가져오기
            }

            float[] nnOutputs = neuralNet.Predict(sensorValues); // 신경망 출력 받기
            float handlingOutput = nnOutputs[0];
            float speedOutput = Mathf.Max(nnOutputs[1], minSpeed); // 최소 속도 적용

            float rotation = handlingOutput * Time.deltaTime;
            transform.Rotate(0, 0, -rotation);
            transform.position += transform.right * speedOutput * Time.deltaTime;

            // 점수 계산 로직 추가
            Checkpoint targetCheckpoint = FindCheckpointByID(nextCheckpointID);

            if (targetCheckpoint != null)
            {
                float distanceX = Mathf.Abs(transform.position.x - targetCheckpoint.transform.position.x);
                float distanceY = Mathf.Abs(transform.position.y - targetCheckpoint.transform.position.y);
                float totalDistance = distanceX + distanceY;

                // 거리 기반 점수 계산
                int distancePenalty = Mathf.RoundToInt(totalDistance * 10f);
                score = checkp_score - distancePenalty;

                // 너무 멀리 벗어나면 패널티 추가
                if (totalDistance > 5f)
                {
                    score -= 500;
                }

                // 체크포인트를 통과하면 추가 점수
                if (totalDistance < 0.5f)
                {
                    score += 1000;
                    nextCheckpointID++;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("wall"))
        {
            isStopped = true;
            Debug.Log($"차량 충돌! 점수: {score}");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint != null && checkpoint.checkpointID == nextCheckpointID)
        {
            Debug.Log($"Checkpoint {nextCheckpointID} 통과!");
            nextCheckpointID++;
            checkp_score += 1000;
        }
    }

    private Checkpoint FindCheckpointByID(int id)
    {
        foreach (Checkpoint checkpoint in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
        {
            if (checkpoint.checkpointID == id)
            {
                return checkpoint;
            }
        }
        return null;
    }

    public int GetScore()
    {
        return score;
    }
}