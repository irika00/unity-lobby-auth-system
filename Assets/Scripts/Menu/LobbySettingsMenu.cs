using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
public class LobbySettingsMenu : Panel
{
    [SerializeField] private Button confirmButton = null;
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private TMP_InputField nameInput = null;
    [SerializeField] private TMP_InputField maxPlayersInput = null;
    [SerializeField] private TMP_Dropdown visibilityDropdown = null;

    private Lobby lobby = null;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);
        nameInput.contentType = TMP_InputField.ContentType.Standard;
        maxPlayersInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        nameInput.characterLimit = 20;
        maxPlayersInput.characterLimit = 2;
        base.Initialize();
    }
    public void Open(Lobby lobby)
    {
        this.lobby = lobby;
        if (lobby == null)
        {
            nameInput.name = "";
            maxPlayersInput.name = "5";
            visibilityDropdown.SetValueWithoutNotify(0);

        }
        else
        {
            nameInput.name = lobby.Name;
            maxPlayersInput.name = lobby.MaxPlayers.ToString();
            visibilityDropdown.SetValueWithoutNotify(lobby.IsPrivate ? 1 : 0);
            for (int i = 0; i < visibilityDropdown.options.Count; i++)
            {
                if ((lobby.IsPrivate && visibilityDropdown.options[i].text.ToLower() == "private") || (lobby.IsPrivate == false && visibilityDropdown.options[i].text.ToLower() == "public"))
                {
                    visibilityDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }


        }
        Open();

    }

    private void Cancel()
    {
        Close();
    }

    private void Confirm()
    {
        string lobbyName = nameInput.text.Trim();
        int maxPlayer = 0;
        int.TryParse(maxPlayersInput.text.Trim(), out maxPlayer);
        bool isPrivate = visibilityDropdown.captionText.text.Trim().ToLower() == "private" ? true : false;
        if (maxPlayer > 0 && string.IsNullOrEmpty(lobbyName) == false)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (lobby == null)
            {
                panel.CreateLobby(lobbyName, maxPlayer, isPrivate);
            }
            else
            {
                panel.UpdateLobby(lobby.Id, lobbyName, maxPlayer, isPrivate);
            }
            Close();
        }

    }

    public override void Close()
    {
        base.Close();
        lobby = null;

    }

}
