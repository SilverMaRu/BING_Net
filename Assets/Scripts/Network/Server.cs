using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private const int EMPTY_HOST_ID = -1;
    private const int EMPTY_CONNECTION_ID = 0;
    private const int EMPTY_PLAYER_NO = 0;
    public bool isServer = true;
    public int serverPort = 8888;
    public int maxPlayerCount = 4;
    public GameObject[] playerGOs;
    private List<int> playerNoList = new List<int>();
    private List<int> connIDList = new List<int>();
    private int currentConnCount = 0;

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

    // Start is called before the first frame update
    void Start()
    {
        // 初始化分配玩家编号
        for (int i = 0; i < maxPlayerCount; i++)
        {
            playerNoList.Add(i + 1);
            connIDList.Add(EMPTY_CONNECTION_ID);
        }
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
            if (isServer && Input.GetKeyDown(KeyCode.C))
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
                    int portionPlayerNo = EMPTY_PLAYER_NO;
                    if (TryPortionPlayerNo(recConnectionID, out portionPlayerNo))
                    {
                        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "SetPlayerNo", portionPlayerNo);
                        NetworkTransport.Send(hostID, recConnectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
                        
                        object[] datas = new object[maxPlayerCount * 2 + 1];
                        datas[0] = "SynchronizePlayerPosition";
                        for (int i = 1; i < datas.Length; i += 2)
                        {
                            int j = (i - 1) / 2;
                            datas[i] = j + 1;
                            datas[i + 1] = playerGOs[j].transform.position;
                        }
                        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, datas);
                        NetworkTransport.Send(hostID, recConnectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
                    }
                    Debug.Log("有新客户端连接,当前连接数 " + currentConnCount + "/" + maxPlayerCount);
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("接收到客户端的新数据");
                    ExecuteInstruction(recConnectionID, NetworkCommunicateHelper.UnPack(recBuffer));
                    break;
                case NetworkEventType.DisconnectEvent:
                    int recyclePlayerNo = EMPTY_PLAYER_NO;
                    TryRecyclePlayerNo(recConnectionID, out recyclePlayerNo);
                    Debug.Log("有客户端断开连接,当前连接数 " + currentConnCount + "/" + maxPlayerCount);
                    break;
                case NetworkEventType.Nothing:
                    Debug.Log("无数据");
                    break;
                default:
                    break;
            }
        }
    }

    public bool TryPortionPlayerNo(int connectionID, out int portionPlayerNo)
    {
        bool isSuccess = false;
        portionPlayerNo = EMPTY_PLAYER_NO;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (connIDList[i] <= EMPTY_CONNECTION_ID)
            {
                portionPlayerNo = playerNoList[i];
                connIDList[i] = connectionID;
                currentConnCount++;
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    public bool TryRecyclePlayerNo(int connectionID, out int recyclePlayerNo)
    {
        bool isSuccess = false;
        recyclePlayerNo = EMPTY_PLAYER_NO;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (connIDList[i] == connectionID)
            {
                recyclePlayerNo = playerNoList[i];
                connIDList[i] = EMPTY_CONNECTION_ID;
                currentConnCount--;
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    public bool TryGetConnectionIDByPlayerNo(int playerNo, out int connectionID)
    {
        bool isSuccess = false;
        connectionID = EMPTY_CONNECTION_ID;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (playerNoList[i] == playerNo)
            {
                connectionID = connIDList[i];
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    public bool TryGetPlayerNoByConnectionID(int connectionID, out int playerNo)
    {
        bool isSuccess = false;
        playerNo = EMPTY_PLAYER_NO;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            if (connIDList[i] == connectionID)
            {
                playerNo = playerNoList[i];
                isSuccess = true;
                break;
            }
        }
        return isSuccess;
    }

    private bool SendEveryPlayer(int hostId, int channelId, byte[] buffer, int size, out byte error)
    {
        bool resultBool = true;
        error = 0;
        for (int i = 0; i < maxPlayerCount; i++)
        {
            int connectionID = connIDList[i];
            if (connectionID > EMPTY_CONNECTION_ID)
            {
                resultBool &= NetworkTransport.Send(hostId, connectionID, channelId, buffer, size, out error);
                if(!resultBool) break;
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
                default:
                    break;
            }
        }
    }

    private void CaseGetKeyDown(int recConnectionID, object[] datas)
    {
        int playerNo = EMPTY_PLAYER_NO;
        if (TryGetPlayerNoByConnectionID(recConnectionID, out playerNo))
        {
            //for (int i = 0; i < maxPlayerCount; i++)
            //{
            //    int connectionID = connIDList[i];
            //    if (connectionID > EMPTY_CONNECTION_ID)
            //    {
            //        sendBufferSize = 0;
            //        sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKeyDown", playerNo, datas[1]);
            //        NetworkTransport.Send(hostID, connectionID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
            //    }
            //}
            sendBufferSize = 0;
            //sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKeyDown", playerNo, datas[1]);
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(datas, 1, playerNo));
            SendEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void CaseGetKeyUp(int recConnectionID, object[] datas)
    {
        int playerNo = EMPTY_PLAYER_NO;
        if (TryGetPlayerNoByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            //sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKeyUp", playerNo, datas[1]);
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(datas, 1, playerNo));
            SendEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void CaseGetKey(int recConnectionID, object[] datas)
    {
        int playerNo = EMPTY_PLAYER_NO;
        if (TryGetPlayerNoByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            //sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetKey", playerNo, datas[1]);
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(datas, 1, playerNo));
            SendEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }

    private void CaseGetAxis(int recConnectionID, object[] datas)
    {
        int playerNo = EMPTY_PLAYER_NO;
        if (TryGetPlayerNoByConnectionID(recConnectionID, out playerNo))
        {
            sendBufferSize = 0;
            //sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, "GetAxis", playerNo, datas[1]);
            sendBuffer = NetworkCommunicateHelper.ToPack(out sendBufferSize, Tool.Insert(datas, 1, playerNo));
            SendEveryPlayer(hostID, unreliableChannelID, sendBuffer, sendBufferSize, out error);
        }
    }
}
