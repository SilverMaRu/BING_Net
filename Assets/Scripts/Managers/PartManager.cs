using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartManager : MonoBehaviour
{
    private Part[] managedParts = new Part[0];

    private AttributesManager attrManager;
    private ActivityManager activityManager;
    public PlayerFaction OwnerFaction
    {
        get
        {
            return attrManager.faction;
        }
    }
    public bool IsOwnerBinging
    {
        get
        {
            return activityManager.isBinging;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        attrManager = GetComponent<AttributesManager>();
        activityManager = GetComponent<ActivityManager>();

        managedParts = transform.GetComponentsInChildren<Part>();
        foreach (Part tempPart in managedParts)
        {
            tempPart.partManager = this;
            tempPart.TouchEvent += OnTouch;
        }
    }

    public void AddOnTouchAction(TouchDelegate OnTouchAction)
    {
        foreach (Part tempPart in managedParts)
        {
            tempPart.TouchEvent += OnTouchAction;
        }
    }

    public void RemoveOnTouchAction(TouchDelegate OnTouchAction)
    {
        foreach (Part tempPart in managedParts)
        {
            tempPart.TouchEvent -= OnTouchAction;
        }
    }

    private void OnTouch(Part sender, TouchEventData eventDate)
    {
        // 如果是服务器则正常做处理
        if (Server.ins.enabled)
        {
            PlayerFaction onwerFaction = attrManager.faction;
            Part otherPart = eventDate.otherPart;
            PartType otherPartType = otherPart.partType;
            if (onwerFaction == PlayerFaction.Ghost && otherPartType == PartType.Body && !otherPart.partManager.IsOwnerBinging)
            {
                //GameControler.ins.SwitchFaction(attrManager, otherPart.partManager.attrManager);
                Server.ins.SwitchFaction(attrManager, otherPart.partManager.attrManager);
            }
            else if (onwerFaction == PlayerFaction.People && !IsOwnerBinging && otherPartType == PartType.RightHand)
            {
                //GameControler.ins.SwitchFaction(otherPart.partManager.attrManager, attrManager);
                Server.ins.SwitchFaction(otherPart.partManager.attrManager, attrManager);
            }
        }
    }
}
