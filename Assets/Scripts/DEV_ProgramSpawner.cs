using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DEV_ProgramSpawner : MonoBehaviour
{
    [SerializeField] private ProgramDatabase database;
    [SerializeField] private GameObject pickupPrefab;

    [SerializeField] private Vector3 startPos;
    [SerializeField] private float distanceBetweenPickups = 2f;
    [SerializeField] private float distanceBetweenSuites = 5f;

    void Start()
    {
        if(startPos == null || startPos == Vector3.zero)
        {
            startPos = this.gameObject.transform.position;
        }

        Dictionary<string, List<ProgramData>> list = new Dictionary<string, List<ProgramData>>();

        // Create Lists.
        foreach (ProgramData data in database.ProgramList)
        {
            string suiteName = data.ApplicationSuite.Name;
            if (list.ContainsKey(suiteName))
            {
                list[suiteName].Add(data);
            }
            else
            {
                list.Add(suiteName, new List<ProgramData> { data });
            }
        }

        // Fill World.
        Vector3 currentPos = startPos;
        foreach(string suite in list.Keys)
        {
            foreach (ProgramData data in list[suite])
            {
                GameObject program = Instantiate(pickupPrefab, currentPos, Quaternion.identity);
                program.transform.parent = this.gameObject.transform;
                ProgramPickup pickup = program.GetComponent<ProgramPickup>();
                pickup.Initialize(data);
                pickup.IsPermanent = true;

                currentPos.x += distanceBetweenPickups;
            }

            currentPos.x = startPos.x;
            currentPos.z += distanceBetweenSuites;
        }
    }
}
