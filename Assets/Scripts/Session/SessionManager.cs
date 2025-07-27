using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;


public class SessionManager : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabsList charactersPrefab = null;
    public static Role role = Role.Client;
    public static string joinCode = "";
    public static string lobbyId = "";
    public enum Role
    {
        Client = 1, Host = 2, Server = 3
    }
    private static SessionManager singleton = null;
    public static SessionManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<SessionManager>();
                singleton.Initialize();
            }
            return singleton;
        }

    }
    private bool initialized = false;
    private void Initialize()
    {
        if (initialized) { return; }
        initialized = true;

    }
    public override void OnDestroy()
    {
        if (singleton == this)
        {
            singleton = null;
        }
        base.OnDestroy();
    }

    private void Start() //not in awake
    {
        Debug.Log("Start called in SessionManager");
        Initialize();
        NetworkManager.Singleton.OnClientConnectedCallback += onClientConnected;
        if (role == Role.Client)
        {
            Debug.Log("role client");
            bool success = NetworkManager.Singleton.StartClient(); // or StartHost()
            if (!success) {
                Debug.LogError("Role.client Failed to start networking.");
            }
        }
        else if (role == Role.Host)
        {
            Debug.Log("role host");
            bool success = NetworkManager.Singleton.StartHost(); // or StartHost()
            if (!success) {
                Debug.LogError( "Role.host Failed to start networking.");
            }
            if (string.IsNullOrEmpty(joinCode) == false && string.IsNullOrEmpty(lobbyId) == false)
            {
                SetLobbyJoinCode(joinCode);
            }
        }
        else
        {
            NetworkManager.Singleton.StartServer();
        }
      

    }

    private void onClientConnected(ulong id)
    {
        Debug.Log("Spawn: Reached onClientConnected");
        if (NetworkManager.Singleton.IsServer)
        {
            RpcParams rpcParams = NetworkManager.Singleton.RpcTarget.Single(id, RpcTargetUse.Temp);
            InitializeRpc(rpcParams);

        }

    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void InitializeRpc(RpcParams rpcParams)
    {
        Debug.Log("Spawn: Calling InitializeRpc()");
        InitializeClient();
    }

    private async void InitializeClient()
    {
        Debug.Log("Spawn: Reached InitializeClient()");

        int character = 0;         // Default character index
        string color = "#FF0000";  // Default color (red)

        InstantiateCharacterRpc(character, AuthenticationService.Instance.PlayerId, color);

    }

    [Rpc(target: SendTo.Server, RequireOwnership = false)]
    private void InstantiateCharacterRpc(int character, string id, string color, RpcParams rpcParams = default)
    {
        Debug.Log("Spawn: Calling InstantiateCharacterRpc()");

        Vector3 position = SessionSpawnPoints.Singleton.GetSpawnPositionOrdered();
        var prefab = charactersPrefab.PrefabList[character].Prefab.GetComponent<NetworkObject>();
        Debug.Log("Instantiating character prefab");
        var networkObject = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(prefab, rpcParams.Receive.SenderClientId, true, true, false, position, Quaternion.identity);
        Debug.Log("Spawn position: " + position);
        SessionPlayer player = networkObject.GetComponent<SessionPlayer>();
        player.ApplyDataRpc(id, color);
        SessionPlayer[] players = FindObjectsByType<SessionPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != player)
                {
                    players[i].ApplyDataRpc();
                }
            }
        }

    }

    private async void SetLobbyJoinCode(string code)
    {
        Debug.Log("SetLobbyJoinCode");
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>();
            options.Data.Add("join_code", new DataObject(DataObject.VisibilityOptions.Public, code));
            var lobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);

        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);

        }

    }

    
}