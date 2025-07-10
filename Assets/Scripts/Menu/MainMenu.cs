using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Linq;

public class MainMenu : Panel
{
    public TextMeshProUGUI nameText = null;

    [SerializeField] private Button logoutButton = null;
    [SerializeField] private Button lobbyButton = null;
    private List<string> joinedLobbyIds = new List<string>();
    private void Start()
    {
        // Delay to ensure all panels are registered
        StartCoroutine(DelayedLobbyCheck());
    }

    private IEnumerator DelayedLobbyCheck()
    {
        yield return new WaitForEndOfFrame(); // or new WaitForSeconds(0.1f)
        Debug.Log("Opening lobby_search...");
        PanelManager.Open("lobby_search");
    }

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        logoutButton.onClick.AddListener(SignOut);
        lobbyButton.onClick.AddListener(Lobby);
        base.Initialize();
    }
    public override void Open()
    {
        UpdatePlayerNameUI();
        base.Open();
    }

    private async void Lobby()
    {
        PanelManager.Open("loading");
        try
        {
            joinedLobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();



        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }

        Lobby lobby = null;
        if (joinedLobbyIds.Count > 0)
        {
            try
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyIds.Last());

            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);

            }
        }

        if (lobby == null)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (panel.JoinedLobby != null && joinedLobbyIds.Count > 0 && panel.JoinedLobby.Id == joinedLobbyIds.Last())
            {
                lobby = panel.JoinedLobby;
            }

        }
        if (lobby != null)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            panel.Open(lobby);

        }
        else
        {
            PanelManager.Open("lobby_search");

        }
        PanelManager.Close("loading");


    }

    private void SignOut()
    {
        MenuManager.Singleton.SignOut();

    }

    private void UpdatePlayerNameUI()
    {
        nameText.text = AuthenticationService.Instance.PlayerName;
    }

}
