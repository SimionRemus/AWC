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
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;

public class SCR_GameManager : MonoBehaviour
{
    private string _lobbyId;
    private RelayHostData _hostData;
    private RelayJoinData _joinData;



    // Start is called before the first frame update
    async void Start()
    {
        await UnityServices.InitializeAsync();

        SetupEvents();

        await SignInAnonymouslyAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public async void FindMatch()
    {
        Debug.Log("Looking for a lobby...");
        try
        {
            //We can add filters here
            QuickJoinLobbyOptions options=new QuickJoinLobbyOptions();

            //Quick-join a random lobby that matches options above
            Lobby lobby=await Lobbies.Instance.QuickJoinLobbyAsync(options);

            Debug.Log("Joined Lobby: "+lobby.Id);
            Debug.Log("Lobby playercount: "+lobby.Players.Count);

            //Get Relay code from created match
            string joinCode=lobby.Data["joinCode"].Value;
            /*JoinAllocation*/var allocation=await Relay.Instance.JoinAllocationAsync(joinCode);

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

        }
        catch(LobbyServiceException e)
        {
            //There is no lobby, we create one:
            Debug.Log("Cannot find a lobby: "+e);
            CreateMatch();
        }
    }

    private async void CreateMatch()
    {
        Debug.Log("Creating a new lobby...");

        //External connections
        int maxConnections =1;

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
            string lobbyName="gameLobbyExample";
            int maxPlayers=2;
            CreateLobbyOptions options=new CreateLobbyOptions();
            options.IsPrivate=false;

            //put Joincode to be visible to every lobby member;
            options.Data=new Dictionary<string,DataObject>()
            {
                {
                    "joinCode",new DataObject(DataObject.VisibilityOptions.Member,_hostData.JoinCode)
                },

            };



            var lobby=await Lobbies.Instance.CreateLobbyAsync(lobbyName,maxPlayers,options);
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

        }
        catch(LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }
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

}
