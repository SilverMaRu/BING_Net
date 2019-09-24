using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ItemInfoList")]
public class ItemInfoList : ScriptableObject
{
    public ItemBaseInfo[] itemInfos;
}
