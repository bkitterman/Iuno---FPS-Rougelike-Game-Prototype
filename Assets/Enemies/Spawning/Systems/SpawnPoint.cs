using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // You could add properties later, like:
    // public bool canSpawnFlying = false;
    // public SpawnZone zone; // Group spawn points into zones

    // Optional gizmo for visualization in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
    }
}