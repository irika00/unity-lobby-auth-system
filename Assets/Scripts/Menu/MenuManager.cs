using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Authentication.PlayerAccounts;

public class MenuManager : MonoBehaviour
{
    private bool initialized = false;
    private static MenuManager singleton = null;
    private bool eventsInitialized = false;

    public static MenuManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<MenuManager>();

                singleton.Initialize();
            }
            return singleton;
        }
    }

    public virtual void Initialize()
    {
        if (initialized) { return; }
        initialized = true;
    }

    private void OnDestroy()
    {
        if (singleton == this)
        {
            singleton = null;
        }
    }

    private void Awake()
    {
        Application.runInBackground = true;
        StartClientService();



    }

    public async void StartClientService()
    {
        Debug.Log("[MenuManager] ▶ StartClientService() called");
        PanelManager.CloseAll();
        Debug.Log("[MenuManager] 🔄 All panels closed");
        PanelManager.Open("loading");
        Debug.Log("[MenuManager] ⏳ 'loading' panel opened");
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("[MenuManager] 🛠 UnityServices not initialized. Initializing...");
                var options = new InitializationOptions();
                options.SetProfile("default_profile");
                await UnityServices.InitializeAsync();
                Debug.Log("[MenuManager] ✅ UnityServices initialized");

            }
            else
            {
                Debug.Log("[MenuManager] ✅ UnityServices already initialized");
            }
            if (!eventsInitialized)
            {
                Debug.Log("[MenuManager] ⚙ Setting up authentication events...");
                SetUpEvents();
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("[MenuManager] 🔐 Session token exists. Signing in anonymously...");
                SignInAnonymouslyAsync();

            }
            else
            {
                Debug.Log("[MenuManager] 👤 No session token. Opening 'auth' panel...");
                PanelManager.Open("auth");
            }

        }
        catch (Exception exception)
        {
            Debug.LogError("[MenuManager] ❌ Exception during StartClientService(): " + exception.Message);
            ShowError(ErrorMenu.Action.StartService, "Failed to connect to the network.", "Retry");

        }
    }

    public async void SignInAnonymouslyAsync()
    {
        PanelManager.Open("loading");
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException exception)
        {
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Failed to sign in.", "OK");

        }
        catch (RequestFailedException exception)
        {

        }
    }

    public async void SignInWithUsernameAndPasswordAsync(string username, string password)
    {
        PanelManager.Open("loading");
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
        }
        catch (RequestFailedException ex)
        {

            if (ex.ErrorCode == 0)
            {
                ShowError(ErrorMenu.Action.OpenAuthMenu, "Account does not exist or password is incorrect", "OK");
            }
            else
            {
                ShowError(ErrorMenu.Action.OpenAuthMenu, "Network error. Please try again.", "OK");
            }
        }


        

    
}

    public async void SignUpWithUsernameAndPasswordAsync(string username, string password)
    {
        PanelManager.Open("loading");
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
        }
        catch (RequestFailedException ex)
        {

            if (ex.ErrorCode == 10003)
            {
                ShowError(ErrorMenu.Action.OpenAuthMenu, "An account with this username already exists", "OK");
            }
            else
            {
                ShowError(ErrorMenu.Action.OpenAuthMenu, "Network error. Please try again.", "OK");
            }
        }

}


    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
        PanelManager.CloseAll();
        PanelManager.Open("auth");
    }

    private void SetUpEvents()
    {
        eventsInitialized = true;
        AuthenticationService.Instance.SignedIn += () =>
        {
            SignInConfirmAsync();

        };
        AuthenticationService.Instance.SignedOut += () =>
        {
            PanelManager.CloseAll();
            PanelManager.Open("auth");

        };
        AuthenticationService.Instance.Expired += () =>
        {
            SignInAnonymouslyAsync();

        };


    }

    private void ShowError(ErrorMenu.Action action = ErrorMenu.Action.None, string error = "", string button = "")
    {
        PanelManager.Close("Loading");
        ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
        panel.Open(action, error, button);
    }

    private async void SignInConfirmAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync("Player");
            }
            PanelManager.CloseAll();
            PanelManager.Open("main");
        }
        catch
        {

        }
    }
}
