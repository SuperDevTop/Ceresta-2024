using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public string entryTeleporterTag = "TeleportEntry"; 
    public string exitTeleporterTag = "TeleportExit"; 
    private bool canTeleport = false;

    // Lists to store spawned entry and exit teleporters
    public List<GameObject> entryTeleporters = new List<GameObject>();
    public List<GameObject> exitTeleporters = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(WaitBeforeTeleport(other.transform));
            Debug.Log("teleport");
            canTeleport = true;
        }
    }

    private void TeleportPlayer(Transform playerTransform)
    {
        if (gameObject.CompareTag(entryTeleporterTag) && canTeleport)
        {
            Debug.Log("Entry Teleporter found: " + gameObject.name);

            int index = entryTeleporters.IndexOf(gameObject);

            if (index < exitTeleporters.Count && index >= 0)
            {
                GameObject exitTeleporter = exitTeleporters[index];
                Rigidbody playerRigidbody = playerTransform.GetComponent<Rigidbody>();

                if (exitTeleporter != null)
                {
                    if (playerRigidbody != null)
                    {
                        playerTransform.position = exitTeleporter.transform.position;
                    }
                    else
                    {
                        playerTransform.position = exitTeleporter.transform.position;
                        Debug.Log("Player teleported!");
                        canTeleport = false;
                    }
                }
                else
                {
                    Debug.LogError("Exit Teleporter not found!");
                }
            }
            else
            {
                Debug.LogError("Corresponding Exit Teleporter not found!");
            }
        }
    }

    private void StoreTeleporters()
    {
        GameObject[] entryTeleportersArray = GameObject.FindGameObjectsWithTag(entryTeleporterTag);
        entryTeleporters.AddRange(entryTeleportersArray);

        GameObject[] exitTeleportersArray = GameObject.FindGameObjectsWithTag(exitTeleporterTag);
        exitTeleporters.AddRange(exitTeleportersArray);
    }

    private IEnumerator WaitBeforeTeleport(Transform playerTransform)
    {
        yield return new WaitForSeconds(2);

        StoreTeleporters();
        TeleportPlayer(playerTransform.transform);
    }
}
