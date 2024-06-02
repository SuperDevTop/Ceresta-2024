using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiTest : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("02_Game");
    }
}
