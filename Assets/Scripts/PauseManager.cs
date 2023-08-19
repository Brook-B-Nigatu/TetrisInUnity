using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public delegate void OnTogglePause();
    public static event OnTogglePause TogglePause;

    private GameObject pauseMenu;
    
    public static AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public static void playSound()
    {
        audioSource.Play();
    }
    void OnEnable()
    {
      
        TogglePause += togglePause;
        pauseMenu = GameObject.FindWithTag("Pause");
        pauseMenu.SetActive(false);
       
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            TogglePause();
        }
    }

    void togglePause(){
        Time.timeScale = 1 - Time.timeScale;
        pauseMenu.SetActive(!pauseMenu.activeSelf);
    }
    
    public void resume()
    {
        TogglePause();
    }

    public void restart()
    {
        TogglePause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }

    public void exit()
    {
        Application.Quit();
    }

    void OnDisable()
    {
        TogglePause -= togglePause;
    }
}
