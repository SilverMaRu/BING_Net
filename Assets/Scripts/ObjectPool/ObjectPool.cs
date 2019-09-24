using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int startSize;
    public int increaseSize;
    
    private List<IPoolWater> restingWaterList = new List<IPoolWater>();
    private List<IPoolWater> workingWaterList = new List<IPoolWater>();

    private void Awake()
    {
        AddRestingWater(startSize);
    }

    private void AddRestingWater(int num)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject tempGO = Instantiate(prefab, transform);
            IPoolWater poolWater = tempGO.GetComponent<IPoolWater>();
            if (poolWater != null)
            {
                poolWater.ownerPool = this;
                poolWater.Rest();
                restingWaterList.Add(poolWater);
            }
        }
    }

    public GameObject Borrow(Vector3 position, Quaternion rotation, Transform parent)
    {
        return Borrow(position, rotation, Vector3.one, parent);
    }

    public GameObject Borrow(Vector3 position, Quaternion rotation, Vector3 localScale, Transform parent)
    {
        if(restingWaterList.Count <= 0) AddRestingWater(increaseSize);
        IPoolWater poolWater = restingWaterList[0];
        Transform transform = (poolWater as Component).transform;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = localScale;
        transform.parent = parent;
        poolWater.Work();
        workingWaterList.Add(poolWater);
        restingWaterList.RemoveAt(0);
        return transform.gameObject;
    }

    public bool GiveBack(IPoolWater poolWater)
    {
        bool isSuccess = false;
        if (poolWater.ownerPool.Equals(this))
        {
            poolWater.Rest();
            (poolWater as Component).transform.parent = transform;
            restingWaterList.Add(poolWater);
            workingWaterList.Remove(poolWater);
        }
        return isSuccess;
    }

    public bool ComeBack(IPoolWater poolWater)
    {
        bool isSuccess = false;
        if (poolWater.ownerPool.Equals(this))
        {
            (poolWater as Component).transform.parent = transform;
            restingWaterList.Add(poolWater);
            workingWaterList.Remove(poolWater);
        }
        return isSuccess;
    }
}
