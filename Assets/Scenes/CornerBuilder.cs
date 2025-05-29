#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CornerPerfectBuilder : MonoBehaviour
{
    public GameObject wallPrefab;
    public float innerRadius = 3f;
    public float trackWidth = 6f;
    public float angleSpan = 90f;

    [ContextMenu("🔧 Build Corner In Editor (Permanent)")]
    public void BuildCornerInEditor()
    {
        // 기존 생성된 벽 삭제 (중복 생성 방지)
        foreach (Transform child in transform)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(child.gameObject);
#else
            DestroyImmediate(child.gameObject);
#endif
        }

        // 안쪽과 바깥쪽 코너 모두 생성
        BuildCorner(innerRadius, "InCorner");
        BuildCorner(innerRadius + trackWidth, "OutCorner");
    }

    void BuildCorner(float radius, string parentName)
    {
        GameObject cornerParent = new GameObject(parentName);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(cornerParent, "Create Corner Parent");
#endif
        cornerParent.transform.parent = this.transform;

        float wallWidth = wallPrefab.transform.localScale.x;

        // 🎯 더 촘촘하게 생성 (틈 없도록 0.8배 간격 유지)
        float arcLength = 2 * Mathf.PI * radius * (angleSpan / 360f);
        int wallCount = Mathf.CeilToInt(arcLength / (wallWidth * 0.8f));

        float angleStep = angleSpan / wallCount;

        for (int i = 0; i <= wallCount; i++)
        {
            float angleDeg = i * angleStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float x = Mathf.Cos(angleRad) * radius;
            float y = Mathf.Sin(angleRad) * radius;
            Vector3 position = new Vector3(x, y, 0f);

            Quaternion rotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            GameObject wall = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
            wall.transform.position = position;
            wall.transform.rotation = rotation;
            wall.transform.SetParent(cornerParent.transform, true);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(wall, "Create Wall");
#endif
        }
    }
}