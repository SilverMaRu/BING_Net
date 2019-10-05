using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FillingType
{
    LeftToRight,
    RightToLeft,
    BottomToTop,
    TopToBottom
}
public class Filling : MonoBehaviour
{
    public FillingType fillingType = FillingType.LeftToRight;
    public AttributesManager attrManager;
    private RectTransform rectTrans;

    private void Start()
    {
        rectTrans = (RectTransform)transform;
        Vector2 pivot = Vector2.zero;
        switch (fillingType)
        {
            case FillingType.LeftToRight:
                pivot = Vector2.up * 0.5f;
                break;
            case FillingType.RightToLeft:
                pivot = Vector2.right + Vector2.up * 0.5f;
                break;
            case FillingType.BottomToTop:
                pivot = Vector2.right * 0.5f;
                break;
            case FillingType.TopToBottom:
                pivot = Vector2.right * 0.5f + Vector2.up;
                break;
        }
        rectTrans.pivot = pivot;
        
        Client.ins.SpawnedLocalPlayerEvent += OnSpawnedLocalPlayer;
    }

    private void OnNetCodeChanged()
    {
        attrManager = Client.ins.GetLocalPlayerGO().GetComponent<AttributesManager>();
        attrManager.CurrentSPChangeEvent += OnCurrentSPChange;
    }

    private void OnSpawnedLocalPlayer()
    {
        attrManager = Client.ins.GetLocalPlayerGO().GetComponent<AttributesManager>();
        attrManager.CurrentSPChangeEvent += OnCurrentSPChange;
    }

    private void OnCurrentSPChange(float currentValue, float maxValue)
    {
        float scale = currentValue / maxValue;
        Vector2 anchorMin = rectTrans.anchorMin;
        Vector2 anchorMax = rectTrans.anchorMax;
        switch (fillingType)
        {
            case FillingType.LeftToRight:
                anchorMax = Vector2.right * scale + Vector2.up * anchorMax.y;
                break;
            case FillingType.RightToLeft:
                anchorMin = Vector2.right * (1 - scale) + Vector2.up * anchorMin.y;
                break;
            case FillingType.BottomToTop:
                anchorMax = Vector2.right * anchorMax.x + Vector2.up * scale;
                break;
            case FillingType.TopToBottom:
                anchorMin = Vector2.right * anchorMin.x + Vector2.up * (1 - scale);
                break;
        }
        rectTrans.anchorMin = anchorMin;
        rectTrans.anchorMax = anchorMax;
    }
}
