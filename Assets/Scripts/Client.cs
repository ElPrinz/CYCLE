using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System;
using TMPro;
using Unity.RemoteConfig;
using System.Collections.Generic;
using System.Collections;

public class Client : MonoBehaviour
{


    public struct userAttributes { }
    public struct appAttributes { }

    public enum SendServerType
    {
        NONE,
        LOGIN_REQUEST,
        CREATE_ACCOUNT,
        ADD_FOLLOW,
        REMOVE_FOLLOW,
        REQUEST_FOLLOW,
        HOME_SETUP,
        COMMING_SOON,

    }

    public static Client Instance { set; get; }

    private const int CurrentVersion = 0;
    private const int MAX_USER = 100;
    private const int PORT = 26000 ;
    private int WEB_PORT = 26001; 
    private const int BYTE_SIZE = 512;
    private string SERVER_IP = "127.0.0.1";

    private byte reliableChannel;
    private int connectionId;
    private int hostId;
    private byte error;

    private bool isStarted;
    public Account self;
    public DebugConsole DebugConsole;
    private string token;
    private bool wrongV = false;


    #region Monobehaviour
    private void Awake()
    {
        ConfigManager.FetchCompleted += SetServerIP;
        //ConfigManager.FetchCompleted += CheckVersion;
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }
    void Start()
    {

        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(InitByHost());
        //Init();

    }
    private void Update()
    {
        UpdateMessagePump();

    }
    IEnumerator InitByHost()
    {
        DebugConsole.isVisible = true;
        DebugConsole.Log("Fetching Host IP...");
        Debug.LogError("Fetching Host IP...");
        yield return new WaitForSeconds(1);
        Init();

    }

    void SetServerIP(ConfigResponse response)
    {
        SERVER_IP = ConfigManager.appConfig.GetString("ServerIP");
        int Version = ConfigManager.appConfig.GetInt("Version");
        WEB_PORT = ConfigManager.appConfig.GetInt("serverport");
        Debug.LogWarning(SERVER_IP + ":" + WEB_PORT);
        DebugConsole.Log(SERVER_IP + ":" + WEB_PORT);
        if (CurrentVersion != Version)
        {
            // Force to update
            Debug.LogError("Missing Latest Version!");
            DebugConsole.Log("Missing Latest Version!");
            Application.Quit();
        }
        else
        {
            Debug.LogError("Version Compatible: " + Version);
            DebugConsole.Log("Version Compatible: " + Version);
        }
    }
   

    public void Init()
    {
        NetworkTransport.Init();



        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable); //If choose Unreliable will be faster but lose data between

        HostTopology topo = new HostTopology(cc, MAX_USER);

        // Client only code
        hostId = NetworkTransport.AddHost(topo, 0);

#if UNITY_WEBGL && !UNITY_EDITOR
        // WEBGL Client
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);
        Debug.LogError(string.Format("Connecting from Web"));
        DebugConsole.Log("Connecting from Web");
#else
        // Standalone Client
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, PORT, 0, out error);
        Debug.LogError(string.Format("Connecting from standlone"));
        DebugConsole.Log("Connecting from standlone");
