using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInput:MonoBehaviour
{
    private int playerNo;
    private Dictionary<string, float> axisNameScalePairs = new Dictionary<string, float>();
    private const int KEY_STATE_KEYUP = 0;
    private const int KEY_STATE_KEYDOWN = 1;
    private const int KEY_STATE_KEY = 2;
    /// <summary>
    /// state => 0:keyUp 1:keyDown 2:key
    /// </summary>
    private Dictionary<KeyCode, int> keyCodeStatePairs = new Dictionary<KeyCode, int>();

    private void Start()
    {
        Client.ins.PlayerNoChangedEvent += OnPlayerNoChangedEvent;
        Client.ins.NetInputAxisEvent += OnNetInputAxisEvent;
        Client.ins.NetKeyUpEvent += OnNetKeyUpEvent;
        Client.ins.NetKeyDownEvent += OnNetKeyDownEvent;
        Client.ins.NetKeyEvent += OnNetKeyEvent;
    }

    private void OnPlayerNoChangedEvent()
    {
        playerNo = Client.ins.playerNo;
    }

    public float GetAxis(string axisName)
    {
        float resultScale = 0;
        axisNameScalePairs.TryGetValue(axisName, out resultScale);
        return resultScale;
    }

    private void SetAxis(string axisName, float axisScale)
    {
        if (axisNameScalePairs.ContainsKey(axisName))
        {
            axisNameScalePairs[axisName] = axisScale;
        }
        else
        {
            axisNameScalePairs.Add(axisName, axisScale);
        }
    }

    private void OnNetInputAxisEvent(int playerNo, string axisName, float axisScale)
    {
        if (playerNo == this.playerNo)
        {
            SetAxis(axisName, axisScale);
        }
    }

    private int GetKeyState(KeyCode key)
    {
        int keyCodeState = -1;
        keyCodeStatePairs.TryGetValue(key, out keyCodeState);
        return keyCodeState;
    }

    public bool GetKeyDown(KeyCode key)
    {
        return GetKeyState(key) == KEY_STATE_KEYDOWN;
    }

    public bool GetKeyUp(KeyCode key)
    {
        bool resultBool = GetKeyState(key) == KEY_STATE_KEYUP;
        SetKeyState(key, KEY_STATE_KEYUP);
        return resultBool;
    }

    public bool GetKey(KeyCode key)
    {
        return GetKeyState(key) == KEY_STATE_KEY;
    }

    private void SetKeyState(KeyCode keyCode, int keyState)
    {
        if (keyCodeStatePairs.ContainsKey(keyCode))
        {
            keyCodeStatePairs[keyCode] = keyState;
        }
        else
        {
            keyCodeStatePairs.Add(keyCode, keyState);
        }
    }

    private void OnNetKeyUpEvent(int playerNo, KeyCode key)
    {
        if(playerNo == this.playerNo) SetKeyState(key, KEY_STATE_KEYUP);
    }

    private void OnNetKeyDownEvent(int playerNo, KeyCode key)
    {
        if(playerNo == this.playerNo && !GetKey(key)) SetKeyState(key, KEY_STATE_KEYDOWN);
    }

    private void OnNetKeyEvent(int playerNo, KeyCode key)
    {
        if (playerNo == this.playerNo) SetKeyState(key, KEY_STATE_KEY);
    }
}
