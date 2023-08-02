using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public delegate void OnTogglePause();
    public static event OnTogglePause TogglePause;
    // Start is called before the first frame update

    void Awake(){
        TogglePause += togglePause;
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
    }
}
