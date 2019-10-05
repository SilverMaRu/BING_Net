using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    public static Server ins { get; private set; }
    private const int EMPTY_HOST_ID = -1;
    private const int EMPTY_CONNECTION_ID = 0;
    private const int EMPTY_NETCODE = -1;
    public bool isServer = true;
    public int serverPort = 8888;
    public int maxPlayerCount = 4;
    private GameObject[] playerGOs { get { return GameControler.ins.playerGOs; } }
    private List<int> connIDList = new List<int>();
    private List<int> netCodeList = new List<int>();
    private int currentConnCount = 0;
    //同步位置评率
    public float synchronizeFrequency = 0.2f;
    private float lastSynchronizeTime = 0;

    private int hostID = EMPTY_HOST_ID;
    private int reliableChannelID = 0;
    private int unreliableChannelID = 0;
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
        // 初始化分配玩家编号
        for (int i = 0; i < maxPlayerCount; i++)
        {
            netCodeList.Add(i);
            connIDList.Add(EMPTY_CONNECTION_ID);
        }
        //playerGOs = Client.ins.playerGOs;
    }

    // Update is called once per frame
    void Update()
    {
        if (hostID <= EMPTY_HOST_ID)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                isServer = true;
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                isServer = false;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isServer)
                {
                    NetworkTransport.Init();
                    ConnectionConfig connConfig = new ConnectionConfig();
                    reliableChannelID = connConfig.AddChannel(QosType.Reliable);
                    unreliableChannelID = connConfig.AddChannel(QosType.Unreliable);
                    HostTopology hostTopology = new HostTopology(connConfig, maxPlayerCount);
                    hostID = NetworkTransport.AddHost(hostTopology, serverPort);
                    recBuffer = new byte[recBufferSize];
                }
                else
                {
                    enabled = false;
                }
            }
        }
        else
        {
            NetworkEventType eventType = NetworkTransport.ReceiveFromHost(hostID, out recConnectionID, out channelID, recBuffer, recBufferSize, out receivedSize, out error);
            switch (eventType)
            {
                case NetworkEventType.ConnectEvent:
                    int netCode = EMPTY_NETCODE;
                    if (TryPortionNetCode(recConnectionID, out netCode))
                    {
                        currentConnCount++;
                        // 通知新玩家设置自己的编号
                        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "SetPlayerNo", netCode);
                        NetworkTransport.Send(hostID, recConnectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);

                        // 通知所有玩家设置各信息
                        List<object> synPlayerInfoDataList = new List<object>();
                        synPlayerInfoDataList.Add("SynchronizePlayerInfo");
                        for (int i = 0; i < currentConnCount; i++)
                        {
                            synPlayerInfoDataList.Add(netCodeList[i]);
                        }
                        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, synPlayerInfoDataList.ToArray());
                        SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);

                        //// 通知所有玩家同步位置
                        //SynchronizeAllPlayerTransform();
                        Debug.Log("有新客户端连接,当前连接数 " + currentConnCount + "/" + maxPlayerCount);
                    }
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("接收到客户端的新数据");
                    ExecuteInstruction(recConnectionID, NetworkCommunicateHelper.UnPack(recBuffer));
                    break;
                case NetworkEventType.DisconnectEvent:
                    int recyclePlayerNo = EMPTY_NETCODE;
                    TryRecycleNetCode(recConnectionID, out recyclePlayerNo);
                    Debug.Log("有客户端断开连接,当前连接数 " + currentConnCount + "/" + maxPlayerCount);
                    break;
                case NetworkEventType.Nothing:
                    Debug.Log("无数据");
                    break;
                default:
                    break;
            }

            //if (Time.time - lastSynchronizeTime > synchronizeFrequency)
            //{
            //    // 通知所有玩家同步位置
            //    SynchronizeAllPlayerTransform();
            //    lastSynchronizeTime = Time.time;
            //}
        }
    }

    public bool HasVacancy()
    {
        bool hasVacancy = false;
        hasVacancy = connIDList.Exists(t => t == EMPTY_CONNECTION_ID);
        return hasVacancy;
    }

    public bool TryPortionNetCode(int connectionID, out int portionNetCode)
    {
        bool isSuccess = false;
        portionNetCode = EMPTY_NETCODE;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (connIDList[i] <= EMPTY_CONNECTION_ID)
            {
                portionNetCode = netCodeList[i];
                connIDList[i] = connectionID;
                //currentConnCount++;
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    public bool TryRecycleNetCode(int connectionID, out int recycleNetCode)
    {
        bool isSuccess = false;
        recycleNetCode = EMPTY_NETCODE;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (connIDList[i] == connectionID)
            {
                recycleNetCode = netCodeList[i];
                connIDList[i] = EMPTY_CONNECTION_ID;
                //currentConnCount--;
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    public bool TryGetConnectionIDByNetCode(int netCode, out int connectionID)
    {
        bool isSuccess = false;
        connectionID = EMPTY_CONNECTION_ID;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (netCodeList[i] == netCode)
            //if (playerInfoList[i].netCode == playerNo)
            {
                connectionID = connIDList[i];
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    public bool TryGetNetCodeByConnectionID(int connectionID, out int netCode)
    {
        bool isSuccess = false;
        netCode = EMPTY_NETCODE;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (connIDList[i] == connectionID)
            {
                netCode = netCodeList[i];
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    // 通知所有玩家哪两位玩家进行身份调换
    public void SwitchFaction(AttributesManager originalGhostAttrManager, AttributesManager originalPeopleAttrManager)
    {
        int originalGhostPlayerNo = EMPTY_NETCODE;
        int originalPeoplePlayerNo = EMPTY_NETCODE;
        if (Client.ins.TryGetNetCodeByPlayerGO(originalGhostAttrManager.gameObject, out originalGhostPlayerNo)
            && Client.ins.TryGetNetCodeByPlayerGO(originalPeopleAttrManager.gameObject, out originalPeoplePlayerNo))
        {
            // 通知交换身份
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "SwitchFaction", originalGhostPlayerNo, originalPeoplePlayerNo);
            SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private bool SendToEveryPlayer(int hostId, int channelId, byte[] buffer, int size, out byte error)
    {
        bool resultBool = true;
        error = 0;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            int connectionID = connIDList[i];
            if (connectionID > EMPTY_CONNECTION_ID)
            {
                resultBool &= NetworkTransport.Send(hostId, connectionID, channelId, buffer, size, out error);
                if (!resultBool) break;
            }
        }
        return resultBool;
    }

    private void ExecuteInstruction(int recConnectionID, object[] unPackDatas)
    {
        if (unPackDatas != null)
        {
            string methodName = (string)unPackDatas[0];
            switch (methodName)
            {
                case "SynchronizePlayerInfoDone":
                    SynchronizeAllPlayerTransform();
                    break;
                case "GetKeyDown":
                    CaseGetKeyDown(recConnectionID, unPackDatas);
                    break;
                case "GetKeyUp":
                    CaseGetKeyUp(recConnectionID, unPackDatas);
                    break;
                case "GetKey":
                    CaseGetKey(recConnectionID, unPackDatas);
                    break;
                case "GetAxis":
                    CaseGetAxis(recConnectionID, unPackDatas);
                    break;
                case "InputDirection":
                    CaseInputDirection(recConnectionID, unPackDatas);
                    break;
                default:
                    break;
            }
        }
    }

    private void SynchronizeAllPlayerTransform()
    {
        //Debug.Log("playerGOs.Length = " + playerGOs.Length);
        List<object> synTransformDataList = new List<object>();
        synTransformDataList.Add("SynchronizePlayerTransform");
        for (int i = 0; i < currentConnCount; i++)
        {
            int netCode = netCodeList[i];
            synTransformDataList.Add(netCode);
            GameObject playerGO = Client.ins.GetPlayerGOByNetCode(netCode);
            synTransformDataList.Add(playerGO.transform.position);
            synTransformDataList.Add(playerGO.transform.rotation);
        }
        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, synTransformDataList.ToArray());
        SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
    }

    private void CaseGetKeyDown(int recConnectionID, object[] unPackDatas)
    {
        //SynchronizeAllPlayerTransform();
        int playerNo = EMPTY_NETCODE;
        if (TryGetNetCodeByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(unPackDatas, 1, playerNo));
            SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void CaseGetKeyUp(int recConnectionID, object[] unPackDatas)
    {
        //SynchronizeAllPlayerTransform();
        int playerNo = EMPTY_NETCODE;
        if (TryGetNetCodeByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(unPackDatas, 1, playerNo));
            SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void CaseGetKey(int recConnectionID, object[] unPackDatas)
    {
        //SynchronizeAllPlayerTransform();
        int playerNo = EMPTY_NETCODE;
        if (TryGetNetCodeByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(unPackDatas, 1, playerNo));
            SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void CaseGetAxis(int recConnectionID, object[] unPackDatas)
    {
        int playerNo = EMPTY_NETCODE;
        if (TryGetNetCodeByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(unPackDatas, 1, playerNo));
            SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
        //SynchronizeAllPlayerTransform();
    }

    private void CaseInputDirection(int recConnectionID, object[] unPackDatas)
    {
        int playerNo = EMPTY_NETCODE;
        if (TryGetNetCodeByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(unPackDatas, 1, playerNo));
            SendToEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
        //SynchronizeAllPlayerTransform();
    }
}
