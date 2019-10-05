using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class Client : MonoBehaviour
{
    public static Client ins { get; private set; }
    public event Action NetCodeChangedEvent;
    public event Action SpawnedLocalPlayerEvent;
    /// <summary>
    /// arg1 spawnPlayerNetCode
    /// </summary>
    public event Action<int> SpawnedPlayerEvent;
    /// <summary>
    /// arg1:NetCode
    /// arg2:axisName
    /// arg3:axisScale
    /// </summary>
    public event Action<int, string, float> NetInputAxisEvent;
    /// <summary>
    /// arg1:NetCode
    /// arg2:inputDirection
    /// </summary>
    public event Action<int, Vector3> NetInputDirectionEvent;
    /// <summary>
    /// arg1:NetCode
    /// arg2:KeyCode
    /// </summary>
    public event Action<int, KeyCode> NetKeyDownEvent;
    /// <summary>
    /// arg1:NetCode
    /// arg2:KeyCode
    /// </summary>
    public event Action<int, KeyCode> NetKeyUpEvent;
    /// <summary>
    /// arg1:NetCode
    /// arg2:KeyCode
    /// </summary>
    public event Action<int, KeyCode> NetKeyEvent;
    private const int EMPTY_HOST_ID = -1;
    private const int EMPTY_CONNECTION_ID = -1;
    private Dictionary<string, float> axisLastScalePair = new Dictionary<string, float>();
    private Vector3 lastInputDirection = Vector3.zero;

    public string serverIP = "127.0.0.1";
    public int serverPort = 8888;
    private int _netCode = -1;
    public int netCode
    {
        get
        {
            return _netCode;
            //return localPlayerInfo.netCode;
        }
        private set
        {
            _netCode = value;
            NetCodeChangedEvent?.Invoke();
        }
    }

    //public GameObject[] playerGOs;
    public GameObject[] playerGOs { get { return GameControler.ins.playerGOs; } }

    // 标识是否等待连接服务器的新玩家
    private bool waitingJoin = false;
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
                waitingJoin = true;
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
            SendInputDirection();
            SendKey(KeyCode.Mouse0);
            SendKey(KeyCode.Mouse1);
            SendKey(KeyCode.LeftShift);
            SendKey(KeyCode.Space);
            SendKey(KeyCode.B);
        }
    }

    public bool TryGetNetCodeByPlayerGO(GameObject playerGO, out int netCode)
    {
        bool isHas = false;
        //playerNo = 0;
        netCode = -1;
        for (int i = 0; i < playerGOs.Length; i++)
        {
            if (playerGOs[i].Equals(playerGO))
            {
                isHas = true;
                //playerNo = i + 1;
                netCode = i;
                break;
            }
        }
        return isHas;
    }

    public GameObject GetPlayerGOByNetCode(int netCode)
    {
        return PlayerState.GetPlayerStateByNetCode(netCode)?.gameObject;
    }

    public bool IsLocalPlayer(int netCode)
    {
        return netCode == this.netCode;
    }

    public GameObject GetLocalPlayerGO()
    {
        return GetPlayerGOByNetCode(netCode);
    }

    private void SendAxis(string axisName)
    {
        float axisScale = Input.GetAxis(axisName);
        //Debug.Log("axisName = " + axisName + ";axisScale = " + axisScale);
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

    private void SendInputDirection()
    {
        Vector3 inputDirection = (Input.GetAxis("V") * CameraCtrl.rigTrans.forward + Input.GetAxis("H") * CameraCtrl.cameraTrans.right);
        if (lastInputDirection != inputDirection)
        {
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "InputDirection", inputDirection);
            NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
            lastInputDirection = inputDirection;
        }
        //sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "InputDirection", inputDirection);
        //NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
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
                case "SynchronizePlayerInfo":
                    CaseSynchronizePlayerInfo(unPackDatas);
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
                case "InputDirection":
                    CaseInputDirection(unPackDatas);
                    break;
                case "SynchronizePlayerTransform":
                    CaseSynchronizePlayerTransform(unPackDatas);
                    break;
                case "SwitchFaction":
                    CaseSwitchFaction(unPackDatas);
                    break;
                default:
                    break;
            }
        }
    }

    private void CaseSetPlayerNo(object[] unPackDatas)
    {
        netCode = (int)unPackDatas[1];
        GameControler.ins.SpawnPlayer(netCode, netCode.ToString());
        SpawnedLocalPlayerEvent?.Invoke();
    }

    private void CaseSynchronizePlayerInfo(object[] unPackDatas)
    {
        for (int i = 1; i < unPackDatas.Length; i++)
        {
            int netCode = (int)unPackDatas[i];
            if (GetPlayerGOByNetCode(netCode) == null)
                GameControler.ins.SpawnPlayer(netCode, netCode.ToString());
        }
        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "SynchronizePlayerInfoDone");
        NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
    }

    private void CaseGetKeyDown(object[] unPackDatas)
    {
        int playerNo = (int)unPackDatas[1];
        string keyCodeName = (string)unPackDatas[2];
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

    private void CaseInputDirection(object[] unPackDatas)
    {
        int playerNo = (int)unPackDatas[1];
        Vector3 inputDirection = (Vector3)unPackDatas[2];
        NetInputDirectionEvent?.Invoke(playerNo, inputDirection);
    }

    private void CaseSynchronizePlayerTransform(object[] unPackDatas)
    {
        for (int i = 1; i < unPackDatas.Length; i += 3)
        {
            int netCode = (int)unPackDatas[i];
            Transform transform = GetPlayerGOByNetCode(netCode).transform;
            transform.position = (Vector3)unPackDatas[i + 1];
            transform.rotation = (Quaternion)unPackDatas[i + 2];
        }
    }

    private void CaseSwitchFaction(object[] unPackDatas)
    {
        int originalGhostPlayerNo = (int)unPackDatas[1];
        int originalPeoplePlayerNo = (int)unPackDatas[2];
        AttributesManager originalGhostAttr = GetPlayerGOByNetCode(originalGhostPlayerNo).GetComponent<AttributesManager>();
        AttributesManager originalPeopleAttr = GetPlayerGOByNetCode(originalPeoplePlayerNo).GetComponent<AttributesManager>();
        GameControler.ins.SwitchFaction(originalGhostAttr, originalPeopleAttr);
    }
}
