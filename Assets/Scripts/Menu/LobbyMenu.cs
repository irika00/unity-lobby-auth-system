using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine.SceneManagement;




public class LobbyMenu : Panel
{
    [SerializeField] private LobbyPlayerItem lobbyPlayerItemPrefab = null;
    [SerializeField] private RectTransform lobbyPlayersContainer = null;
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button leaveButton = null;
    [SerializeField] private Button readyButton = null;
    [SerializeField] private Button startButton = null;

    private Lobby lobby = null; public Lobby JoinedLobby { get { return lobby; }}

    private float updateTimer = 0;
    private float heartbeatPeriod = 15;
    private bool sendingHeartbeat = false;
    private ILobbyEvents events = null;
    private bool isReady = false;
    private bool isHost = false;
    private string eventsLobbyId = "";
    private bool isStarted = false;
    private bool isJoining = false;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        ClearPlayersList();
        closeButton.onClick.AddListener(ClosePanel);
        leaveButton.onClick.AddListener(LeaveLobby);
        readyButton.onClick.AddListener(SwitchReady);
        startButton.onClick.AddListener(StartGame);
        base.Initialize();
    }
    public void Open(Lobby lobby)
    {
        if (eventsLobbyId != lobby.Id)
        {
            _ = SubscribeToEventsAsync(lobby.Id);

        }
        this.lobby = lobby;
        nameText.text = lobby.Name;
        CheckStartGameStatus();
        startButton.gameObject.SetActive(false);
        isHost = false;
        LoadPlayers();
        Open();


    }

    private async void StartGame()
    {
        Debug.Log("called StartGame");
        PanelManager.Open("loading");
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var data = AllocationUtils.ToRelayServerData(allocation, "dtls");
            transport.SetRelayServerData(data);
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            SessionManager.role = SessionManager.Role.Host;
            SessionManager.joinCode = code;
            SessionManager.lobbyId = lobby.Id;
            SetLobbyStarting();
            StartingSessionMenu panel = (StartingSessionMenu)PanelManager.GetSingleton("start");
            heartbeatPeriod = 5;
            await UnsubscribeToEventsAsync();
            panel.StartGameByLobby(lobby, false);

        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);

        }
        PanelManager.Close("loading");

    }

    private async void SetLobbyStarting()
    {
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>();
            options.Data.Add("started", new DataObject(DataObject.VisibilityOptions.Public, "1"));
            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);

        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);

        }

    }

    private async void CheckStartGameStatus()
    {
        if (lobby == null || lobby.Data == null)
        {
            Debug.LogWarning("Lobby or lobby.Data is null. Cannot check start game status.");
            return;
        }

        StartingSessionMenu panel = (StartingSessionMenu)PanelManager.GetSingleton("start");
        isStarted = lobby.Data.ContainsKey("started");
        string joinCode = lobby.Data.ContainsKey("join_code") ? lobby.Data["join_code"].Value : null;
        if (panel.isLoading == false && isStarted)
        {
            panel.StartGameByLobby(lobby, true);
        }
        if (isJoining == false && panel.isLoading && string.IsNullOrEmpty(joinCode) == false && panel.isConfirmed == false)
        {
            panel.StartGameByLobby(lobby, true);
            JoinGame(joinCode);
        }

    }

    private async void JoinGame(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode) == false)
        {
            isJoining = true;
            PanelManager.Open("loading");
            try
            {
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                var data = AllocationUtils.ToRelayServerData(allocation, "dtls");
                transport.SetRelayServerData(data);

                //set session data

                StartingSessionMenu panel = (StartingSessionMenu)PanelManager.GetSingleton("start");
                await UnsubscribeToEventsAsync();
                panel.StartGameConfirm();

            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
                await Leave();
                isJoining = false;
                PanelManager.Close("start");

            }
            PanelManager.Close("loading");
        }
        
    }
    private void Update()
    {
        if (lobby == null)
        {
            return;
        }
        if (isHost == false && isJoining == false)
        {
            CheckStartGameStatus();
        }
        if (lobby.HostId == AuthenticationService.Instance.PlayerId && sendingHeartbeat == false)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < heartbeatPeriod)
            {
                return;
            }
            updateTimer = 0;
            HeartbeatLobbyAsync();
        }
    }

    private async void HeartbeatLobbyAsync()
    {
        sendingHeartbeat = true;
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            Debug.Log("Heartbeat sent for: " + lobby.Name);
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);

        }
        sendingHeartbeat = false;
    }

    private void LoadPlayers()
    {
        ClearPlayersList();
        bool isEveryoneReady = true;
        bool youAreMember = false;
        for (int i = 0; i < lobby.Players.Count; i++)
        {
            bool ready = lobby.Players[i].Data["ready"].Value == "1";
            LobbyPlayerItem item = Instantiate(lobbyPlayerItemPrefab, lobbyPlayersContainer);
            item.Initialize(lobby.Players[i], lobby.Id, lobby.HostId);
            if (lobby.Players[i].Id == AuthenticationService.Instance.PlayerId)
            {
                youAreMember = true;
                isReady = ready;
                isHost = lobby.Players[i].Id == lobby.HostId;
            }
            if (ready == false)
            {
                isEveryoneReady = false;
            }
            startButton.gameObject.SetActive(isHost);
            if (isHost)
            {
                startButton.interactable = isEveryoneReady;
            }
            if (youAreMember == false)
            {
                Close();
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        PanelManager.Open("loading");
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Data = new Dictionary<string, DataObject>();
            options.Player = new Player();
            options.Player.Data = new Dictionary<string, PlayerDataObject>();
            options.Player.Data.Add("name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerName));
            options.Player.Data.Add("ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0"));

            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            PanelManager.Close("lobby_search");
            Open(lobby);

        }
        catch (Exception exception)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to create Lobby", "OK");
            Debug.Log(exception.Message);

        }

        PanelManager.Close("loading");

    }

    public async void JoinLobby(string id)
    {
        Debug.Log("Attempting to join lobby with ID: " + id);
        PanelManager.Open("loading");
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();

            options.Player = new Player();
            options.Player.Data = new Dictionary<string, PlayerDataObject>();
            options.Player.Data.Add("name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerName));
            options.Player.Data.Add("ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0"));

            lobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, options);
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            panel.Open(lobby);
            PanelManager.Close("lobby_search");
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        PanelManager.Close("loading");
    }

    public async void UpdateLobby(string lobbyId, string lobbyName, int maxPlayers, bool isPrivate)
    {
        PanelManager.Open("loading");
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Name = lobbyName;
            options.MaxPlayers = maxPlayers;
            options.Data = new Dictionary<string, DataObject>();



            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
            PanelManager.Close("lobby_search");
            Open(lobby);

        }
        catch (Exception exception)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to change Lobby Host", "OK");
            Debug.Log(exception.Message);

        }

        PanelManager.Close("loading");

    }
    private void LeaveLobby()
    {
        _ = Leave();
    }

    private async Task Leave()
    {
        PanelManager.Open("loading");
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);
            lobby = null;
            Close();
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        PanelManager.Close("loading");
    }

    private async void SwitchReady()
    {
        readyButton.interactable = false;
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>();
            options.Data.Add("ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, isReady ? "0" : "1"));
            lobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, options);
            LoadPlayers();
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);

        }
        readyButton.interactable = true;

    }

    private void ClearPlayersList()
    {
        LobbyPlayerItem[] items = lobbyPlayersContainer.GetComponentsInChildren<LobbyPlayerItem>();
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Destroy(items[i].gameObject);
            }

        }
    }

    private async Task<bool> SubscribeToEventsAsync(string id)
    {
        try
        {
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnChanged;
            callbacks.LobbyEventConnectionStateChanged += OnConnectionChanged;
            callbacks.KickedFromLobby += OnKicked;
            events = await LobbyService.Instance.SubscribeToLobbyEventsAsync(id, callbacks);
            eventsLobbyId = lobby.Id;
            return true;

        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        return false;
    }

    private async Task UnsubscribeToEventsAsync()
    {
        try
        {
            if (events != null)
            {
                await events.UnsubscribeAsync();
                events = null;
            }

        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);

        }


    }
    private void OnKicked()
    {
        if (IsOpen)
        {
            Close();
        }
        lobby = null;
        events = null;
        isStarted = false;
        isJoining = false;

    }

    private void OnChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            if (IsOpen)
            {
                Close();
            }
            lobby = null;
            events = null;
            isStarted = false;
            isJoining = false;
        }
        else
        {
            changes.ApplyToLobby(lobby);
            CheckStartGameStatus();
            if (IsOpen)
            {
                LoadPlayers();
            }
        }
    }

    private async void OnConnectionChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed:
            case LobbyEventConnectionState.Error:
                if (lobby != null)
                {

                }
                break;
            case LobbyEventConnectionState.Subscribing: break;
            case LobbyEventConnectionState.Subscribed: break;
            case LobbyEventConnectionState.Unsynced: break;

        }
    }
    private void ClosePanel()
    {
        Close();

    }

}

