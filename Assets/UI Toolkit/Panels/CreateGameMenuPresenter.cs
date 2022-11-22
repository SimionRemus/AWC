using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Linq;

public class CreateGameMenuPresenter : MonoBehaviour
{
    [SerializeField] GameObject gameHUD;
    [SerializeField] GameObject playPanel;
    [SerializeField] GameObject gameManager;

    private string lobbyName;
    private int playercount;
    private GameType gameType;
    private WinningCondition winningCondition;
    private int winningConditionCount;
    private string mapName;
    private bool hasPassword;
    private string password;

    private VisualElement root;

    private void OnEnable()
    {
        root=GetComponent<UIDocument>().rootVisualElement;

        root.Q<DropdownField>("MapName").choices=gameManager.GetComponent<SCR_GameManager>().Maps;
        root.Q<DropdownField>("MapName").index=0;
        root.Q<DropdownField>("GameType").choices=Enum.GetNames(typeof(GameType)).ToList<string>();
        root.Q<DropdownField>("GameType").index=0;
        root.Q<DropdownField>("WinCondition").choices=Enum.GetNames(typeof(WinningCondition)).ToList<string>();
        root.Q<DropdownField>("WinCondition").index=0;


        root.Q<Button>("StartGame").clicked+=async ()=>{
                root.style.display=DisplayStyle.None;
                gameObject.SetActive(false);

                SetGameParameters();
                Debug.Log(lobbyName);
                Debug.Log(playercount);
                Debug.Log(gameType);
                Debug.Log(winningCondition);
                Debug.Log(winningConditionCount);
                Debug.Log(mapName);
                Debug.Log(hasPassword);
                var joinCode= await gameManager.GetComponent<SCR_GameManager>().CreateMatch(
                    lobbyName,
                    playercount,
                    gameType,
                    winningCondition,
                    winningConditionCount,
                    mapName,
                    password
                );

                bool _= await playPanel.GetComponent<PlayMenuPresenter>().RebuildList(playPanel.GetComponent<UIDocument>().rootVisualElement);
                
                gameHUD.SetActive(true);
                gameHUD.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
                gameHUD.GetComponent<UIDocument>().rootVisualElement.Q<Label>("LobbyID").text="Game ID: "+joinCode;
            };

        root.Q<Button>("Return").clicked+=()=>{
                root.style.display=DisplayStyle.None;
                gameObject.SetActive(false);
                
                playPanel.SetActive(true);
                playPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
            };
    }

    private void SetGameParameters()
    {
        lobbyName=root.Q<TextField>("GameName").text;
        playercount=root.Q<SliderInt>("PlayCount").value;
        gameType= (GameType)Enum.Parse(typeof(GameType),root.Q<DropdownField>("GameType").value);
        winningCondition=(WinningCondition) Enum.Parse(typeof(WinningCondition),root.Q<DropdownField>("WinCondition").value);
        winningConditionCount=root.Q<SliderInt>("WinConditionValue").value;
        mapName=root.Q<DropdownField>("MapName").value;
        password=root.Q<TextField>("Password").text;
        hasPassword=!String.IsNullOrEmpty(password);
        
    }
}
