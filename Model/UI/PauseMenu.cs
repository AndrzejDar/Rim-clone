using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool menuActive;
    private int prevSpeed;

    // Start is called before the first frame update
    void Start()
    {
        menuActive = false;
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //TO DO - close open windows and then dispaly pause menu
            pauseMenu.SetActive(!menuActive);
            prevSpeed = Clock.Speed;
            if (Clock.Speed != 0)
            {
                prevSpeed = Clock.Speed;
                Clock.SetSpeed(0); 
            }
            menuActive = !menuActive;
            if (menuActive == false) Clock.SetSpeed(prevSpeed);
            if (menuActive == true) { 
            //hide menu
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(!menuActive);
        menuActive = false;
        Clock.SetSpeed(prevSpeed);
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("TO DO loading Manin Menu");
    }
    
    
    public void Quit()
    {
        Debug.Log("Quiting application");
        Application.Quit();
    }
}
