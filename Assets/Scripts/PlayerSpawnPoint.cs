using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    void Start()
    {
        if (PersistentPlayer.Instance != null)
        {
            CharacterController cc = PersistentPlayer.Instance.GetComponent<CharacterController>();

            if (cc != null)
            {
                // 1. Disable the CharacterController
                cc.enabled = false;

                // 2. Teleport the player's transform to this object's position/rotation
                PersistentPlayer.Instance.transform.SetPositionAndRotation(transform.position, transform.rotation);

                // 3. Re-enable the CharacterController
                cc.enabled = true;
            }
            else
            {
                Debug.LogError("PlayerSpawnPoint: Could not find CharacterController on PersistentPlayer!");
            }
        }
        else
        {
            Debug.LogWarning("PlayerSpawnPoint: PersistentPlayer.Instance was not found. (This is normal if testing the scene directly).");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
    }
}
