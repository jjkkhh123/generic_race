using UnityEngine;
using System.Linq;

public class CameraMove : MonoBehaviour
{
    private GameObject bestCar;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero; // SmoothDamp용 변수 추가
    public GameObject[] cars; // 차량 리스트를 저장하는 배열
    void Start()
    {
        UpdateCarList(); // 차량 리스트 초기화

        if (bestCar != null)
        {
            offset = transform.position - bestCar.transform.position; // 초기 오프셋 계산
        }
    }

    void Update()
    {
        UpdateCarList(); // 매 프레임마다 차량 리스트 업데이트
        bestCar = FindBestCar(); // 가장 높은 점수를 가진 차량 찾기

        if (bestCar != null)
        {
            Vector3 targetPosition = bestCar.transform.position + offset;
            targetPosition.z = -10; // 카메라는 2D 게임에서 뒤에 있어야 하므로 z값을 -10으로 고정

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
            bestCar = FindBestCar(); // 가장 높은 점수를 가진 차량 찾기
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