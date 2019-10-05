using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{
    public GameObject bindPlayer;
    public Text pointValueText;

    private PlayerState bindPlayerState;

    // Start is called before the first frame update
    void Start()
    {
        Client.ins.SpawnedLocalPlayerEvent += OnSpawnedLocalPlayer;
    }

    private void OnSpawnedLocalPlayer()
    {
        bindPlayer = Client.ins.GetLocalPlayerGO();
        bindPlayerState = bindPlayer.GetComponent<PlayerState>();
        bindPlayerState.PlayerScoreChangedEvent += OnPlayerScoreChanged;
    }

    private void OnPlayerScoreChanged(int netCode, string name, int currentValue)
    {
        pointValueText.text = currentValue.ToString();
    }
}
