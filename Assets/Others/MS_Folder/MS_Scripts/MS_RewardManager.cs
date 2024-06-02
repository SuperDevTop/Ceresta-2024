using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MS_RewardManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void GotoMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
