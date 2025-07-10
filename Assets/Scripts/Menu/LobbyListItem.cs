using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyListItem : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] public TextMeshProUGUI playersText = null;
    [SerializeField] public TextMeshProUGUI hostText = null;
    [SerializeField] private Button joinButton = null;

    private Lobby lobby = null;

    private void Start()
    {
        Debug.Log("[LobbyListItem] Start() called");
        if (joinButton == null)
        {
            Debug.LogError("[LobbyListItem] ❌ joinButton is NOT assigned!");
        }
        else
        {
            Debug.Log("[LobbyListItem] ✅ joinButton is assigned. Adding listener...");
            joinButton.onClick.AddListener(Join);
        }
    }

    public void Initialize(Lobby lobby)
    {
        Debug.Log("[LobbyListItem] Initialize() called with lobby: " + lobby.Name);
        this.lobby = lobby;

        nameText.text = lobby.Name;
        playersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        for (int i = 0; i < lobby.Players.Count; i++)
        {
            if (lobby.Players[i].Id == lobby.HostId)
            {
                hostText.text = lobby.Players[i].Data["name"].Value;
                Debug.Log("[LobbyListItem] Host found: " + hostText.text);
                break;
            }
        }
    }

    private void Join()
    {
        Debug.Log("[LobbyListItem] Join() method triggered");
        if (lobby == null)
        {
            Debug.LogError("[LobbyListItem] ❌ Cannot join: Lobby is null");
            return;
        }

        Debug.Log("[LobbyListItem] ✅ Join clicked for lobby ID: " + lobby.Id);

        LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
        if (panel == null)
        {
            Debug.LogError("[LobbyListItem] ❌ LobbyMenu panel not found via PanelManager");
            return;
        }

        Debug.Log("[LobbyListItem] Calling panel.JoinLobby with ID: " + lobby.Id);
        panel.JoinLobby(lobby.Id);
    }
}

