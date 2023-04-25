using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField]
    public TMP_InputField usernameField;
    [SerializeField]
    public GameObject startMenu;

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        NetworkManager.Singleton.ConnectToServer();
    }
}

