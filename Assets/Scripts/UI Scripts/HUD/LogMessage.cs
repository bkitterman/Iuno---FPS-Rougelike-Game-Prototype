using UnityEngine;
using TMPro;

public class LogMessage : MonoBehaviour
{
    private TMP_Text text;
    private float lifetime;
    private float fadeDuration;
    private float timer;

    public void Init(string message, Color color, float lifetime, float fadeDuration)
    {
        text = GetComponent<TMP_Text>();
        text.text = message;
        text.color = color;

        this.lifetime = lifetime;
        this.fadeDuration = fadeDuration;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > lifetime)
        {
            float t = (timer - lifetime) / fadeDuration;
            Color c = text.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
