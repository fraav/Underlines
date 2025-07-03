using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer = 1f;
    private Color textColor;

    public static void Create(Vector3 position, int amount, Color color)
    {
        GameObject damagePopup = new GameObject("DamagePopup");
        damagePopup.transform.position = position;
        DamagePopup popup = damagePopup.AddComponent<DamagePopup>();
        popup.Setup(amount, color);
    }

    private void Awake()
    {
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 4;
        textMesh.sortingOrder = 10;
    }

    public void Setup(int amount, Color color)
    {
        textMesh.text = amount.ToString();
        textMesh.color = color;
        textColor = color;
    }

    private void Update()
    {
        // Mover hacia arriba
        transform.position += Vector3.up * Time.deltaTime;

        // Efecto de desvanecimiento
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}