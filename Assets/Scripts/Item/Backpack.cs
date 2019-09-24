using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack
{
    public GameObject ownerGO;
    private AttributesManager attrManager;
    private PlayerFaction ownerFaction { get { return attrManager.faction; } }
    private BackpackGrid[] grids;
    private int backpackSize = 1;
    //private int lastCheckIndex = 0;

    public Backpack(GameObject ownerGO) : this(ownerGO, 1)
    {
    }

    public Backpack(GameObject ownerGO, int backpackSize)
    {
        this.ownerGO = ownerGO;
        attrManager = ownerGO.GetComponent<AttributesManager>();
        this.backpackSize = backpackSize;
        grids = new BackpackGrid[backpackSize];
        for (int i = 0; i < backpackSize; i++)
        {
            grids[i] = new BackpackGrid(this);
        }
    }

    public int GetEmptyGridIndex()
    {
        int resultIdx = -1;
        for (int i = 0; i < backpackSize; i++)
        {
            if (grids[i].isEmpty)
            {
                resultIdx = i;
                break;
            }
        }
        return resultIdx;
    }

    public bool CanPutIn(int gridIndex, ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        extraItemGroup = null;
        bool canPutIn = false;
        if (!AtRange(gridIndex)) gridIndex = GetEmptyGridIndex();
        if (AtRange(gridIndex)) canPutIn = grids[gridIndex].CanPutIn(putInItemGroup, out extraItemGroup);
        return canPutIn;
    }

    public bool CanPutIn(ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        return CanPutIn(-1, putInItemGroup, out extraItemGroup);
    }

    public void PutIn(int gridIndex, ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        extraItemGroup = null;
        if (!AtRange(gridIndex)) gridIndex = GetEmptyGridIndex();
        grids[gridIndex].PutIn(putInItemGroup, out extraItemGroup);
    }

    public void PutIn(ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        PutIn(-1, putInItemGroup, out extraItemGroup);
    }

    public bool TryPutIn(int gridIndex, ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        extraItemGroup = null;
        bool canPutIn = CanPutIn(gridIndex, putInItemGroup, out extraItemGroup);
        if (canPutIn) PutIn(gridIndex, putInItemGroup, out extraItemGroup);
        return canPutIn;
    }

    public bool TryPutIn(ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        return TryPutIn(-1, putInItemGroup, out extraItemGroup);
    }

    public bool CanTakeOut(int gridIndex, int takeOutCount = 1)
    {
        bool canTakeOut = false;
        if (AtRange(gridIndex)) canTakeOut = grids[gridIndex].CanTakeOut(takeOutCount);
        return canTakeOut;
    }

    public ItemGroup TakeOut(int gridIndex, int takeOutCount = 1)
    {
        return grids[gridIndex].TakeOut(takeOutCount);
    }

    public bool TryTakeOut(int gridIndex, out ItemGroup outItemGroup, int takeOutCount = 1)
    {
        outItemGroup = null;
        bool canTakeOut = CanTakeOut(gridIndex, takeOutCount);
        if (canTakeOut) outItemGroup = TakeOut(gridIndex, takeOutCount);
        return canTakeOut;
    }

    public bool CanUse(int gridIndex)
    {
        bool canUse = false;
        if(AtRange(gridIndex) && !grids[gridIndex].isEmpty) canUse = grids[gridIndex].CanUse(attrManager);
        return canUse;
    }

    public void Use(int gridIndex)
    {
        grids[gridIndex].Use(attrManager);
    }

    public bool TryUse(int gridIndex)
    {
        bool canUse = CanUse(gridIndex);
        if (canUse) Use(gridIndex);
        return canUse;
    }

    public void Destroy(int gridIndex, int destroyCount)
    {
        if (AtRange(gridIndex)) grids[gridIndex].Destroy(destroyCount);
    }

    private bool AtRange(int gridIndex)
    {
        return gridIndex >= 0 && gridIndex < backpackSize;
    }
}

public class BackpackGrid
{
    private Backpack ownerBackpack;
    private ItemGroup myItemGroup;
    public bool isEmpty
    {
        get
        {
            Reflash();
            return myItemGroup.itemInfo == null;
        }
    }

