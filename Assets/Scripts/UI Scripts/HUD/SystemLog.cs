using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SystemLog : MonoBehaviour
{
    public static SystemLog Instance;

    [Header("UI References")]
    [SerializeField] private RectTransform contentRoot; // parent container (Vertical Layout Group)
    [SerializeField] private GameObject messagePrefab;  // prefab with TMP_Text
    [SerializeField] private int maxMessages = 10;
    [SerializeField] private float fadeDuration = 2f;

    private readonly Queue<GameObject> messages = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// Add a new system message to the log.
    /// </summary>
    public void PostMessage(string text, Color? color = null, float lifetime = 4f)
    {
        GameObject newMsg = Instantiate(messagePrefab, contentRoot);

        Color msgColor = color ?? Color.white;
        var logMsg = newMsg.GetComponent<LogMessage>();
        logMsg.Init(text, msgColor, lifetime, fadeDuration);

        messages.Enqueue(newMsg);

        if (messages.Count > maxMessages)
        {
            Destroy(messages.Dequeue());
        }
    }   
}
