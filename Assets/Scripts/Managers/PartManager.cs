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
            //tempPart.SetEnable(attrManager.faction);
            tempPart.TouchEvent += OnTouch;
        }
        
        //attrManager.PlayerFactionChangeEvent += OnPlayerFactionChange;
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
        PlayerFaction onwerFaction = attrManager.faction;
        Part otherPart = eventDate.otherPart;
        PartType otherPartType = otherPart.partType;
        if (onwerFaction == PlayerFaction.Ghost && otherPartType == PartType.Body && !otherPart.partManager.IsOwnerBinging)
        {
            //GameControler.ins.SwitchFaction(gameObject, otherPart.partManager.gameObject);
            GameControler.ins.SwitchFaction(attrManager, otherPart.partManager.attrManager);
        }
        else if (onwerFaction == PlayerFaction.People && !IsOwnerBinging && otherPartType == PartType.RightHand)
        {
            //GameControler.ins.SwitchFaction(otherPart.partManager.gameObject, gameObject);
            GameControler.ins.SwitchFaction(otherPart.partManager.attrManager, attrManager);
        }
    }

    //private void OnPlayerFactionChange(PlayerFaction currentFaction)
    //{
    //    foreach (Part tempPart in managedParts)
    //    {
    //        tempPart.SetEnable(currentFaction);
    //    }
    //}
}
