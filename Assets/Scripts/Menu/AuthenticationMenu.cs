using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;

public class AuthenticationMenu : Panel
{
    [SerializeField] private TMP_InputField usernameInput = null;
    [SerializeField] private TMP_InputField passwordInput = null;
    [SerializeField] private Button signinButton = null;
    [SerializeField] private Button signupButton = null;
    [SerializeField] private Button anonymousButton = null;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        anonymousButton.onClick.AddListener(AnonymousSignIn);
        signinButton.onClick.AddListener(SignIn);
        signupButton.onClick.AddListener(SignUp);
        base.Initialize();
    }

    public override void Open()
    {
        usernameInput.text = "";
        passwordInput.text = "";
        base.Open();
    }
    private void AnonymousSignIn()
    {
        MenuManager.Singleton.SignInAnonymouslyAsync();
    }

    private void SignIn()
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text.Trim();
        if (string.IsNullOrEmpty(user) == false && string.IsNullOrEmpty(pass) == false)
        {
            MenuManager.Singleton.SignInWithUsernameAndPasswordAsync(user, pass);

        }

    }

    private void SignUp()
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text.Trim();
        if (string.IsNullOrEmpty(user) == false && string.IsNullOrEmpty(pass) == false)
        {
            if (IsPasswordValid(pass))
            {
                MenuManager.Singleton.SignUpWithUsernameAndPasswordAsync(user, pass);

            }
            else
            {
                ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
                panel.Open(ErrorMenu.Action.None, "Password is not strong enough: There needs to be 1 uppercase, 1 lowercase, 1 digit and a special case ", "OK");
            }
            

        }

    }

    private bool IsPasswordValid(string password)
    {
        if (password.Length < 0 || password.Length > 30)
        {
            return false;
        }
        bool hasUpperCase = false;
        bool hasLowerCase = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c))
            {
                hasUpperCase = true;
            }
            else if (char.IsLower(c))
            {
                hasLowerCase = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else if (!char.IsLetterOrDigit(c))
            {
                hasSymbol = true;
            }

        }

        return hasUpperCase && hasLowerCase && hasDigit && hasSymbol; 
    }

}
