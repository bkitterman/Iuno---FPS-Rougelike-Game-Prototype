using UnityEngine;

public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(this.transform.parent.gameObject);
        }
        else
        {
            Destroy(this.transform.parent.gameObject);
        }
    }
}