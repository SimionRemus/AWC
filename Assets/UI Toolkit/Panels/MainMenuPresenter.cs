using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject playPanel;
    [SerializeField] GameObject shopPanel;
   
    private void OnEnable()
    {
        VisualElement root=GetComponent<UIDocument>().rootVisualElement;
        
        //root.Q<Button>("PlayButton").clicked+=()=>gm.GetComponent<SCR_GameManager>().FindMatch();
        root.Q<Button>("PlayButton").clicked+=()=>{
            
            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);
            
            playPanel.SetActive(true);
            playPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
        };

        root.Q<Button>("Shop").clicked+=()=>{
            
            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);
            
            shopPanel.SetActive(true);
            shopPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
        };

        root.Q<Button>("SettingsButton").clicked+=()=>{

            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);
            
            settingsPanel.SetActive(true);
            settingsPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;        
        };

        root.Q<Button>("QuitButton").clicked+=()=> Application.Quit();
    }
}
