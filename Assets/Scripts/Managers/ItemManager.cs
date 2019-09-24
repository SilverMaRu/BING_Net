using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ObjectPool pickUpItemPool;
    // 道具生成频率(秒)
    public float spawFrequency = 30;
    // 已过时间
    private float pastedTime = 0;
    public int spawNum = 100;
    // 大小随机范围
    [Range(0.1f, 1.5f)]
    public float spawMinScale = 1;
    [Range(0.1f, 1.5f)]
    public float spawMaxScale = 1;
    public ItemInfoList itemInfoBase;
    private ItemBaseInfo[] itemInfos;

    public int minItemGroupCount = 1;
    public int maxItemGroupCount = 999;

    public GameObject itemPrefab;
    public ItemSpawRange rangeInfo;

    private GameObject[] playerGOs;
    private List<GameObject> scenePickUpList = new List<GameObject>();

    private void Awake()
    {
        itemInfos = new ItemBaseInfo[itemInfoBase.itemInfos.Length];
        itemInfoBase.itemInfos.CopyTo(itemInfos, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        playerGOs = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject tempPlayerGO in playerGOs)
        {
            tempPlayerGO.GetComponent<BackpackManager>().PickUpEvent += OnPickUp;
        }
        if (spawMaxScale < spawMinScale) spawMaxScale = spawMinScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        #region 道具生成测试
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    for (int i = 0; i < testSpawNum; i++)
        //    {
        //        scenePickUpList.Add(CreateItem());
        //    }
        //}
        #endregion 道具生成测试
        if (pastedTime > spawFrequency)
        {
            ClearItems();
            for (int i = 0; i < spawNum; i++)
            {
                scenePickUpList.Add(CreateItem());
            }
            pastedTime = 0;
        }
        else
        {
            pastedTime += Time.deltaTime;
        }
    }

    public GameObject CreateItem()
    {
        int randomInfoIdx = Random.Range(0, itemInfos.Length);
        return CreateItem(randomInfoIdx);
    }

    public GameObject CreateItem(int index)
    {
        Vector3 position = rangeInfo.RandomPosition();
        Vector3 scale = Vector3.one * Random.Range(spawMinScale, spawMaxScale);
        //GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);
        GameObject newItem = pickUpItemPool.Borrow(position, Quaternion.identity, scale, pickUpItemPool.transform);
        int count = Random.Range(minItemGroupCount, maxItemGroupCount + 1);
        PickUp item = newItem.GetComponent<PickUp>();
        item.itemGroup = new ItemGroup(itemInfos[index].Clone(), count);
        return newItem;
    }

    public void ClearItems()
    {
        foreach (GameObject tempGO in scenePickUpList)
        {
            IPoolWater poolWater = tempGO.GetComponent<IPoolWater>();
            if (poolWater != null) pickUpItemPool.GiveBack(poolWater);
            else Destroy(tempGO);
        }
        scenePickUpList.Clear();
    }

    private void OnPickUp(BackpackManager sender, PickUpEventDate eventDate)
    {
        switch (eventDate.pickerFaction)
        {
            case PlayerFaction.People:
                scenePickUpList.Remove(eventDate.pickUpItem.gameObject);
                break;
            case PlayerFaction.Ghost:
                ClearItems();
                break;
        }
    }
}

public enum SpawRangeType
{
    Cube,
    Sphere,
    Cylinder
}

[System.Serializable]
public class ItemSpawRange
{
    public SpawRangeType rangeType = SpawRangeType.Cylinder;
    public Vector3 center = Vector3.zero;
    public Vector3 extents = Vector3.zero;

    public Vector3 RandomPosition()
    {
        Vector3 position = center;
        float randomX = 0;
        float randomY = 0;
        float randomZ = 0;
        switch (rangeType)
        {
            case SpawRangeType.Cube:
                randomX = Random.Range(-1f, 1f);
                randomY = Random.Range(-1f, 1f);
                randomZ = Random.Range(-1f, 1f);
                Vector3 randomScale = new Vector3(randomX, randomY, randomZ);
                position = center + Vector3.Scale(extents, randomScale);
                break;
            case SpawRangeType.Sphere:
                randomX = Random.Range(-1f, 1f);
                position = Vector3.Scale(extents, Vector3.right * randomX);
                position = Random.rotation * position;
                position += center;
                break;
            case SpawRangeType.Cylinder:
                randomX = Random.Range(-1f, 1f);
                randomY = Random.Range(-1f, 1f);
                randomZ = Random.Range(0f, 360f);
                position = Vector3.Scale(extents, Vector3.right * randomX + Vector3.up * randomY);
                position = Quaternion.Euler(Vector3.up * randomZ) * position;
                position += center;
                break;
        }
        return position;
    }
}