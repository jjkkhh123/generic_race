using UnityEngine;

/// 지정된 방향에서 가장 가까운 장애물까지의 거리를 측정하는 센서 클래스.
public class Sensor : MonoBehaviour
{
    // 이 센서가 감지할 레이어 (유니티 에디터에서 설정 가능).
    [SerializeField]
    private LayerMask LayerToSense;
    // 센서의 크로스헤어 표시 (유니티 에디터에서 설정 가능).
    [SerializeField]
    private SpriteRenderer Cross;

    // 최대 및 최소 감지 거리
    public static float MAX_DIST = 6f; // 최대 감지 거리: 10 유닛
    public static float MIN_DIST = 0.01f; // 최소 감지 거리: 0.01 유닛

    /// 현재 센서 감지 값 (최대 거리 대비 백분율).
    public float Output
    {
        get;
        private set;
    }

    /// 초기화: 크로스헤어를 활성화하여 표시.
    void Start()
    {
        Cross.gameObject.SetActive(true);
    }

    /// Unity의 물리 엔진을 활용하여 센서 값을 갱신.
    void FixedUpdate()
    {
        // 센서 방향 계산
        Vector2 direction = Cross.transform.position - this.transform.position;
        direction.Normalize(); // 방향 벡터 정규화

        // 센서 방향으로 Raycast(광선) 발사
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, direction, MAX_DIST, LayerToSense);

        // 감지한 거리 확인
        if (hit.collider == null) // 장애물 감지 안 됨 → 최대 거리 설정
            hit.distance = MAX_DIST;
        else if (hit.distance < MIN_DIST) // 감지 거리 너무 짧으면 → 최소 거리 설정
            hit.distance = MIN_DIST;

        // 감지한 거리 값 저장 (백분율 변환)
        this.Output = hit.distance;
        // 감지한 위치로 크로스헤어 이동 (시각적 표시)
        Cross.transform.position = (Vector2)this.transform.position + direction * hit.distance;
    }

    /// 크로스헤어를 숨김.
    public void Hide()
    {
        Cross.gameObject.SetActive(false);
    }

    /// 크로스헤어를 표시.
    public void Show()
    {
        Cross.gameObject.SetActive(true);
    }
}