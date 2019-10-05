using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void PlayerStateChanged(int netCode, string name, int currentValue);
public class PlayerState : MonoBehaviour
{
    private static List<PlayerState> playerStateList = new List<PlayerState>();
    public event PlayerStateChanged PlayerNetCodeChangedEvent;
    public event PlayerStateChanged PlayerScoreChangedEvent;
    public int netCode = -1;
    public string name;
    private int _score;
    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            PlayerScoreChangedEvent?.Invoke(netCode, name, _score);
        }
    }

    private void Awake()
    {
        Debug.Log("PlayerState Awake");
        playerStateList.Add(this);
    }

    public static PlayerState GetPlayerStateByIndex(int index)
    {
        return playerStateList[index];
    }

    public static PlayerState GetPlayerStateByNetCode(int netCode)
    {
        PlayerState resultState = null;
        foreach(PlayerState tempState in playerStateList)
        {
            if(tempState.netCode == netCode)
            {
                resultState = tempState;
                break;
            }
        }
        return resultState;
    }

    public static PlayerState GetLocalPlayerState()
    {
        PlayerState resultState = null;
        foreach(PlayerState tempState in playerStateList)
        {
            if (Client.ins.IsLocalPlayer(tempState.netCode))
            {
                resultState = tempState;
                break;
            }
        }
        return resultState;
    }
}
