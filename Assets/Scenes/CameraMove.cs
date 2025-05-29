using UnityEngine;
using System.Linq;

public class CameraMove : MonoBehaviour
{
    private GameObject bestCar;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero; // SmoothDamp�� ���� �߰�
    public GameObject[] cars; // ���� ����Ʈ�� �����ϴ� �迭
    void Start()
    {
        UpdateCarList(); // ���� ����Ʈ �ʱ�ȭ

        if (bestCar != null)
        {
            offset = transform.position - bestCar.transform.position; // �ʱ� ������ ���
        }
    }

    void Update()
    {
        UpdateCarList(); // �� �����Ӹ��� ���� ����Ʈ ������Ʈ
        bestCar = FindBestCar(); // ���� ���� ������ ���� ���� ã��

        if (bestCar != null)
        {
            Vector3 targetPosition = bestCar.transform.position + offset;
            targetPosition.z = -10; // ī�޶�� 2D ���ӿ��� �ڿ� �־�� �ϹǷ� z���� -10���� ����

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.3f);
        }
    }
    public void SetCarList(GameObject[] newCars)
    {
        cars = newCars;
    }
    void UpdateCarList()
    {
        GameObject[] activeCars = Object.FindObjectsByType<CarMove>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                                        .Select(car => car.gameObject)
                                        .ToArray();

        if (activeCars.Length > 0)
        {
            bestCar = FindBestCar(); // ���� ���� ������ ���� ���� ã��
        }
    }

    GameObject FindBestCar()
    {
        return Object.FindObjectsByType<CarMove>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                     .Where(car => car.gameObject != null)
                     .OrderByDescending(car => car.GetScore())
                     .Select(car => car.gameObject)
                     .FirstOrDefault();
    }
}