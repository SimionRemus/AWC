using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

using Unity.Services.Relay;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using UnityEngine.UIElements;

public class SCR_GameManager : MonoBehaviour
{
    public List<string> Maps=new List<string>{
        "Dark Forest",
        "Ominous Realm"
    };
    private string _lobbyId;
    public string inputPassword;
    private RelayHostData _hostData;
    private RelayJoinData _joinData;
    private bool isCorrect;
    private IEnumerator myCoroutine;

    [SerializeField] GameObject checkPassUI;

    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        SetupEvents();

        await SignInAnonymouslyAsync();

        //lobbyEntryList=new List<LobbyEntry>();
    }

    #region UnityLogin
    //Region where we login to unity. We can use data from here to set up lobby and relay

    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn+=()=>{
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
        };

        AuthenticationService.Instance.SignInFailed+=(err)=>{
            Debug.Log(err);
        };

        AuthenticationService.Instance.SignedOut+=()=>{
            Debug.Log("Player signed out.");
        };
    }

    async Task SignInAnonymouslyAsync()
    {

        #if UNITY_EDITOR
                if (ParrelSync.ClonesManager.IsClone())
                {
                    // When using a ParrelSync clone, we'll automatically switch to a different authentication profile.
                    // This will cause the clone to sign in as a different anonymous user account.  If you're going to use
                    // authentication profiles for some other purpose, you may need to change the profile name.
                    string customArgument = ParrelSync.ClonesManager.GetArgument();
                    AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
                }
        #endif
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded.");
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }
    }

    #endregion

    #region Lobby

    public async Task<string> FindMatch(string enteredLobbyId)
    {
        if(myCoroutine!=null)
            StopCoroutine(myCoroutine);
        Debug.Log("Looking for a lobby...");
        try
        {
            Lobby lobby;
            QuickJoinLobbyOptions options=new QuickJoinLobbyOptions();
                options.Filter=new List<QueryFilter>()
                {
                    new QueryFilter(
                        QueryFilter.FieldOptions.S4,
                        "",
                        QueryFilter.OpOptions.EQ
                    )
                };
            if(enteredLobbyId==null)
            {
                //We can add filters here
                
                //Quick-join a random lobby that matches options above
                lobby=await Lobbies.Instance.QuickJoinLobbyAsync(options);
                _lobbyId=lobby.Id;
            }
            else
            {
                _lobbyId=enteredLobbyId;
                lobby=await Lobbies.Instance.JoinLobbyByIdAsync(_lobbyId);
            }
            
            //Check here if lobby can be entered
            if(lobby.Data["Password"].Value=="")//There is no password on the lobby
            {
                return await JoinCurrentlySelectedLobby(lobby);
            }
            else //check the password
            {
                myCoroutine=IsPasswordValid(lobby, lobby.Data["Password"].Value);
                StartCoroutine(myCoroutine);
                return _lobbyId;
                
            }
        }
        catch(LobbyServiceException e)
        {
            //There is no lobby, we create one:
            string temp_lobbyID;
            Debug.Log("Cannot find a lobby: "+e);
            if(enteredLobbyId==null)
            {
                temp_lobbyID = await CreateMatch("QuickMatch",8,GameType.FFA,WinningCondition.Time,30, Maps[0],"");
                return temp_lobbyID;
            }
            else
            {
                return "";
            }
        }
    }

    public async Task<string> CreateMatch(string lobbyName, int playercount,GameType gameType, WinningCondition winCond, int winContCount,string mapName, string password)
    {
        Debug.Log("Creating a new lobby...");

        //External connections
        int maxConnections =playercount;

        try
        {
            //Create a Relay connection
            var /*Allocation*/ allocation=await Relay.Instance.CreateAllocationAsync(maxConnections);
            _hostData=new RelayHostData
            {
                Key=allocation.Key,
                Port=(ushort) allocation.RelayServer.Port,
                AllocationID=allocation.AllocationId,
                AllocationIDBytes=allocation.AllocationIdBytes,
                ConnectionData=allocation.ConnectionData,
                IPv4Address=allocation.RelayServer.IpV4
            };

            //get JoinCode
            _hostData.JoinCode=await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId); 
            //create a new Lobby based on the Relay above
            //int maxPlayers=2;
            CreateLobbyOptions options=new CreateLobbyOptions();
            options.IsPrivate=false;

            //put Joincode to be visible to every lobby member;
            options.Data=new Dictionary<string,DataObject>()
            {
                {
                    "joinCode",new DataObject(DataObject.VisibilityOptions.Member,_hostData.JoinCode)
                },
                {
                    "GameType",new DataObject(
                        DataObject.VisibilityOptions.Public,
                        gameType.ToString(),
                        DataObject.IndexOptions.S1
                    )
                },
                {
                    "WinningCondition",new DataObject(
                        DataObject.VisibilityOptions.Public,
                        winCond.ToString(),
                        DataObject.IndexOptions.S2
                    )
                },
                {
                    "WinningConditionValue",new DataObject(
                        DataObject.VisibilityOptions.Public,
                        winContCount.ToString(),
                        DataObject.IndexOptions.N1
                    )
                },
                {
                    "MapName",new DataObject(
                        DataObject.VisibilityOptions.Public,
                        mapName,
                        DataObject.IndexOptions.S3
                    )
                },
                {
                    "Password",new DataObject(
                        DataObject.VisibilityOptions.Public,
                        password,
                        DataObject.IndexOptions.S4
                    )
                },
            };

            Lobby lobby=await Lobbies.Instance.CreateLobbyAsync(lobbyName,playercount,options);
            _lobbyId=lobby.Id;

            Debug.Log("Created lobby: "+lobby.Id);

            //Lobby hearbeat to keep it active (25 sec, 30 is max)
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id,25));

            //Once Relay and Lobby are set, we need to set transport data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData
            );

            //START THE HOST!
            NetworkManager.Singleton.StartHost();
            return _lobbyId;

        }
        catch(LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }

        
    }



    public async Task<List<Lobby>> GetLobbiesList()
    {
        List<Lobby> lobbies = (await Lobbies.Instance.QueryLobbiesAsync()).Results;
        // foreach (Lobby lobby in lobbies)
        // {
        //     Debug.Log(lobby.Id);
        // }
        return lobbies;
    }


    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay=new WaitForSecondsRealtime(waitTimeSeconds);
        while(true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            Debug.Log("...Hearbeat sent...");
            yield return delay;
        }
    }

    private void OnDestroy()
    {
        Lobbies.Instance.DeleteLobbyAsync(_lobbyId);     
    }

    #endregion

    #region Relay

    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

    #endregion

    private async Task<string> JoinCurrentlySelectedLobby(Lobby lobby)
    {
        Debug.Log("Joined Lobby: "+lobby.Id);
                Debug.Log("Lobby playercount: "+lobby.Players.Count);

                //Get Relay code from created match
                string joinCode=lobby.Data["joinCode"].Value;

                /*JoinAllocation*/
                var allocation=await Relay.Instance.JoinAllocationAsync(joinCode);

                //get join data
                _joinData=new RelayJoinData
                {
                    Key=allocation.Key,
                    Port=(ushort) allocation.RelayServer.Port,
                    AllocationID=allocation.AllocationId,
                    AllocationIDBytes=allocation.AllocationIdBytes,
                    ConnectionData=allocation.ConnectionData,
                    HostConnectionData=allocation.HostConnectionData,
                    IPv4Address=allocation.RelayServer.IpV4
                };

                //set Transport data
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    _joinData.IPv4Address,
                    _joinData.Port,
                    _joinData.AllocationIDBytes,
                    _joinData.Key,
                    _joinData.ConnectionData,
                    _joinData.HostConnectionData
                );

                //Start Client
                NetworkManager.Singleton.StartClient();
                return _lobbyId;
    }

    private IEnumerator IsPasswordValid(Lobby lobby, string password){

        CheckPassToJoinPresenter checkPassCS=checkPassUI.GetComponent<CheckPassToJoinPresenter>();
        checkPassUI.SetActive(true);
        checkPassUI.GetComponent<UIDocument>().rootVisualElement.style.display=DisplayStyle.Flex;
        checkPassCS.password=password;
        var delay=new WaitForSecondsRealtime(0.1f);
        while(!checkPassCS.isPasscorrect)
        {
            yield return delay;
        }
        var _=  JoinCurrentlySelectedLobby(lobby);
    }
}

[System.Serializable]
public struct LobbyEntry
{
    public string username;
    public string playercount;
    public GameType gametype;
    public WinningCondition winCondition;
    public int winConditionCount;
    public string mapName;
    public bool isPassProtected;
    public string _joinCode;
}

public enum GameType
{
    FFA,TFFA,CTF,KotH
}

public enum WinningCondition
{
    Time, Kills
}

