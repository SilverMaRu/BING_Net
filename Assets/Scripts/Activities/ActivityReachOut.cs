using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityReachOut : ActivityBase
{
    private AttributesManager attrManager;
    private Transform reachOutIKTran;
    private long reviseReceipt;

    private ActivityReachOutInfo reachOutInfo;
    private float _weight = 0;
    private float weight
    {
        get
        {
            return _weight;
        }
        set
        {
            _weight = Mathf.Clamp(value, 0, 1);
        }
    }

    public ActivityReachOut(GameObject ownerGO) : base(ownerGO)
    {
        attrManager = ownerGO.GetComponent<AttributesManager>();
        reachOutInfo = activityInfo as ActivityReachOutInfo;
        reachOutIKTran = ownerGO.transform.Find(reachOutInfo.IKGameObjectName);
    }

    public ActivityReachOut(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
        attrManager = ownerGO.GetComponent<AttributesManager>();
        reachOutInfo = activityInfo as ActivityReachOutInfo;
        reachOutIKTran = ownerGO.transform.Find(reachOutInfo.IKGameObjectName);
    }

    public override bool MeetEnterCondition()
    {
        //return Input.GetKey(KeyCode.Mouse1);
        return networkInput.GetKey(KeyCode.Mouse1);
    }

    public override void EnterActivity()
    {
        base.EnterActivity();
        AddRevise();
        activityManager.OnAnimatorIKAction += ReachOut;
        activityManager.OnAnimatorIKAction -= Countermand;
    }

    public override bool MeetExitCondition()
    {
        //return !Input.GetKey(KeyCode.Mouse1);
        return !networkInput.GetKey(KeyCode.Mouse1);
    }

    public override void ExitActivity()
    {
        base.ExitActivity();
        activityManager.OnAnimatorIKAction += Countermand;
        activityManager.OnAnimatorIKAction -= ReachOut;
    }

    private void AddRevise()
    {
        reviseReceipt = attrManager.AddActivityRevise(reachOutInfo.reviseInfo);
    }

    private void ReachOut(int layerIndex)
    {
        weight += Time.deltaTime / reachOutInfo.reachOutUseTime;
        SetIK(layerIndex, weight);
    }

    private void Countermand(int layerIndex)
    {
        weight -= Time.deltaTime / reachOutInfo.countermandUseTime;
        SetIK(layerIndex, weight);
        if(weight <= 0) activityManager.OnAnimatorIKAction -= Countermand;
    }

    private void SetIK(int layerIndex, float weight)
    {
        animator.SetIKPosition(reachOutInfo.effectGoal, reachOutIKTran.position);
        animator.SetIKRotation(reachOutInfo.effectGoal, reachOutIKTran.rotation);
        animator.SetIKPositionWeight(reachOutInfo.effectGoal, weight);
        animator.SetIKRotationWeight(reachOutInfo.effectGoal, weight);
    }
}
