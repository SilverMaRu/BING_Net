using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NameReviseInfo",menuName ="ScriptableObject/ReviseInfo")]
public class ReviseInfo : ScriptableObject
{
    public ReviseField reviseField = ReviseField.CurrentSP;
    public float reviseValue = 0;
    public ReviseType reviseMode = ReviseType.Normal;
    public ComputeMode computeMode = ComputeMode.Add;
    // 道具持续时间 (小于等于0时, 视为永久效果道具)
    public float duration;
}
