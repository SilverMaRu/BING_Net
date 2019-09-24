using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityMove : ActivityBase
{
    protected Vector3 inputDirection
    {
        get
        {
            //Vector3 result = Input.GetAxis("V") * CameraCtrl.rigTrans.forward + Input.GetAxis("H") * CameraCtrl.cameraTrans.right;
            Vector3 result = networkInput.GetAxis("V") * CameraCtrl.rigTrans.forward + networkInput.GetAxis("H") * CameraCtrl.cameraTrans.right;
            result.Normalize();
            return result;
        }
    }
    protected virtual float axisAbsMin { get { return 0; } }
    protected virtual float axisAbsMax { get { return 1; } }
    protected AttributesManager attrManager;
    protected float enterPunishSP
    {
        get
        {
            ActivityMoveInfo info = activityInfo as ActivityMoveInfo;
            float enterValue = attrManager.maxSP * info.enterPunishPercent + info.enterPunishValue;
            return enterValue;
        }
    }
    protected float exitPunishSP
    {
        get
        {
            ActivityMoveInfo info = activityInfo as ActivityMoveInfo;
            float exitValue = attrManager.maxSP * info.exitPunishPercent + info.exitPunishValue;
            return exitValue;
        }
    }
    private long reviseReceipt;

    public ActivityMove(GameObject ownerGO) : base(ownerGO)
    {
        attrManager = ownerGO.GetComponent<AttributesManager>();
    }

    public ActivityMove(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
        attrManager = ownerGO.GetComponent<AttributesManager>();
    }

    public override bool MeetEnterCondition()
    {
        Vector3 inputDir = inputDirection;
        return inputDir != Vector3.zero
            && Vector3.Dot(inputDir, ownerGO.transform.forward) >= 0f
            && animator.GetBool("OnGround")
            ;
    }

    public override void EnterActivity()
    {
        base.EnterActivity();
        animator.applyRootMotion = true;
        AddRevise();
    }

    public override void Update()
    {
        Debug.Log("isPunishing = " + activityManager.isPunishing);
        SetAnimatorParam(axisAbsMax, 0.2f, Time.deltaTime);
        TryEndPunish();
        base.Update();
    }

    public override bool MeetExitCondition()
    {
        Vector3 inputDir = inputDirection;
        return inputDir == Vector3.zero
            || Vector3.Dot(inputDir, ownerGO.transform.forward) < 0f
            || !animator.GetBool("OnGround")
            ;
    }

    public override void ExitActivity()
    {
        base.ExitActivity();
        RemoveRevise();
    }

    private void AddRevise()
    {
        ActivityMoveInfo info = activityInfo as ActivityMoveInfo;
        reviseReceipt = attrManager.AddActivityRevise(info.reviseInfo);
    }

    private void RemoveRevise()
    {
        ActivityMoveInfo info = activityInfo as ActivityMoveInfo;
        attrManager.RemoveActivityRevise(info.reviseInfo.reviseField, reviseReceipt);
    }

    private void TryEndPunish()
    {
        if (activityManager.isPunishing && attrManager.currentSP >= exitPunishSP) activityManager.isPunishing = false;
    }
}

public class ActivityRun : ActivityMove
{
    public ActivityRun(GameObject ownerGO) : base(ownerGO)
    {
    }

    public ActivityRun(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
    }

    public override bool MeetEnterCondition()
    {
        return base.MeetEnterCondition()
            //&& !Input.GetKey(KeyCode.LeftShift)
            && !networkInput.GetKey(KeyCode.LeftShift)
            && !activityManager.isPunishing;
        ;
    }

    public override bool MeetExitCondition()
    {
        return base.MeetExitCondition()
            //|| Input.GetKey(KeyCode.LeftShift)
            || networkInput.GetKey(KeyCode.LeftShift)
            || attrManager.currentSP <= enterPunishSP;
        ;
    }

    public override void ExitActivity()
    {
        base.ExitActivity();
        activityManager.isPunishing = attrManager.currentSP <= enterPunishSP;
    }
}

public class ActivityWalk : ActivityMove
{
    protected override float axisAbsMax { get { return 0.5f; } }

    public ActivityWalk(GameObject ownerGO) : base(ownerGO)
    {
    }

    public ActivityWalk(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
    }

    public override bool MeetEnterCondition()
    {
        return base.MeetEnterCondition()
            //&& (Input.GetKey(KeyCode.LeftShift) || activityManager.isPunishing)
            && (networkInput.GetKey(KeyCode.LeftShift) || activityManager.isPunishing)
            ;
    }

    public override bool MeetExitCondition()
    {
        return base.MeetExitCondition()
            //|| (!Input.GetKey(KeyCode.LeftShift) && !activityManager.isPunishing)
            || (!networkInput.GetKey(KeyCode.LeftShift) && !activityManager.isPunishing)
            ;
    }
}

public class ActivityStopMove : ActivityBase
{
    public ActivityStopMove(GameObject ownerGO) : base(ownerGO)
    {
    }

    public ActivityStopMove(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
    }

    public override bool MeetEnterCondition()
    {
        //return Input.GetAxis("H") == 0
        //    && Input.GetAxis("V") == 0
        return networkInput.GetAxis("H") == 0
            && networkInput.GetAxis("V") == 0
            && animator.GetFloat(activityInfo.animatorParamName) > 0
            && animator.GetBool("OnGround")
            ;
    }

    public override void Update()
    {
        SetAnimatorParam(0, 0.1f, Time.deltaTime);
        base.Update();
    }

    public override bool MeetExitCondition()
    {
        //return Input.GetAxis("H") != 0
        //    || Input.GetAxis("V") != 0
        return networkInput.GetAxis("H") != 0
            || networkInput.GetAxis("V") != 0
            || animator.GetFloat(activityInfo.animatorParamName) <= 0
            || !animator.GetBool("OnGround")
            ;
    }
}
