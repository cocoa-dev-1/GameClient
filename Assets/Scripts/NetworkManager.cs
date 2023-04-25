using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;

public class NetworkManager : SingletonBehaviour<NetworkManager>
{
    public readonly int dataBufferSize = 4096;

    public int myId;
    public string ip = "127.0.0.1";
    public int port = 26950;

    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private Dictionary<int, PacketHandler> packetHandlers;

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect(); // Disconnect when the game is closed
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        isConnected = true;
        tcp.Connect();
    }


    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = Singleton.dataBufferSize,
                SendBufferSize = Singleton.dataBufferSize
            };

            receiveBuffer = new byte[Singleton.dataBufferSize];

            socket.BeginConnect(Singleton.ip, Singleton.port, new AsyncCallback(ConnectCallback), null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null); // Send data to server
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via TCP: {ex}");
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, Singleton.dataBufferSize, new AsyncCallback(ReceiveCallback), null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                Debug.Log("tcp data received.");
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Singleton.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data)); // Reset receivedData if all data was handled
                stream.BeginRead(receiveBuffer, 0, Singleton.dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            try
            {
                //Debug.Log("tcp handle data 1");
                int packetLength = 0;

                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    // If client's received data contains a packet
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        // If packet contains no data
                        return true; // Reset receivedData instance to allow it to be reused
                    }
                }

                //Debug.Log("tcp handle data 2");

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    //Debug.Log("tcp handle data 3");
                    // While packet contains data AND packet data length doesn't exceed the length of the packet we're reading
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    Debug.Log("tcp handle data 4");
                    //Debug.Log($"test : {ThreadManager.Singleton}");
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        //Debug.Log("tcp handle data 5");
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            NetworkManager.Singleton.packetHandlers[packetId](packet); // Call appropriate method to handle the packet
                        }
                    });
                    //Debug.Log("tcp handle data 6");

                    packetLength = 0; // Reset packet length
                    if (receivedData.UnreadLength() >= 4)
                    {
                        // If client's received data contains another packet
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            // If packet contains no data
                            return true; // Reset receivedData instance to allow it to be reused
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true; // Reset receivedData instance to allow it to be reused
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.Log($"Error handling TCP data : {e}");
                return false;
            }
        }

        public void Disconnect()
        {
            Singleton.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;

        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(Singleton.ip), Singleton.port);
        }

        public void Connect(int port)
        {
            socket = new UdpClient(port);

            socket.Connect(endPoint);

            socket.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(Singleton.myId); // Insert the client's ID at the start of the packet
                if (socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via UDP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(new AsyncCallback(ReceiveCallback), null);

                if (data.Length < 4)
                {
                    Singleton.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();
                    NetworkManager.Singleton.packetHandlers[packetId](packet); // Call appropriate method to handle the packet
                }
            });
        }

        public void Disconnect()
        {
            Singleton.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    public void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ServerReceive.Welcome },
            { (int)ServerPackets.spawnPlayer, ServerReceive.SpawnPlayer },
            { (int)ServerPackets.playerPosition, ServerReceive.PlayerPosition },
            { (int)ServerPackets.playerRotation, ServerReceive.PlayerRotation },
        };
        Debug.Log("Initialized packets.");
    }

    public void Disconnect()
    {
        if (!isConnected) return;

        isConnected = false;

        tcp.socket.Close();
        udp.socket.Close();

        Debug.Log("Disconnected from server.");
    }
}

