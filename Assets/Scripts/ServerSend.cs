using System;

public class ServerSend
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(Packet _packet)
    {
        _packet.InsertLength();
        NetworkManager.Singleton.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(Packet _packet)
    {
        _packet.InsertLength();
        NetworkManager.Singleton.udp.SendData(_packet);
    }

    #region TCP Packets

    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(NetworkManager.Singleton.myId);
            _packet.Write(UIManager.Singleton.usernameField.text);

            SendTCPData(_packet);
        }
    }

    #endregion

    #region UDP Packets

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs"></param>
    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.Singleton.players[NetworkManager.Singleton.myId].transform.rotation);

            SendUDPData(_packet);
        }
    }

    #endregion
}

