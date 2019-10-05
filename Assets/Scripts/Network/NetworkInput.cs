using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInput : MonoBehaviour
{
    public int netCode = -1;
    public Vector3 inputDirection { get; private set; }
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
        //Client.ins.NetCodeChangedEvent += OnPlayerNoChangedEvent;
        netCode = GetComponent<PlayerState>().netCode;
        Client.ins.NetInputAxisEvent += OnNetInputAxis;
        Client.ins.NetInputDirectionEvent += OnNetInputDirection;
        Client.ins.NetKeyUpEvent += OnNetKeyUp;
        Client.ins.NetKeyDownEvent += OnNetKeyDown;
        Client.ins.NetKeyEvent += OnNetKey;
    }

    private void OnNetInputDirection(int netCode, Vector3 inputDirection)
    {
        if (netCode == this.netCode) this.inputDirection = inputDirection;
    }

    private void OnPlayerNoChangedEvent()
    {
        netCode = Client.ins.netCode;
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

    private void OnNetInputAxis(int netCode, string axisName, float axisScale)
    {
        if (netCode == this.netCode)
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

    private void OnNetKeyUp(int netCode, KeyCode key)
    {
        if (netCode == this.netCode) SetKeyState(key, KEY_STATE_KEYUP);
    }

    private void OnNetKeyDown(int netCode, KeyCode key)
    {
        if (netCode == this.netCode && !GetKey(key)) SetKeyState(key, KEY_STATE_KEYDOWN);
    }

    private void OnNetKey(int netCode, KeyCode key)
    {
        if (netCode == this.netCode) SetKeyState(key, KEY_STATE_KEY);
    }
}
