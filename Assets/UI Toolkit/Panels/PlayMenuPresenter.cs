using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Unity.Services.Lobbies.Models;

public class PlayMenuPresenter : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject gameManager;

    [SerializeField] GameObject gameHUD;
    [SerializeField] GameObject createGamePanel;

    [SerializeField] GameObject checkPassToJoin;

    [SerializeField] List<LobbyEntry> lobbyListStruct;
    [SerializeField] GameObject lobbyVisualElement;

    private string joinCode;
    private string listViewJoinCode;

    private List<Lobby> listOfLobbies;

    private async void OnEnable()
    {
        VisualElement root=GetComponent<UIDocument>().rootVisualElement;
        LobbyListToStructList();
        #region buttons

        root.Q<Button>("BackButton").clicked+=()=>{
            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);
            
            
            mainPanel.SetActive(true);
            mainPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
        };

        root.Q<Button>("Refresh").clicked+=async ()=>{
            var _= await RebuildList(root);
        };

        root.Q<Button>("QuickPlay").clicked+= async ()=>{
                joinCode = await gameManager.GetComponent<SCR_GameManager>().FindMatch(null);
                bool __ = await RebuildList(root);

                root.style.display = DisplayStyle.None;
                gameObject.SetActive(false);

                gameHUD.SetActive(true);
                gameHUD.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
                gameHUD.GetComponent<UIDocument>().rootVisualElement.Q<Label>("LobbyID").text = "Game ID: " + joinCode;
        };

        root.Q<Button>("CreateGame").clicked+=()=>{
            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);

            createGamePanel.SetActive(true);
            createGamePanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;

        };

        root.Q<Button>("JoinCode").clicked+=async ()=>{
            joinCode=root.Q<TextField>("CodeString").text;
            var _=gameManager.GetComponent<SCR_GameManager>().FindMatch(joinCode);
            bool __=await RebuildList(root);

            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);

            gameHUD.SetActive(true);
            gameHUD.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
            gameHUD.GetComponent<UIDocument>().rootVisualElement.Q<Label>("LobbyID").text="Game ID: "+joinCode;
        };

        root.Q<Button>("JoinSelected").clicked+=async ()=>{
            //Check for password!!!
            joinCode=listViewJoinCode;
            var _=gameManager.GetComponent<SCR_GameManager>().FindMatch(listViewJoinCode);
            bool __=await RebuildList(root);

            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);

            gameHUD.SetActive(true);
            gameHUD.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
            gameHUD.GetComponent<UIDocument>().rootVisualElement.Q<Label>("LobbyID").text="Game ID: "+joinCode;
        };


        #endregion

        #region lobby list
        bool _=await RebuildList(root);
        #endregion
    }

    private async void Start()
    {
        VisualElement root=GetComponent<UIDocument>().rootVisualElement;
        bool _=await RebuildList(root);
    }

    private void onItemsChosen(IEnumerable<object> objects)
    {
        foreach(LobbyEntry entry in objects)
        {
            
            listViewJoinCode=entry._joinCode;
            Debug.Log(listViewJoinCode);
        }
    }

    private async void LobbyListToStructList()
    {
        List<Lobby> lobbyList=await gameManager.GetComponent<SCR_GameManager>().GetLobbiesList();
        lobbyListStruct.Clear();
        foreach(Lobby item in lobbyList)
        {
            lobbyListStruct.Add(new LobbyEntry
            {
                username=item.Name,
                playercount=item.Players.Count.ToString()+"/"+item.MaxPlayers.ToString(),
                gametype=(GameType)Enum.Parse(typeof(GameType),item.Data["GameType"].Value),//GameType.FFA,
                winCondition=(WinningCondition)Enum.Parse(typeof(WinningCondition),item.Data["WinningCondition"].Value),
                winConditionCount=Int32.Parse(item.Data["WinningConditionValue"].Value),
                mapName=item.Data["MapName"].Value,
                isPassProtected=!String.IsNullOrEmpty(item.Data["Password"].Value),
                _joinCode=item.Id
            });
        }
    }

    public async Task<bool> RebuildList(VisualElement root)
    {
        ListView lobbylist=root.Q<ListView>("LobbyList");
        listOfLobbies=await gameManager.GetComponent<SCR_GameManager>().GetLobbiesList();

        LobbyListToStructList();

        lobbylist.itemsSource=lobbyListStruct;
        lobbylist.makeItem=()=>{
            return lobbyVisualElement.GetComponent<UIDocument>().visualTreeAsset.CloneTree();
        };
        lobbylist.bindItem=(VisualElement root, int i)=>{
            var entry = lobbyListStruct[i];

            Label creator = root.Q<Label>("creator");
            creator.text = entry.username;

            Label numberOfPlayers = root.Q<Label>("numberOfPlayers");
            numberOfPlayers.text = entry.playercount;

            Label gameType = root.Q<Label>("gameType");
            gameType.text = entry.gametype.ToString();

            Label winCondition = root.Q<Label>("winCondition");
            winCondition.text = entry.winCondition.ToString();

            Label winConditionValue = root.Q<Label>("winConditionValue");
            winConditionValue.text = entry.winConditionCount.ToString();

            Label map = root.Q<Label>("map");
            map.text = entry.mapName;

            Label password = root.Q<Label>("password");
            password.text = entry.isPassProtected?"Yes":"No";
        };

        lobbylist.onSelectionChange+=onItemsChosen;
        lobbylist.onItemsChosen+=onItemsChosen;
        lobbylist.Rebuild();
        return true;
    }

    private bool CheckPassword()
    {
        return checkPassToJoin.GetComponent<CheckPassToJoinPresenter>().isPasscorrect;
    }

}


