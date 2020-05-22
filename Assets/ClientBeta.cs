using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ClientBeta : MonoBehaviour
{
    #region Variables

    //CONSTANTS
    private const int MAX_CONECTIONS = 40;
    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 8999;
    private const int SERVER_WEB_PORT = 8998;
    private const int BUFFER_SIZE = 1024;

    private const int BYTE_SIZE = 1024;

    //CHANNELS
    private int reliableChannelID;
    private int unReliableChannelID;

    //HOSTS
    private int hostID;
    private int connectionID;


    //LOGIC
    private bool isConnected;
    private byte[] buffer = new byte[BUFFER_SIZE];

    private byte error;
    #endregion
    private bool isStarted;
    // Start is called before the first frame update
    private void Start()
    {
        //getting the msg that server is started or not
        //ButtonScript gameObject = new ButtonScript();

        GlobalConfig confing = new GlobalConfig();
        NetworkTransport.Init(confing);

        //HOST TOPOLOGY
        ConnectionConfig connectionConfig = new ConnectionConfig();
        reliableChannelID = connectionConfig.AddChannel(QosType.Reliable);
        unReliableChannelID = connectionConfig.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(connectionConfig, MAX_CONECTIONS);

        //CONNECTING TO HOSTS
        hostID = NetworkTransport.AddHost(topology, 0); //DEFINE WHAT CONNECTION TO BE SYNC WITH AND TO CARRY TOPOLOGY IN SAME NETWORK MAP

        //Standalone client
#if UNITY_WEBGL
        //WEBGL client
        connectionID = NetworkTransport.Connect(hostID, SERVER_IP, SERVER_WEB_PORT, 0, out error);
#else
        //Standalone client

        connectionID = NetworkTransport.Connect(hostID, SERVER_IP, SERVER_PORT, 0, out error);
#endif

        isStarted = true;//ameObject.isStartGameCliked;

        if (isStarted)
        {
            Debug.Log("Server is online and Game is now starting");
        }
    }

    private void Update()
    {
        UpdatemessagePump();
        if (Input.GetKeyDown(KeyCode.Space)) TESTFUNCTIONCREATEACCOUNT();
    }

    public void UpdatemessagePump()
    {
        if (!isStarted) { return; }

        int recHostID;      //Which platform
        int connectionID;   //Which User Sending the message
        int channelID;      //Which Lane used to send the specific message

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostID, out connectionID, out channelID, recBuffer, BYTE_SIZE, out dataSize, out error);

        switch (type)
        {
            case NetworkEventType.Nothing:
                {
                    break;
                }
            case NetworkEventType.ConnectEvent:
                {
                    Debug.Log(string.Format(" connected to the server", connectionID));
                    break;
                }
            case NetworkEventType.DisconnectEvent:
                {
                    Debug.Log(string.Format("disconnected from the server", connectionID));
                    break;
                }   //
            case NetworkEventType.DataEvent:
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream(recBuffer);
                    NetMsg message = (NetMsg)formatter.Deserialize(memoryStream);

                    OnData(connectionID, channelID, recHostID, message);

                    break;
                }

            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected Nettwork event type" + type);
                break;
        }
    }

    #region OnData
    private void OnData(int connectionID, int channelID, int recHostID, NetMsg message)
    {
        switch (message.OP)
        {
            case NetOP.None:
                Debug.Log("Unexpected Network Operation");
                break;

        }

    }

    #endregion

    #region SendingData
    public void SendServer(NetMsg message)
    {
        //data hodler
        byte[] dataBuffer = new byte[BYTE_SIZE];

        //crushing data to byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream memoryStream = new MemoryStream(dataBuffer);
        formatter.Serialize(memoryStream, message);

        NetworkTransport.Send(hostID, connectionID, reliableChannelID, dataBuffer, BYTE_SIZE, out error);
    }
    #endregion

    public void TESTFUNCTIONCREATEACCOUNT()
    {
        Debug.Log("sending message..");
        Net_CreateAccount createAccount = new Net_CreateAccount();
        createAccount.Email = "Sakuna";
        createAccount.Email = "test1234";
        createAccount.Email = "quiz+week+1+0";

        SendServer(createAccount);

    }
}