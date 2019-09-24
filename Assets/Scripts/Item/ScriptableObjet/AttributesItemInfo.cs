using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemNameInfo", menuName = "ScriptableObject/AttributesItemInfo")]
public class AttributesItemInfo : ItemBaseInfo
{
    public ReviseInfo reviseInfo;
    public ReviseField reviseAttr;
    public float reviseValue;
    public ReviseType reviseMode;
    public ComputeMode computeMode;
    // 道具持续时间 (小于等于0时, 视为一次性道具)
    public float duration;

    public override ItemBaseInfo Clone()
    {
        AttributesItemInfo cloneInfo = (AttributesItemInfo)base.Clone();
        cloneInfo.reviseAttr = reviseAttr;
        cloneInfo.reviseValue = reviseValue;
        cloneInfo.reviseMode = reviseMode;
        cloneInfo.computeMode = computeMode;
        return cloneInfo;
    }

    public override void Use(AttributesManager effectAttrManager)
    {
        base.Use(effectAttrManager);
        long receipt = effectAttrManager.AddItemRevise(reviseAttr, reviseValue, reviseMode, computeMode, duration);
    }

    public override string ToString()
    {
        return base.ToString()
            + string.Format("\nreviseAttr:{0},reviseValue:{1},reviseMode:{2},computeMode:{3},duration:{4}"
            , reviseAttr
            , reviseValue
            , reviseMode
            , computeMode
            , duration);
    }
}
