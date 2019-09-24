using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{
    public GameObject bindPlayer;
    public Text pointValueText;

    // Start is called before the first frame update
    void Start()
    {
        GameControler.ins.PlayerPointChangedEvent += OnPlayerPointChanged;
    }

    private void OnPlayerPointChanged(GameObject getPointPlayer, int currentPoint)
    {
        if (getPointPlayer.Equals(bindPlayer))
        {
            pointValueText.text = currentPoint.ToString();
        }
    }
}
