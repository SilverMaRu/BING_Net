using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PickUpDelegate(BackpackManager sender, PickUpEventDate eventDate);
public class BackpackManager : MonoBehaviour
{
    public event PickUpDelegate PickUpEvent;
    public int backpackSize = 1;
    private Backpack backpack;
    private AttributesManager attrManager;
    private NetworkInput networkInput;
    public bool canPickUp { get; private set; } = true;

    // Start is called before the first frame update
    void Start()
    {
        attrManager = GetComponent<AttributesManager>();
        networkInput = GetComponent<NetworkInput>();
        backpack = new Backpack(gameObject, backpackSize);
        PickUpEvent += OnPickUp;
    }

    private void Update()
    {
        PlayerFaction faction = attrManager.faction;
        //if (faction == PlayerFaction.People && Input.GetKeyDown(KeyCode.Mouse0))
        if (faction == PlayerFaction.People && networkInput.GetKeyDown(KeyCode.Mouse0))
        {
            backpack.TryUse(0);
        }
        else if (faction == PlayerFaction.Ghost)
        {
            if (!backpack.TryUse(0)) backpack.Destroy(0, -1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("PickUp") && canPickUp)
        {
            PickUp pickUpItem = other.GetComponent<PickUp>();
            ItemGroup extraItemGroup = null;
            backpack.TryPutIn(pickUpItem.itemGroup, out extraItemGroup);
            PickUpEvent?.Invoke(this, new PickUpEventDate(pickUpItem, attrManager.faction));
        }
    }

    private void OnPickUp(BackpackManager sender, PickUpEventDate eventDate)
    {
        switch (eventDate.pickerFaction)
        {
            case PlayerFaction.People:
                if (attrManager.faction == PlayerFaction.People) canPickUp = false;
                break;
            case PlayerFaction.Ghost:
                canPickUp = true;
                break;
        }
    }
}

public class PickUpEventDate
{
    public PickUp pickUpItem;
    public PlayerFaction pickerFaction;

    public PickUpEventDate(PickUp pickUpItem, PlayerFaction pickerFaction)
    {
        this.pickUpItem = pickUpItem;
        this.pickerFaction = pickerFaction;
    }
}