    public BackpackGrid(Backpack ownerBackpack)
    {
        this.ownerBackpack = ownerBackpack;
        myItemGroup = new ItemGroup();
    }

    public bool CanPutIn(ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        bool canPutIn = isEmpty || myItemGroup.itemCount > 0 && myItemGroup.CompareTo(putInItemGroup) == 0;
        extraItemGroup = null;
        if (canPutIn && myItemGroup != null)
        {
            ItemBaseInfo putInItemInfo = putInItemGroup.itemInfo;
            int extraCount = putInItemGroup.itemCount + myItemGroup.itemCount - putInItemInfo.maxCountInGrid;
            if (extraCount > 0)
            {
                extraItemGroup = new ItemGroup(putInItemInfo, extraCount);
            }
        }
        return canPutIn;
    }

    public void PutIn(ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        extraItemGroup = null;
        myItemGroup.itemInfo = putInItemGroup.itemInfo;
        myItemGroup.itemCount += putInItemGroup.itemCount;
        int extraCount = myItemGroup.itemCount - myItemGroup.itemInfo.maxCountInGrid;
        if (extraCount > 0)
        {
            extraItemGroup = new ItemGroup(putInItemGroup.itemInfo, extraCount);
        }
        Reflash();
    }

    public bool TryPutIn(ItemGroup putInItemGroup, out ItemGroup extraItemGroup)
    {
        extraItemGroup = null;
        bool canPutIn = CanPutIn(putInItemGroup, out extraItemGroup);
        if (canPutIn) PutIn(putInItemGroup, out extraItemGroup);
        return canPutIn;
    }

    public bool CanTakeOut(int takeOutCount = 1)
    {
        takeOutCount = Mathf.Max(1, takeOutCount);
        bool canTakeOut = !isEmpty && myItemGroup.itemCount > takeOutCount;
        return canTakeOut;
    }

    public ItemGroup TakeOut(int takeOutCount = 1)
    {
        if (takeOutCount <= 1)
        {
            takeOutCount = myItemGroup.itemCount;
        }
        ItemGroup outItemGroup = new ItemGroup(myItemGroup.itemInfo, takeOutCount);
        myItemGroup.itemCount -= takeOutCount;
        Reflash();
        return outItemGroup;
    }

    public bool TryTakeOut(out ItemGroup outItemGroup, int takeOutCount = 1)
    {
        outItemGroup = null;
        bool canTakeOut = CanTakeOut(takeOutCount);
        if (canTakeOut) outItemGroup = TakeOut(takeOutCount);
        return canTakeOut;
    }

    public bool CanUse(AttributesManager userAttrManager)
    {
        bool canUse = false;
        if (!isEmpty)
        {
            Vector3 position = userAttrManager.transform.position;
            PlayerFaction myFaction = userAttrManager.faction;
            switch (myItemGroup.itemInfo.effectGroup)
            {
                case EffectGroup.MySelf:
                case EffectGroup.RandomTeammate:
                case EffectGroup.AllTeammate:
                case EffectGroup.RandomEnemy:
                case EffectGroup.AllEnemy:
                    canUse = true;
                    break;
                case EffectGroup.OneNearbyTeammate:
                    canUse = AttributesManager.FindNearbyPlayer(myFaction, position, myItemGroup.itemInfo.radius) != null;
                    break;
                case EffectGroup.RangeRandomTeammate:
                case EffectGroup.RangeTeammate:
                    canUse = AttributesManager.FindRangePlayer(myFaction, position, myItemGroup.itemInfo.radius).Length > 0;
                    break;
                case EffectGroup.OneNearbyEnemy:
                    canUse = AttributesManager.FindNearbyPlayer(AttributesManager.GetEnemyFaction(myFaction), position, myItemGroup.itemInfo.radius) != null;
                    break;
                case EffectGroup.RangeRandomEnemy:
                case EffectGroup.RangeEnemy:
                    canUse = AttributesManager.FindRangePlayer(AttributesManager.GetEnemyFaction(myFaction), position, myItemGroup.itemInfo.radius).Length > 0;
                    break;
            }
        }
        return canUse;
    }

