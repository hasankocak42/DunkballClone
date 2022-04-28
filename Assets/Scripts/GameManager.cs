using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;
    public static bool isGameOver;
    
    void Start()
    {
        isGameOver = false;
    }

    
    void Update()
    {
        if (isGameOver == true)
        {
            gameOverUI.SetActive(true);
        }
        if (isGameOver == true && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("dunkball");
        }
    }
}
