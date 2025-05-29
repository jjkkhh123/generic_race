using UnityEngine;
using System;
public class CarMove : MonoBehaviour
{
    public bool isStopped = false;

    // 유전 요소
    public int handling = 100;  // 회전 강도 (예: 500~1000 사이)
    private float moveSpeed = 3f;  // 고정 이동 속도

    private int nextCheckpointID = 0;
    private int score = 0;
    private int checkp_score = 1000;
    public float[] handlingHistory = new float[1000];

    private int frameIndex = 0; // 현재 프레임 인덱스

    void Start()
    {
        Debug.Log("차가 움직이기 시작합니다!");
        /*
        if (GeneticAlgorithm.bestHandlingHistory != null && GeneticAlgorithm.bestHandlingHistory.Length > 0)
        {
            handlingHistory = new float[1000];
            Array.Copy(GeneticAlgorithm.bestHandlingHistory, handlingHistory, handlingHistory.Length);
        }
        */
    }


    void Update()
    {
        if(!isStopped)
        {
            // i번째 핸들링 값을 가져와 적용
            float rotation = handlingHistory[frameIndex] * Time.deltaTime;

            transform.Rotate(0, 0, -rotation);
            //Debug.Log(rotation);
            transform.position += transform.right * moveSpeed * Time.deltaTime;
            frameIndex = (frameIndex + 1) % handlingHistory.Length;

        }
        Checkpoint targetCheckpoint = FindCheckpointByID(nextCheckpointID);
        if (targetCheckpoint != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - targetCheckpoint.transform.position.x);
            float distanceY = Mathf.Abs(transform.position.y - targetCheckpoint.transform.position.y);
            score = checkp_score - Mathf.RoundToInt(distanceX * 10f) - Mathf.RoundToInt(distanceY * 10f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("wall"))
        {
            isStopped = true;
            Debug.Log(score);
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

    public void SetHandlingHistory(float[] history)
    {
        handlingHistory = new float[1000]; // 새로운 배열 생성
        Array.Copy(history, handlingHistory, handlingHistory.Length);
    }

    public int GetScore()
    {
        return score;
    }
}