    // 获得将被影响的AttributesManager
    private AttributesManager[] GetAllEffectAttrManager(AttributesManager userAttrManager)
    {
        PlayerFaction myFaction = userAttrManager.faction;
        PlayerFaction enemyFaction = AttributesManager.GetEnemyFaction(myFaction);
        Vector3 position = userAttrManager.transform.position;
        float radius = myItemGroup.itemInfo.radius;

        List<AttributesManager> effectAttrManagerList = new List<AttributesManager>();
        switch (myItemGroup.itemInfo.effectGroup)
        {
            case EffectGroup.MySelf:
                effectAttrManagerList.Add(userAttrManager);
                break;
            case EffectGroup.RandomTeammate:
                AttributesManager[] teammates = AttributesManager.GetFactionAllAttrManager(myFaction);
                effectAttrManagerList.Add(teammates[Random.Range(0, teammates.Length)]);
                break;
            case EffectGroup.OneNearbyTeammate:
                AttributesManager nearbyTeammate = AttributesManager.FindNearbyPlayer(myFaction, position, radius);
                if (nearbyTeammate != null) effectAttrManagerList.Add(nearbyTeammate);
                break;
            case EffectGroup.RangeRandomTeammate:
                AttributesManager[] rangeTeamates = AttributesManager.FindRangePlayer(myFaction, position, radius);
                effectAttrManagerList.Add(rangeTeamates[Random.Range(0, rangeTeamates.Length)]);
                break;
            case EffectGroup.RangeTeammate:
                effectAttrManagerList.AddRange(AttributesManager.FindRangePlayer(myFaction, position, radius));
                break;
            case EffectGroup.AllTeammate:
                effectAttrManagerList.AddRange(AttributesManager.GetFactionAllAttrManager(myFaction));
                break;
            case EffectGroup.RandomEnemy:
                AttributesManager[] enemys = AttributesManager.GetFactionAllAttrManager(enemyFaction);
                effectAttrManagerList.Add(enemys[Random.Range(0, enemys.Length)]);
                break;
            case EffectGroup.OneNearbyEnemy:
                AttributesManager nearbyEnemy = AttributesManager.FindNearbyPlayer(enemyFaction, position, radius);
                if (nearbyEnemy != null) effectAttrManagerList.Add(nearbyEnemy);
                break;
            case EffectGroup.RangeRandomEnemy:
                AttributesManager[] rangeEnemys = AttributesManager.FindRangePlayer(enemyFaction, position, radius);
                effectAttrManagerList.Add(rangeEnemys[Random.Range(0, rangeEnemys.Length)]);
                break;
            case EffectGroup.RangeEnemy:
                effectAttrManagerList.AddRange(AttributesManager.FindRangePlayer(enemyFaction, position, radius));
                break;
            case EffectGroup.AllEnemy:
                effectAttrManagerList.AddRange(AttributesManager.GetFactionAllAttrManager(enemyFaction));
                break;
        }
        return effectAttrManagerList.ToArray();
    }

    public void Use(AttributesManager userAttrManager)
    {
        AttributesManager[] allEffectAttrManager = GetAllEffectAttrManager(userAttrManager);
        foreach(AttributesManager tempAttrManager in allEffectAttrManager)
        {
            myItemGroup.itemInfo.Use(tempAttrManager);
        }
        myItemGroup.itemCount--;
        Reflash();
    }

    public bool TryUse(AttributesManager userAttrManager)
    {
        bool canUse = CanUse(userAttrManager);
        if (canUse) Use(userAttrManager);
        return canUse;
    }

    public void Destroy(int destroyCount)
    {
        if (destroyCount < 0 || destroyCount > myItemGroup.itemCount) destroyCount = myItemGroup.itemCount;
        myItemGroup.itemCount -= destroyCount;
        Reflash();
    }

    public void Reflash()
    {
        if (myItemGroup != null && myItemGroup.itemCount <= 0)
        {
            myItemGroup.itemInfo = null;
            myItemGroup.itemCount = 0;
        }
    }
}

[System.Serializable]
public class ItemGroup
{
    public ItemBaseInfo itemInfo;
    public int itemCount;

    public ItemGroup() { }
    public ItemGroup(ItemBaseInfo itemInfo, int itemCount)
    {
        this.itemInfo = itemInfo;
        this.itemCount = itemCount;
    }

    public int CompareTo(ItemGroup other)
    {
        return itemInfo.itemID.CompareTo(other.itemInfo.itemID);
    }
}
