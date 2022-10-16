using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] GameObject gm;
    private void Awake()
    {
        VisualElement root=GetComponent<UIDocument>().rootVisualElement;
        if(gm!=null)
        {
            root.Q<Button>("PlayButton").clicked+=()=>gm.GetComponent<SCR_GameManager>().FindMatch();
            root.Q<Button>("SettingsButton").clicked+=()=> Debug.Log("Settings clicked");
            root.Q<Button>("QuitButton").clicked+=()=> Debug.Log("Quit clicked");
        }
        
    }
}
