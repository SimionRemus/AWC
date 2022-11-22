using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Linq;

public class CheckPassToJoinPresenter : MonoBehaviour
{
    [SerializeField] GameObject gameManager;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject gameHUD;
    private VisualElement root;
    public string password;
    private string inputPassword;
    private Label incorrectMessage;

    public bool isPasscorrect=false;

    private void OnEnable()
    {
        root=GetComponent<UIDocument>().rootVisualElement;
        incorrectMessage=root.Q<Label>("WrongPass");

        root.Q<Button>("Submit").clicked+=()=>{
                inputPassword=root.Q<TextField>("Password").text;
                if(password==inputPassword) //is equal to the lobby password
                {
                    root.style.display=DisplayStyle.None;
                    gameObject.SetActive(false);
                    
                    // gameManager.SetActive(true);
                    // gameManager.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;

                    incorrectMessage.text="Password is correct. Joining game...";
                    isPasscorrect=true;
                }
                else
                {
                    incorrectMessage.text="Incorrrect password entered. Try again!";
                    isPasscorrect=false;
                }
            };

        root.Q<Button>("Return").clicked+=()=>{
                root.style.display=DisplayStyle.None;
                gameObject.SetActive(false);
                
                // playPanel.SetActive(true);
                // playPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
            };

    }
}
