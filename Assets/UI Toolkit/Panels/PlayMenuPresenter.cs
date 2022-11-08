using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayMenuPresenter : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject gameManager;
    [SerializeField] List<LobbyEntry> lobbyListSO;
    [SerializeField] GameObject lobbyVisualElement;

    private void OnEnable()
    {
        VisualElement root=GetComponent<UIDocument>().rootVisualElement;

        root.Q<Button>("QuickPlay").clicked+=()=>{
            gameManager.GetComponent<SCR_GameManager>().FindMatch();

            root.style.display=DisplayStyle.None;
            gameObject.SetActive(false);
        };

        root.Q<Button>("BackButton").clicked+=()=>{
                root.style.display=DisplayStyle.None;
                gameObject.SetActive(false);
                
                mainPanel.SetActive(true);
                mainPanel.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
            };


        ListView lobbylist=root.Q<ListView>("LobbyList");
        

        lobbylist.itemsSource=lobbyListSO;
        lobbylist.makeItem=()=>{
            return lobbyVisualElement.GetComponent<UIDocument>().visualTreeAsset.CloneTree();
        };
        lobbylist.bindItem=(VisualElement root, int i)=>{
            var entry = lobbyListSO[i];

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
        lobbylist.Rebuild();
    }
}
