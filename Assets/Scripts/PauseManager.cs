using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public delegate void OnTogglePause();
    public static event OnTogglePause TogglePause;

    private GameObject pauseMenu;

    // Start is called before the first frame update

    void Awake(){
      
    }

    void OnEnable()
    {
      
        TogglePause += togglePause;
        pauseMenu = GameObject.FindWithTag("Pause");
        pauseMenu.SetActive(false);
       
    }
    void Start()
    {
        
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
        Debug.Log("Exit Called");
    }

    void OnDisable()
    {
        TogglePause -= togglePause;
    }
}