#endif
        Debug.LogError(string.Format("Attemping to connect on port {0}...", SERVER_IP));
        DebugConsole.Log("Attemping to connect on port...idk");
        isStarted = true;
    }
    public void Shutdown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }

    private void UpdateMessagePump()
    {
        if (!isStarted)
            return;

        int recHostId;             // Is this from Web? Or standalone
        int connectionId;         // Which user is sending me this?
        int channelId;           // Which lane is he sending that message from

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                Debug.LogError("We have connected to the server");
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.LogError("We have been disconnected");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
                //in online games now send to login screen
                break;

            case NetworkEventType.DataEvent:
                // MUST BACK

                try
                {

                    string message = System.Text.Encoding.UTF8.GetString(recBuffer, 0, recBuffer.Length);

                    Debug.LogWarning(recBuffer.Length + " <> " + message);
                    //SendToServer(recHostId, connectionId);
                    //TESTFUNCTIONCREATEACCOUNT();
                    string[] splitData = message.Split('|');
                    switch (splitData[0])
                    {
                        case "ON_CREATE_ACCOUNT":
                            //CreateAccountNew(splitData[1], splitData[2], splitData[3], recHostId, connectionId);
                            if (splitData[1] == "1")
                            {
                                LobbyScene.Instance.ChangeAuthenticationMessage(splitData[2]);
                                LobbyScene.Instance.EnableInputs();
                            }
                            else
                            {
                                LobbyScene.Instance.ChangeAuthenticationMessage(splitData[2]);
                                LobbyScene.Instance.EnableInputs();
                            }
                            break;

                        case "ON_LOGIN_REQUEST":
                            if (splitData[1] == "1")
                            {
                                LobbyScene.Instance.ChangeAuthenticationMessage(splitData[2]);
                                LobbyScene.Instance.DisableInputs();
                                Debug.LogWarning("LOGIN SUCCESS!");
                                self = new Account();
                                self.ActiveConnection = connectionId;
                                string[] splitDataAgain = splitData[4].Split('#');
                                self.Username = splitDataAgain[0];//splitDataAgain[0];
                                self.Discriminator = splitDataAgain[1];//splitDataAgain[1];
                                token = splitData[3];

                                UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
                            }
                            else
                            {
                                LobbyScene.Instance.ChangeAuthenticationMessage(splitData[2]);
                                LobbyScene.Instance.EnableInputs();
                                Debug.LogWarning("LOGIN FAILED!");
                            }
                            break;

                        case "ON_ADD_FOLLOW":
                            if (splitData[1] == "1")
                            {
                                Net_OnAddFollow oaf = new Net_OnAddFollow();

                                oaf.Success = byte.Parse(splitData[1]);
                                oaf.Follow = new Account();
                                oaf.Follow.ActiveConnection = int.Parse(splitData[2]);
                                oaf.Follow.Status = byte.Parse(splitData[3]);
                                oaf.Follow.Username = splitData[4];
                                oaf.Follow.Discriminator = splitData[5];
                                OnAddFollow(oaf);
                            }
                            else
                            {
                                Debug.LogWarning("USER NOT FOUND!");
                            }
                            break;

                        case "ON_REQUEST_FOLLOW":
                            Debug.LogError(message);
                            int count = int.Parse(splitData[1]);
                            List<Account> followsResponse = new List<Account>();
                            for (int i = 0; i < count; i++)
                            {
                                int j = 4 * i;
                                Account newAccount = new Account();
                                newAccount.ActiveConnection = int.Parse(splitData[2 + j]); // 2 or 6 or 10 or 14
                                newAccount.Status = byte.Parse(splitData[3 + j]); // 3 or 7 or 11
                                newAccount.Username = splitData[4 + j];
                                newAccount.Discriminator = splitData[5 + j];

                                followsResponse.Add(newAccount);
                                HubScene.Instance.AddFollowToUi(newAccount);
                                Debug.Log(count + " USER ADDED!");
                            }

                            //OnRequestFollow(followsResponse);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("CLIENT RECEIVE: " + e.Message);
                }

                break;

            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type");
                break;
        }

    }

    #endregion
    #region OnData
    private void OnData(int cnnId, int channelId, int recHostId, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetOP.None:
                Debug.LogError("Unexpected NETOP");
                DebugConsole.Log("Unexpected NETOP");
                break;

            case NetOP.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)msg);
                break;

            case NetOP.OnLoginRequest:
                OnLoginRequest((Net_OnLoginRequest)msg);
                break;

            case NetOP.OnAddFollow:
                OnAddFollow((Net_OnAddFollow)msg);
                break;

            case NetOP.OnRequestFollow:
                OnRequestFollow((Net_OnRequestFollow)msg);
                break;

            case NetOP.FollowUpdate:
                FollowUpdate((Net_FollowUpdate)msg);
                break;


        }
    }

    private void OnCreateAccount(Net_OnCreateAccount oca)
    {
        LobbyScene.Instance.EnableInputs();
        LobbyScene.Instance.ChangeAuthenticationMessage(oca.Information);
    }
    private void OnLoginRequest(Net_OnLoginRequest olr)
    {
        LobbyScene.Instance.ChangeAuthenticationMessage(olr.Information);
        if (olr.Success != 1)
        {
            // Unable to login
            LobbyScene.Instance.EnableInputs();


        }
        else
        {
            //Successfull login
            // This is where we are going to save data about ourself

            self = new Account();
            self.ActiveConnection = olr.ConnectionId;
            self.Username = olr.Username;
            self.Discriminator = olr.Discriminator;

            token = olr.Token;

            UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
        }
    }
    private void OnAddFollow(Net_OnAddFollow oaf)
    {
        if (oaf.Success == 1)
            HubScene.Instance.AddFollowToUi(oaf.Follow);
    }
    private void OnRequestFollow(Net_OnRequestFollow orf)
    {
        foreach (var follow in orf.Follows)
            HubScene.Instance.AddFollowToUi(follow);
    }
    private void FollowUpdate(Net_FollowUpdate fu)
    {
        HubScene.Instance.UpdateFollow(fu.Follow);
    }
    #endregion
    #region Send

    public void SendServer(NetMsg msg)
    {
        try
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(msg.ToString());

            Debug.developerConsoleVisible = true;
            NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
        }
        catch (Exception e)
        {
            Debug.LogError("CLIENT SEND TO SERVER: " + e.Message);
        }
    }
    public void SendCreateAccount(string username, string password, string email)
    {


        if (!Utility.IsUsername(username))
        {
            // Invalid username
            LobbyScene.Instance.ChangeAuthenticationMessage("Username is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (!Utility.IsEmail(email))
        {
            // Invalid email
            LobbyScene.Instance.ChangeAuthenticationMessage("Email is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (password == null || password == "")
        {
            // Invalid password
            LobbyScene.Instance.ChangeAuthenticationMessage("Password is empty");
            LobbyScene.Instance.EnableInputs();
            return;
        }


        Net_CreateAccount ca = new Net_CreateAccount();
        ca.Username = username;
        ca.Password = Utility.Sha256FromString(password);
        ca.Email = email;

        LobbyScene.Instance.ChangeAuthenticationMessage("Sending request...");
        SendServer(ca);
        // send union server
        string message = ("|" + username + "|" + password + "|" + email + "|");
        SendServerType type = SendServerType.CREATE_ACCOUNT;

        SendUnionServer(type, message);
    }

    public void SendUnionServer(SendServerType type, string message)
    {
        try
        {

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes((type.ToString() + message).ToString());

            Debug.developerConsoleVisible = true;
            NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
        }
        catch (Exception e)
        {
            Debug.LogError("CLIENT SEND TO SERVER: " + e.Message);
        }
    }
    public void SendLoginRequest(string usernameOrEmail, string password)
    {
        if (!Utility.IsUsernameAndDiscriminator(usernameOrEmail) && !Utility.IsEmail(usernameOrEmail))
        {
            // Invalid username or email
            LobbyScene.Instance.ChangeAuthenticationMessage("Email or Username#Discriminator is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (password == null || password == "")
        {
            // Invalid password
            LobbyScene.Instance.ChangeAuthenticationMessage("Password is empty");
            LobbyScene.Instance.EnableInputs();
            return;
        }


        Net_LoginRequest lr = new Net_LoginRequest();

        lr.UsernameOrEmail = usernameOrEmail;
        lr.Password = Utility.Sha256FromString(password);

        LobbyScene.Instance.ChangeAuthenticationMessage("Sending login request...");
        SendServer(lr);


        string message = ("|" + usernameOrEmail + "|" + password + "|");
        SendServerType type = SendServerType.LOGIN_REQUEST;

        SendUnionServer(type, message);
    }

    public void SendAddFollow(string usernameOrEmail)
    {
        Net_AddFollow af = new Net_AddFollow();

        af.Token = token;
        af.UsernameDiscriminatorOrEmail = usernameOrEmail;
        //SendServer(af);

        string message = ("|" + af.Token + "|" + af.UsernameDiscriminatorOrEmail + "|");
        SendServerType type = SendServerType.ADD_FOLLOW;
        SendUnionServer(type, message);
    }
    public void SendRemoveFollow(string username)
    {
        Net_RemoveFollow rf = new Net_RemoveFollow();

        rf.Token = token;
        rf.UsernameDiscriminator = username;

        //SendServer(rf);
        string message = ("|" + rf.Token + "|" + rf.UsernameDiscriminator + "|");
        SendServerType type = SendServerType.REMOVE_FOLLOW;
        SendUnionServer(type, message);
    }
    public void SendRequestFollow()
    {
        Net_RequestFollow rf = new Net_RequestFollow();
        rf.Token = token;

        //SendServer(rf);
        string message = ("|" + rf.Token + "|");
        SendServerType type = SendServerType.REQUEST_FOLLOW;
        SendUnionServer(type, message);
    }

    public void SendHomeSetup() // x1:y1|x2:y2|x3:y3
    {
        Net_HomeSetup hs = new Net_HomeSetup();

        hs.Token = token;
        hs.item1PosX = GameObject.Find("Items/item1").transform.position.x;
        hs.item1PosY = GameObject.Find("Items/item1").transform.position.y;
        hs.item2PosX = GameObject.Find("Items/item2").transform.position.x;
        hs.item2PosY = GameObject.Find("Items/item2").transform.position.y;
        hs.item3PosX = GameObject.Find("Items/item3").transform.position.x;
        hs.item3PosY = GameObject.Find("Items/item3").transform.position.y;

        Debug.LogError(hs.item1PosX + " " + hs.item1PosY);
        Debug.LogError(hs.item2PosX + " " + hs.item2PosY);
        Debug.LogError(hs.item3PosX + " " + hs.item3PosY);
        //string message = ("|" + hs.Token + "|" + af.UsernameDiscriminatorOrEmail + "|");
        //SendServerType type = SendServerType.HOME_SETUP;
        //SendUnionServer(type, message);
    }
    #endregion




}