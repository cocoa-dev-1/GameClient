using System;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class ServerReceive
{
    public static void Welcome(Packet packet)
    {
        int myId = packet.ReadInt();
        string msg = packet.ReadString();

        Debug.Log($"Message from server: {msg}");
        NetworkManager.Singleton.myId = myId;
        ServerSend.WelcomeReceived();

        // Now that we have the client's id, connect UDP
        NetworkManager.Singleton.udp.Connect(((IPEndPoint)NetworkManager.Singleton.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet packet)
    {
        Debug.Log(packet.Length());
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.Singleton.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        if (GameManager.Singleton.players.TryGetValue(id, out PlayerManager player))
        {
            player.transform.position = position;
        }
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        if (GameManager.Singleton.players.TryGetValue(id, out PlayerManager player))
        {
            player.transform.rotation = rotation;
        }
    }
}

