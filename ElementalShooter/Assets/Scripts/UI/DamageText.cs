using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Vector3 moveSpeed = new Vector3(0, 1.5f, 0); // Hướng bay lên
    [SerializeField] private AnimationCurve scaleCurve; // Curve chỉnh độ phình/nhỏ lại của chữ
    [SerializeField] private AnimationCurve alphaCurve; // Curve chỉnh độ mờ dần (Fade out)

    private float timer = 0f;
    private Color baseColor;
    private GameObject basePrefab;

    public void Setup(float damage, Color color, GameObject prefab)
    {
        this.basePrefab = prefab;
        textMesh.text = Mathf.RoundToInt(damage).ToString();
        this.baseColor = color;
        textMesh.color = color;
        timer = 0f;
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 1. Di chuyển chữ bay lên trên
        transform.position += moveSpeed * Time.deltaTime;

        // Tỷ lệ thời gian trôi qua từ 0 -> 1
        float normalizedTime = timer / lifetime;

        if (normalizedTime >= 1.0f)
        {
            Deactivate();
            return;
        }

        // 2. Cập nhật Kích thước (Scale) dựa trên Curve
        if (scaleCurve != null && scaleCurve.keys.Length > 0)
        {
            float scale = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        // 3. Cập nhật độ mờ (Alpha/Fade Out) dựa trên Curve
        if (alphaCurve != null && alphaCurve.keys.Length > 0)
        {
            float alpha = alphaCurve.Evaluate(normalizedTime);
            Color newColor = baseColor;
            newColor.a = alpha;
            textMesh.color = newColor;
        }
    }

    private void Deactivate()
    {
        if (gameObject.activeSelf)
        {
            PoolManager.Instance.ReturnToPool(basePrefab, gameObject);
        }
    }

    public void OnSpawn()
    {
        timer = 0f;
    }

    public void OnDespawn()
    {
        // Reset chữ về mặc định
        textMesh.text = "";
    }
}
