using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class Client : MonoBehaviour
{
    public static Client ins { get; private set; }
    public event Action PlayerNoChangedEvent;
    /// <summary>
    /// arg1:playerNo
    /// arg2:axisName
    /// arg3:axisScale
    /// </summary>
    public event Action<int, string, float> NetInputAxisEvent;
    /// <summary>
    /// arg1:playerNo
    /// arg2:KeyCode
    /// </summary>
    public event Action<int, KeyCode> NetKeyDownEvent;
    /// <summary>
    /// arg1:playerNo
    /// arg2:KeyCode
    /// </summary>
    public event Action<int, KeyCode> NetKeyUpEvent;
    /// <summary>
    /// arg1:playerNo
    /// arg2:KeyCode
    /// </summary>
    public event Action<int, KeyCode> NetKeyEvent;
    private const int EMPTY_HOST_ID = -1;
    private const int EMPTY_CONNECTION_ID = -1;
    private Dictionary<string, float> axisLastScalePair = new Dictionary<string, float>();

    public string serverIP = "127.0.0.1";
    public int serverPort = 8888;
    private int _playerNo;
    public int playerNo
    {
        get
        {
            return _playerNo;
        }
        set
        {
            _playerNo = value;
            PlayerNoChangedEvent?.Invoke();
        }
    }

    public GameObject[] playerGOs;

    private int hostID = EMPTY_HOST_ID;
    private int connectionID = EMPTY_CONNECTION_ID;
    private int unreliableChannelID = 0;
    private int reliableChannelID = 0;
    private byte[] recBuffer;
    private int recBufferSize = 1024;

    private int recConnectionID = 0;
    private int channelID = 0;
    private int receivedSize = 0;

    private byte[] sendBuffer;
    private int sendBufferSize;

    private byte error = 0;

    private void Awake()
    {
        ins = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (hostID <= EMPTY_HOST_ID)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                NetworkTransport.Init();
                ConnectionConfig connConfig = new ConnectionConfig();
                reliableChannelID = connConfig.AddChannel(QosType.Reliable);
                unreliableChannelID = connConfig.AddChannel(QosType.Unreliable);
                HostTopology hostTopology = new HostTopology(connConfig, 1);
                hostID = NetworkTransport.AddHost(hostTopology);
                connectionID = NetworkTransport.Connect(hostID, serverIP, serverPort, 0, out error);
                recBuffer = new byte[recBufferSize];
            }
        }
        else
        {
            NetworkEventType eventType = NetworkTransport.ReceiveFromHost(hostID, out recConnectionID, out channelID, recBuffer, recBufferSize, out receivedSize, out error);
            switch (eventType)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("成功连接到服务器");
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("接收到服务器的新数据");
                    ExecuteInstruction(NetworkCommunicateHelper.UnPack(recBuffer));
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("有客户端断开连接");
                    break;
                case NetworkEventType.Nothing:
                    Debug.Log("无数据");
                    break;
                default:
                    break;
            }

            SendAxis("H");
            SendAxis("V");
            SendKey(KeyCode.Mouse0);
            SendKey(KeyCode.Mouse1);
            SendKey(KeyCode.LeftShift);
            SendKey(KeyCode.Space);
            SendKey(KeyCode.B);
        }
    }

    private void SendAxis(string axisName)
    {
        float axisScale = Input.GetAxis(axisName);
        Debug.Log("axisName = " + axisName + ";axisScale = " + axisScale);
        float axisLastScale = 0;
        bool tryGetSuccess = axisLastScalePair.TryGetValue(axisName, out axisLastScale);

        if (!tryGetSuccess)
        {
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetAxis", axisName, axisScale);
            NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
            axisLastScalePair.Add(axisName, axisScale);
        }
        else if (axisLastScale != axisScale)
        {
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetAxis", axisName, axisScale);
            NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
            axisLastScalePair[axisName] = axisScale;
        }
    }

    private void SendKey(KeyCode key)
    {
        if (Input.GetKeyUp(key))
        {
            Debug.Log("Enum.GetName(key.GetType(), key) = " + Enum.GetName(key.GetType(), key));
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKeyUp", Enum.GetName(key.GetType(), key));
            NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
        else if (Input.GetKeyDown(key))
        {
            Debug.Log("Enum.GetName(key.GetType(), key) = " + Enum.GetName(key.GetType(), key));
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKeyDown", Enum.GetName(key.GetType(), key));
            NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
        else if (Input.GetKey(key))
        {
            Debug.Log("Enum.GetName(key.GetType(), key) = " + Enum.GetName(key.GetType(), key));
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKey", Enum.GetName(key.GetType(), key));
            NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void ExecuteInstruction(object[] unPackDatas)
    {
        if (unPackDatas != null && unPackDatas.Length > 0)
        {
            string methodName = (string)unPackDatas[0];
            switch (methodName)
            {
                case "SetPlayerNo":
                    CaseSetPlayerNo(unPackDatas);
                    break;
                case "GetKeyDown":
                    CaseGetKeyDown(unPackDatas);
                    break;
                case "GetKeyUp":
                    CaseGetKeyUp(unPackDatas);
                    break;
                case "GetKey":
                    CaseGetKey(unPackDatas);
                    break;
                case "GetAxis":
                    CaseGetAxis(unPackDatas);
                    break;
                case "SynchronizePlayerPosition":
                    CaseSynchronizePlayerPosition(unPackDatas);
                    break;
                default:
                    break;
            }
        }
    }

    private void CaseSetPlayerNo(object[] unPackDatas)
    {
        playerNo = (int)unPackDatas[1];
    }

    private void CaseGetKeyDown(object[] unPackDatas)
    {
        int playerNo = (int)unPackDatas[1];
        string keyCodeName = (string)unPackDatas[2];
        //Transform playerGOTrans = playerGOs[playerNo - 1].transform;
        //switch (keyCodeName)
        //{
        //    case "W":
        //        playerGOTrans.position += playerGOTrans.forward * 5 * Time.deltaTime;
        //        break;
        //    case "A":
        //        playerGOTrans.position -= playerGOTrans.right * 5 * Time.deltaTime;
        //        break;
        //    case "S":
        //        playerGOTrans.position -= playerGOTrans.forward * 5 * Time.deltaTime;
        //        break;
        //    case "D":
        //        playerGOTrans.position += playerGOTrans.right * 5 * Time.deltaTime;
        //        break;
        //    default:
        //        break;
        //}
        NetKeyDownEvent?.Invoke(playerNo, (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeName));
    }

    private void CaseGetKeyUp(object[] unPackDatas)
    {
        int playerNo = (int)unPackDatas[1];
        string keyCodeName = (string)unPackDatas[2];
        NetKeyUpEvent?.Invoke(playerNo, (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeName));
    }

    private void CaseGetKey(object[] unPackDatas)
    {
        int playerNo = (int)unPackDatas[1];
        string keyCodeName = (string)unPackDatas[2];
        NetKeyEvent?.Invoke(playerNo, (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeName));
    }

    private void CaseGetAxis(object[] unPackDatas)
    {
        int playerNo = (int)unPackDatas[1];
        string axisName = (string)unPackDatas[2];
        float axisScale = (float)unPackDatas[3];
        NetInputAxisEvent?.Invoke(playerNo, axisName, axisScale);
    }

    private void CaseSynchronizePlayerPosition(object[] unPackDatas)
    {
        for (int i = 1; i < unPackDatas.Length; i += 2)
        {
            int playerNo = (int)unPackDatas[i];
            Vector3 position = (Vector3)unPackDatas[i + 1];
            playerGOs[playerNo - 1].transform.position = position;
        }
    }
}
