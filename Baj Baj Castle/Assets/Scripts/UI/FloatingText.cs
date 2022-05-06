using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private readonly float fadeSpeed = 1f;
    private float lifeTime;
    private float speed;
    private Color textColor;

    private TextMeshPro textMesh;

    public static void Create(string text, Color color, Vector3 position, float textSize, float lifeTime,
        float speed)
    {
        var floatingTextObject = Instantiate(GameAssets.Instance.FloatingTextObject, Vector3.zero, Quaternion.identity);
        var floatingText = floatingTextObject.GetComponent<FloatingText>();
        floatingText.Setup(text, color, position, textSize, lifeTime, speed);
    }

    [UsedImplicitly]
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    [UsedImplicitly]
    public void Update()
    {
        transform.position += new Vector3(0, speed) * Time.deltaTime;
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0) Destroy(gameObject);
        }
    }

    public void Setup(string text, Color color, Vector3 position, float textSize, float newLifeTime, float newSpeed)
    {
        textMesh.SetText(text);
        textMesh.fontSize = textSize;
        textMesh.color = color;
        textColor = color;
        transform.position = position;
        lifeTime = newLifeTime;
        speed = newSpeed;
    }
}