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
    private float minSpeed = 1f; // �ּ� �ӵ� ����
    public Neural_Network neuralNet; // �Ű�� �߰�
    public Sensor[] sensors; // ������ ������ ������

    void Start()
    {
        neuralNet = new Neural_Network(); // �Ű�� �ν��Ͻ� ����

        // ���� �ڵ� �˻� �� �ʱ�ȭ
        sensors = GetComponentsInChildren<Sensor>();

        if (sensors == null || sensors.Length == 0)
        {
            Debug.LogError("������ �ùٸ��� �Ҵ���� �ʾҽ��ϴ�! ������ Sensor ������Ʈ�� �ִ��� Ȯ���ϼ���.");
        }
    }

    void Update()
    {
        if (!isStopped)
        {
            if (sensors == null || sensors.Length == 0)
            {
                Debug.LogError("���� �迭�� �ʱ�ȭ���� �ʾҽ��ϴ�! ������ ������ �����ϰ� �ִ��� Ȯ���ϼ���.");
                return;
            }

            float[] sensorValues = new float[sensors.Length];

            for (int i = 0; i < sensors.Length; i++)
            {
                sensorValues[i] = sensors[i].Output; // ������ ��������
            }

            float[] nnOutputs = neuralNet.Predict(sensorValues); // �Ű�� ��� �ޱ�
            float handlingOutput = nnOutputs[0];
            float speedOutput = Mathf.Max(nnOutputs[1], minSpeed); // �ּ� �ӵ� ����

            float rotation = handlingOutput * Time.deltaTime;
            transform.Rotate(0, 0, -rotation);
            transform.position += transform.right * speedOutput * Time.deltaTime;

            // ���� ��� ���� �߰�
            Checkpoint targetCheckpoint = FindCheckpointByID(nextCheckpointID);

            if (targetCheckpoint != null)
            {
                float distanceX = Mathf.Abs(transform.position.x - targetCheckpoint.transform.position.x);
                float distanceY = Mathf.Abs(transform.position.y - targetCheckpoint.transform.position.y);
                float totalDistance = distanceX + distanceY;

                // �Ÿ� ��� ���� ���
                int distancePenalty = Mathf.RoundToInt(totalDistance * 10f);
                score = checkp_score - distancePenalty;

                // �ʹ� �ָ� ����� �г�Ƽ �߰�
                if (totalDistance > 5f)
                {
                    score -= 500;
                }

                // üũ����Ʈ�� ����ϸ� �߰� ����
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
            Debug.Log($"���� �浹! ����: {score}");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint != null && checkpoint.checkpointID == nextCheckpointID)
        {
            Debug.Log($"Checkpoint {nextCheckpointID} ���!");
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