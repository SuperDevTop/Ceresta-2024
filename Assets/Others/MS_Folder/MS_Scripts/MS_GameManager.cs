using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MS_GameManager : MonoBehaviour
{
    public static MS_GameManager instance;

    public bool keyCollected = false;

    [Space(10)]
    public GameObject player;
    public GameObject arrowObject;
    public GameObject gridMap;

    [Space(10)]
    public GameObject firePrefab;
    public GameObject keyPrefab;
    public GameObject finishLine;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

    }
}
