using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ServerClient : MonoBehaviour
{

    public static ServerClient instance;

    private static bool ready;
    public static bool Ready { get { return ready; } }

    public int hostingPort;
    public int webhostingPort;
    public bool isServer;
    public string connectionIP;
    public TextMeshProUGUI infoText;

    int reliableId;
    int unreliableId;
    HostTopology topology;
    int hostId;
    int websocketId;
    int connectionId = -1;
    List<int> clientConnections = new List<int>();
    bool connected = false;

    void Start()
    {
        instance = this;
        // Initialize the Transport layer.
        NetworkTransport.Init();
        // Configure topology
        ConnectionConfig config = new ConnectionConfig();
        reliableId = config.AddChannel(QosType.Reliable);
        unreliableId = config.AddChannel(QosType.Unreliable);
        // At most 12 connected clients. Should be enough.
        topology = new HostTopology(config, 12);
        websocketId = NetworkTransport.AddWebsocketHost(topology, webhostingPort, null);
        hostId = NetworkTransport.AddHost(topology, hostingPort);
        infoText.text = "Socket Open. Socket ID is: " + hostId;
        Debug.Log("Socket Open. Socket ID is: " + hostId);
        ServerClient.ready = true;
    }

    public bool BecomeServer()
    {
        if (connectionId != -1) return false;
        Debug.Log("Becoming Server...");
        isServer = true;
        return true;
    }

    public bool ConnectToServer()
    {
        if (isServer) return false;
        Debug.Log("Attempting to Connect to Server...");
        infoText.text = "Attempting to Connect to Server...";
        isServer = false;
        byte error;
#if UNITY_WEBGL
        connectionId = NetworkTransport.Connect(hostId, connectionIP, webhostingPort, 0, out error);
#else
        connectionId = NetworkTransport.Connect(hostId, connectionIP, hostingPort, 0, out error);
#endif
        if ((NetworkError)error == NetworkError.Ok)
        {
            Debug.Log("Connected to server. ConnectionId: " + connectionId);
            infoText.text = "Connected to server. ConnectionId: " + connectionId;
            connected = true;
        }
        else
        {
            Debug.Log("Failed to connect to server, with error: " + (NetworkError)error);
            infoText.text = "Failed to connect to server, with error: " + (NetworkError)error;
            connected = false;
        }
        return connected;
    }

    void Update()
    {
        // Handle messages from the queue.
        int recHostId;
        int recConId;
        int chanId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConId, out chanId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing: break;
            case NetworkEventType.ConnectEvent:
                {
                    if (connectionId == recConId)
                    {
                        connected = true;
                        Debug.Log("Connected!");
                    }
                    else
                    {
                        if (isServer)
                        {
                            Debug.Log("Recieved connection request from " + recConId);
                            clientConnections.Add(recConId);
                        }
                    }
                    break;
                }
            case NetworkEventType.DataEvent:
                {
                    string res = "";
                    for (int i = 0; i < dataSize; i++)
                        res = res + recBuffer[i];
                    infoText.text = dataSize + " Recieved Data: " + res;
                    Stream stream = new MemoryStream(recBuffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(stream) as string;
                    Debug.Log("Recieved Data: " + message);
                    if (isServer)
                    {
                        Debug.Log("Sending confirmation...");
                        SendToClient(recHostId, recConId);
                    }
                    break;
                }
            case NetworkEventType.DisconnectEvent:
                {
                    if (connectionId == recConId)
                    {
                        connected = false;
                        Debug.Log("Connection Failed!");
                    }
                    else
                    {
                        // One of the established connections has disconnected.
                        Debug.Log("Dead!");
                    }
                    break;
                }
            case NetworkEventType.BroadcastEvent:
                {
                    Debug.Log("Broadcast recieved!");
                    break;
                }
        }

        if (Input.GetKeyDown(KeyCode.S)) BecomeServer();
        if (Input.GetKeyDown(KeyCode.C)) ConnectToServer();
        if (Input.GetKeyDown(KeyCode.D)) SendCmd();
        if (Input.GetKeyDown(KeyCode.E)) SendToClient(hostId,connectionId);
    }

    public void SendCmd()
    {
        Debug.Log("Sending...");
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, "Hello from Client");
        int bufferSize = 1024;
        NetworkTransport.Send(hostId, connectionId, reliableId, buffer, bufferSize, out error);
        if ((NetworkError)error != NetworkError.Ok)
            Debug.Log("Failed to send command. Send failed with error: " + (NetworkError)error);
        else Debug.Log("Sent with no errors!");
    }

    public void SendToClient(int hid, int clientId)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, "Hello from Server");
        int bufferSize = 1024;
        NetworkTransport.Send(hid, clientId, reliableId, buffer, bufferSize, out error);
        if ((NetworkError)error != NetworkError.Ok)
            Debug.Log("Failed to send command. Send failed with error: " + (NetworkError)error);
        else Debug.Log("Sent with no errors!");
    }

}
