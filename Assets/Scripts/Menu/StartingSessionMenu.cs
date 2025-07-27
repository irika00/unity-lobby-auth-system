using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingSessionMenu : Panel
{
    [SerializeField] private TextMeshProUGUI statusText = null;
    [SerializeField] private Image progressimage = null;
    private bool allowSceneActivation = false; public bool isConfirmed { get { return allowSceneActivation; } }
    private bool loading = false; public bool isLoading { get { return loading; } }

    public void StartGameByLobby(Lobby lobby, bool waitForConfirmation)
    {
        Debug.Log("[StartingSessionMenu] StartGameByLobby called for: " + lobby.Name);

        Open();
        statusText.text = "Loading the game, please wait..";

        string sceneName = "SessionMap01";
        //set scene based on map value

        allowSceneActivation = !waitForConfirmation;
        if (loading == false)
        {
            loading = true;
            StartCoroutine(LoadAsyncScene(sceneName));

        }


    }

    public void StartGameConfirm()
    {
        allowSceneActivation = true;

    }

    public override void Open()
    {
        progressimage.fillAmount = 0;
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        loading = false;
        allowSceneActivation = false;

    }
    private System.Collections.IEnumerator LoadAsyncScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        while (operation.isDone == false)
        {
            progressimage.fillAmount = operation.progress;
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = allowSceneActivation;
            }
            yield return null;
        }


    }


}
