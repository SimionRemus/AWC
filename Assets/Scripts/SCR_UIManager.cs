using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Networking;

public class SCR_UIManager : MonoBehaviour
{
    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private Text IPAddress;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // startHostButton.onClick.AddListener(()=>
        // {
        //     if(NetworkManager.Singleton.StartHost())
        //     {
        //         Debug.Log("Host Started...");
        //     }
        //     else
        //     {
        //         Debug.Log("Host could not be started...");
        //     }
        // });
    
        // startClientButton.onClick.AddListener(()=>
        // {
        //     // NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address=IPAddress.text;

        //     if(NetworkManager.Singleton.StartClient())
        //     {
        //         Debug.Log("Client Started...");
        //     }
        //     else
        //     {
        //         Debug.Log("lient could not be started...");
        //     }
        // });
    }

    // Update is called once per frame
    void Update()
    {
        //playersInGameText.text=$"Players in game: {PlayerManager.Instance.PlayersInGame}";
    }
}
