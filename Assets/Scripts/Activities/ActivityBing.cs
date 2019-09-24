using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityBing : ActivityBase
{
    private AttributesManager attrManager;
    private PartManager partManager;
    private long timeScaleReceipt;

    public ActivityBing(GameObject ownerGO) : base(ownerGO)
    {
        attrManager = ownerGO.GetComponent<AttributesManager>();
        partManager = ownerGO.GetComponent<PartManager>();
    }

    public ActivityBing(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
        attrManager = ownerGO.GetComponent<AttributesManager>();
        partManager = ownerGO.GetComponent<PartManager>();
    }

    public override bool MeetEnterCondition()
    {
        return attrManager.faction == PlayerFaction.People
            //&& Input.GetKeyDown(KeyCode.B);
            && networkInput.GetKeyDown(KeyCode.B);
    }

    public override void EnterActivity()
    {
        base.EnterActivity();
        timeScaleReceipt = attrManager.AddActivityRevise(ReviseField.TimeScale, -1, ReviseType.PercentBase, ComputeMode.Add, 0);
        attrManager.AddActivityRevise(ReviseField.CurrentSP, 0, ReviseType.Normal, ComputeMode.Set, 0);
        activityManager.enabled = false;
        partManager.AddOnTouchAction(OnTouch);
    }

    public override void Update() { }

    public override void ExitActivity()
    {
        base.ExitActivity();
        attrManager.RemoveActivityRevise(ReviseField.TimeScale, timeScaleReceipt);
        activityManager.enabled = true;
        partManager.RemoveOnTouchAction(OnTouch);
    }

    private void OnTouch(Part sender, TouchEventData eventData)
    {
        if (sender.partType == PartType.Body)
        {
            Part otherPart = eventData.otherPart;
            if (otherPart.partManager.OwnerFaction == PlayerFaction.People && otherPart.partType == PartType.RightHand)
            {
                ExitActivity();
            }
        }
    }
}
