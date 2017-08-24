using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{   
    public void OnePlayer()
    {
        PlayerPrefs.SetInt("players", 1);
        SceneManager.LoadScene("play");
    }

    public void TwoPlayers()
    {
        PlayerPrefs.SetInt("players", 2);
        SceneManager.LoadScene("play");
    }
}
