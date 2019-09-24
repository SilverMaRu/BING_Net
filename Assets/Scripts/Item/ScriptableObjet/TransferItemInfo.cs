using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemNameInfo", menuName = "ScriptableObject/TransferItemInfo")]
public class TransferItemInfo : ItemBaseInfo
{
    public float newPositionX;
    public float newPositionY;
    public float newPositionZ;

    public Vector3 GetPosition()
    {
        return new Vector3(newPositionX, newPositionY, newPositionZ);
    }

    public override ItemBaseInfo Clone()
    {
        TransferItemInfo cloneInfo = (TransferItemInfo)base.Clone();
        cloneInfo.newPositionX = newPositionX;
        cloneInfo.newPositionY = newPositionY;
        cloneInfo.newPositionZ = newPositionZ;
        return cloneInfo;
    }

    public override void Use(AttributesManager effectAttrManager)
    {
        base.Use(effectAttrManager);
        effectAttrManager.transform.position = GetPosition();
    }
}
