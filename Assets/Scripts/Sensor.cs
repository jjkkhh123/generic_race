using UnityEngine;

/// ������ ���⿡�� ���� ����� ��ֹ������� �Ÿ��� �����ϴ� ���� Ŭ����.
public class Sensor : MonoBehaviour
{
    // �� ������ ������ ���̾� (����Ƽ �����Ϳ��� ���� ����).
    [SerializeField]
    private LayerMask LayerToSense;
    // ������ ũ�ν���� ǥ�� (����Ƽ �����Ϳ��� ���� ����).
    [SerializeField]
    private SpriteRenderer Cross;

    // �ִ� �� �ּ� ���� �Ÿ�
    public static float MAX_DIST = 6f; // �ִ� ���� �Ÿ�: 10 ����
    public static float MIN_DIST = 0.01f; // �ּ� ���� �Ÿ�: 0.01 ����

    /// ���� ���� ���� �� (�ִ� �Ÿ� ��� �����).
    public float Output
    {
        get;
        private set;
    }

    /// �ʱ�ȭ: ũ�ν��� Ȱ��ȭ�Ͽ� ǥ��.
    void Start()
    {
        Cross.gameObject.SetActive(true);
    }

    /// Unity�� ���� ������ Ȱ���Ͽ� ���� ���� ����.
    void FixedUpdate()
    {
        // ���� ���� ���
        Vector2 direction = Cross.transform.position - this.transform.position;
        direction.Normalize(); // ���� ���� ����ȭ

        // ���� �������� Raycast(����) �߻�
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, direction, MAX_DIST, LayerToSense);

        // ������ �Ÿ� Ȯ��
        if (hit.collider == null) // ��ֹ� ���� �� �� �� �ִ� �Ÿ� ����
            hit.distance = MAX_DIST;
        else if (hit.distance < MIN_DIST) // ���� �Ÿ� �ʹ� ª���� �� �ּ� �Ÿ� ����
            hit.distance = MIN_DIST;

        // ������ �Ÿ� �� ���� (����� ��ȯ)
        this.Output = hit.distance;
        // ������ ��ġ�� ũ�ν���� �̵� (�ð��� ǥ��)
        Cross.transform.position = (Vector2)this.transform.position + direction * hit.distance;
    }

    /// ũ�ν��� ����.
    public void Hide()
    {
        Cross.gameObject.SetActive(false);
    }

    /// ũ�ν��� ǥ��.
    public void Show()
    {
        Cross.gameObject.SetActive(true);
    }
}