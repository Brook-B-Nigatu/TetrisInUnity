using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour, ISelectHandler
{
    private Button button;
    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnSelect(BaseEventData data)
    {
        PauseManager.playSound();
    }
}
