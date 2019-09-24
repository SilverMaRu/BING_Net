using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBaseInfo : ScriptableObject
{
    public string itemName;
    public int itemID;
    // 单个背包格子最大叠加数
    public int maxCountInGrid = 999;
    public EffectGroup effectGroup;
    // 范围半径
    public float radius;

    public virtual ItemBaseInfo Clone()
    {
        //Debug.Log("Do ItemBaseInfo.Clone");
        ItemBaseInfo cloneInfo = (ItemBaseInfo)CreateInstance(GetType());
        cloneInfo.itemName = itemName;
        cloneInfo.itemID = itemID;
        cloneInfo.maxCountInGrid = maxCountInGrid;
        cloneInfo.effectGroup = effectGroup;
        cloneInfo.radius = radius;
        return cloneInfo;
    }

    public virtual void Use(AttributesManager effectAttrManager)
    {
    }

    public override string ToString()
    {
        return string.Format("itemName:{0},itemID:{1},maxCountInGrid:{2},effectGroup:{3},radius:{4}"
            , itemName
            , itemID
            , maxCountInGrid
            , effectGroup
            , radius);
    }
}
// 作用群体
public enum EffectGroup
{
    // 自己
    MySelf,
    // 一位随机队友(包括自己)
    RandomTeammate,
    // 最近的一名队友(如果阵营中有队友则不包括自己, 若阵营中只有自己则等同与MySelf)
    OneNearbyTeammate,
    // 范围内随机一名队友(包括自己)
    RangeRandomTeammate,
    // 范围内的所有队友(包括自己)
    RangeTeammate,
    // 全队
    AllTeammate,
    // 随机一名敌人
    RandomEnemy,
    // 最近的一名敌人
    OneNearbyEnemy,
    // 范围内随机一名敌人
    RangeRandomEnemy,
    // 范围内的所有敌人
    RangeEnemy,
    // 所有敌人
    AllEnemy,
}
