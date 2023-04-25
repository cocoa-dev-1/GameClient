using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public Dictionary<int, PlayerManager> players;

    [SerializeField]
    private GameObject localPlayerPrefab;
    [SerializeField]
    private GameObject playerPrefab;

    private void Awake()
    {
        players = new Dictionary<int, PlayerManager>();
    }

    private void Start()
    {
        //Screen.SetResolution(640, 480, false);
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        if (id == NetworkManager.Singleton.myId)
        {
            player = Instantiate(localPlayerPrefab, position, rotation);
        } else
        {
            player = Instantiate(playerPrefab, position, rotation);
        }

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.id = id;
        playerManager.username = username;

        players.Add(id, playerManager);
    }
}
