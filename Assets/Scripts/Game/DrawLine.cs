using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class DrawLine : MonoBehaviour
{
    public Transform target1; // Reference to the first target
    public Transform target2; // Reference to the second target
    private LineRenderer lineRenderer;
    public List<Transform> targets = new List<Transform>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        targets.Add(target1);
        targets.Add(target2);
        lineRenderer.positionCount = 2; // We have 2 points: start and end of the line
    }

    void Update()
    {
        //lineRenderer.SetPosition(0, target1.position + new Vector3(0, 0, -1f)); // Start position
        //lineRenderer.SetPosition(1, target2.position + new Vector3(0, 0, -1f)); // End position

        // Update the positions of the line renderer
        lineRenderer.positionCount = targets.Count;

        for (int i = 0; i < targets.Count; i++)
        {
            lineRenderer.SetPosition(i, targets[i].position + new Vector3(0, 0, -1f));
        }
    }

    public void ShowPath()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for(int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PhotonView>().AmOwner)
            {
                if (!targets.Contains(players[i].transform))
                {
                    targets.Add(players[i].transform);
                }
                                
                break;
            }
        }
    }
}