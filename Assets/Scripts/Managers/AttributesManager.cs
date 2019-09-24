using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MaxAttributesChangeDelegate(float currentValue);
public delegate void CurrentAttributesChangeDelegate(float currentValue, float maxValue);
public delegate void PlayerFactionChangeDelegate(PlayerFaction currentFaction);
public class AttributesManager : MonoBehaviour
{
    private static List<AttributesManager> _allAttrManager = new List<AttributesManager>();
    public static AttributesManager[] allAttrManager { get { return _allAttrManager.ToArray(); } }
    public event MaxAttributesChangeDelegate MaxSPChangeEvent;
    public event CurrentAttributesChangeDelegate CurrentSPChangeEvent;
    public event PlayerFactionChangeDelegate PlayerFactionChangeEvent;
    public event System.Action<float> TimeScaleChangeEvent;

    #region UseTest
    public PlayerFaction initFaction = PlayerFaction.People;
    #endregion UseTest
    public Attributes peopleAttributes;
    public Attributes ghostAttributes;
    private PlayerFaction _faction = PlayerFaction.People;
    public PlayerFaction faction
    {
        get
        {
            return _faction;
        }
        set
        {
            _faction = value;
            PlayerFactionChangeEvent?.Invoke(_faction);
        }
    }
    private Dictionary<long, float> receiptActivityReviseMaxSPPairs = new Dictionary<long, float>();
    private Dictionary<long, float> receiptItemReviseMaxSPPairs = new Dictionary<long, float>();
    public float baseMaxSP
    {
        get
        {
            float resultValue = 0;
            switch (faction)
            {
                case PlayerFaction.People:
                    resultValue = peopleAttributes.maxSP;
                    break;
                case PlayerFaction.Ghost:
                    resultValue = ghostAttributes.maxSP;
                    break;
            }
            return resultValue;
        }
    }
    public float maxSP
    {
        get
        {
            float finalValue = baseMaxSP;
            #region 合计道具修正
            float[] itemReviseArray = new float[receiptItemReviseMaxSPPairs.Count];
            receiptItemReviseMaxSPPairs.Values.CopyTo(itemReviseArray, 0);
            foreach (float tempRevise in itemReviseArray)
            {
                finalValue += tempRevise;
            }
            #endregion 合计道具修正
            #region 合计行为修正
            float[] activityReviseArray = new float[receiptActivityReviseMaxSPPairs.Count];
            receiptActivityReviseMaxSPPairs.Values.CopyTo(activityReviseArray, 0);
            foreach (float tempRevise in activityReviseArray)
            {
                finalValue += tempRevise;
            }
            #endregion 合计行为修正
            return Mathf.Max(0, finalValue);
        }
    }
    private float _currentSP;
    public float currentSP
    {
        get { return Mathf.Min(_currentSP, maxSP); }
        set
        {
            _currentSP = Mathf.Clamp(value, 0, maxSP);
            CurrentSPChangeEvent?.Invoke(_currentSP, maxSP);
        }
    }
    private Dictionary<long, float> receiptActivityReviseRecoverSPPairs = new Dictionary<long, float>();
    private Dictionary<long, float> receiptItemReviseRecoverSPPairs = new Dictionary<long, float>();
    public float baseRecoverSP
    {
        get
        {
            float resultValue = 0;
            switch (faction)
            {
                case PlayerFaction.People:
                    resultValue = peopleAttributes.recoverSP;
                    break;
                case PlayerFaction.Ghost:
                    resultValue = ghostAttributes.recoverSP;
                    break;
            }
            return resultValue;
        }
    }
    public float recoverSP
    {
        get
        {
            float finalValue = baseRecoverSP;
            #region 合计道具修正
            float[] itemReviseArray = new float[receiptItemReviseRecoverSPPairs.Count];
            receiptItemReviseRecoverSPPairs.Values.CopyTo(itemReviseArray, 0);
            foreach (float tempRevise in itemReviseArray)
            {
                finalValue += tempRevise;
            }
            #endregion 合计道具修正
            #region 合计行为修正
            float[] activityReviseArray = new float[receiptActivityReviseRecoverSPPairs.Count];
            receiptActivityReviseRecoverSPPairs.Values.CopyTo(activityReviseArray, 0);
            foreach (float tempRevise in activityReviseArray)
            {
                finalValue += tempRevise;
            }
            #endregion 合计行为修正
            return finalValue;
        }
    }
    private Dictionary<long, float> receiptActivityReviseTimeScalePairs = new Dictionary<long, float>();
    private Dictionary<long, float> receiptItemReviseTimeScalePairs = new Dictionary<long, float>();
    public float baseTimeScale
    {
        get
        {
            float resultValue = 0;
            switch (faction)
            {
                case PlayerFaction.People:
                    resultValue = peopleAttributes.timeScale;
                    break;
                case PlayerFaction.Ghost:
                    resultValue = ghostAttributes.timeScale;
                    break;
            }
            return resultValue;
        }
    }
    public float timeScale
    {
        get
        {
            float finalValue = baseTimeScale;
            #region 合计道具修正
            float[] itemReviseArray = new float[receiptItemReviseTimeScalePairs.Count];
            receiptItemReviseTimeScalePairs.Values.CopyTo(itemReviseArray, 0);
            foreach (float tempRevise in itemReviseArray)
            {
                finalValue += tempRevise;
            }
            #endregion 合计道具修正
            #region 合计行为修正
            float[] activityReviseArray = new float[receiptActivityReviseTimeScalePairs.Count];
            receiptActivityReviseTimeScalePairs.Values.CopyTo(activityReviseArray, 0);
            foreach (float tempRevise in activityReviseArray)
            {
                finalValue += tempRevise;
            }
            #endregion 合计行为修正
            return Mathf.Max(0, finalValue);
        }
    }

    private Animator anim;

    private void Awake()
    {
        faction = initFaction;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSP = maxSP;
        anim = GetComponent<Animator>();
        SetAnimatorSpeed(timeScale);
    }

    // Update is called once per frame
    void Update()
    {
        currentSP += recoverSP * Time.deltaTime * timeScale;
    }

    public long AddActivityRevise(ReviseField reviseField, float reviseValue, ReviseType reviseMode, ComputeMode computeMode, float duration)
    {
        long receipt = -1;
        switch (reviseField)
        {
            case ReviseField.MaxSP:
                receipt = AddActivityReviseMaxSP(reviseValue, reviseMode, duration);
                break;
            case ReviseField.CurrentSP:
                ReviseCurrentSP(reviseValue, reviseMode, computeMode);
                break;
            case ReviseField.RecoverSP:
                receipt = AddActivityReviseRecoverSP(reviseValue, reviseMode, duration);
                break;
            case ReviseField.TimeScale:
                receipt = AddActivityReviseTimeScale(reviseValue, reviseMode, duration);
                break;
        }

        return receipt;
    }

    public long AddActivityRevise(ReviseInfo info)
    {
        return AddActivityRevise(info.reviseField, info.reviseValue, info.reviseMode, info.computeMode, info.duration);
    }

    public long AddItemRevise(ReviseField reviseField, float reviseValue, ReviseType reviseMode, ComputeMode computeMode, float duration)
    {
        long receipt = -1;
        switch (reviseField)
        {
            case ReviseField.MaxSP:
                receipt = AddItemReviseMaxSP(reviseValue, reviseMode, duration);
                break;
            case ReviseField.CurrentSP:
                ReviseCurrentSP(reviseValue, reviseMode, computeMode);
                break;
            case ReviseField.RecoverSP:
                receipt = AddItemReviseRecoverSP(reviseValue, reviseMode, duration);
                break;
            case ReviseField.TimeScale:
                receipt = AddItemReviseTimeScale(reviseValue, reviseMode, duration);
                break;
        }

        return receipt;
    }

    public long AddItemRevise(ReviseInfo info)
    {
        return AddItemRevise(info.reviseField, info.reviseValue, info.reviseMode, info.computeMode, info.duration);
    }

    public void RemoveActivityRevise(ReviseField reviseField, long receipt)
    {
        switch (reviseField)
        {
            case ReviseField.MaxSP:
                RemoveActivityReviseMaxSP(receipt);
                break;
            case ReviseField.RecoverSP:
                RemoveActivityReviseRecoverSP(receipt);
                break;
            case ReviseField.TimeScale:
                RemoveActivityReviseTimeScale(receipt);
                break;
        }
    }

    public void RemoveAllActivityRevise()
    {
        receiptActivityReviseMaxSPPairs.Clear();
        receiptActivityReviseRecoverSPPairs.Clear();
        receiptActivityReviseTimeScalePairs.Clear();
    }

    public void RemoveItemRevise(ReviseField reviseField, long receipt)
    {
        switch (reviseField)
        {
            case ReviseField.MaxSP:
                RemoveItemReviseMaxSP(receipt);
                break;
            case ReviseField.RecoverSP:
                RemoveItemReviseRecoverSP(receipt);
                break;
            case ReviseField.TimeScale:
                RemoveItemReviseTimeScale(receipt);
                break;
        }
    }

    public void RemoveAllItemRevise()
    {
        receiptItemReviseMaxSPPairs.Clear();
        receiptItemReviseRecoverSPPairs.Clear();
        receiptItemReviseTimeScalePairs.Clear();
    }

    private long AddActivityReviseMaxSP(float reviseValue, ReviseType reviseMode, float duration)
    {
        long receipt = System.DateTime.Now.ToBinary();
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = baseMaxSP * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = maxSP * reviseValue;
                break;
        }
        receiptActivityReviseMaxSPPairs.Add(receipt, revise);
        MaxSPChangeEvent?.Invoke(maxSP);
        if (duration > 0) StartCoroutine(IEnumeratorHelper.After(RemoveActivityReviseMaxSP, receipt, duration));
        return receipt;
    }

    private long AddItemReviseMaxSP(float reviseValue, ReviseType reviseMode, float duration)
    {
        long receipt = System.DateTime.Now.ToBinary();
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = baseMaxSP * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = maxSP * reviseValue;
                break;
        }
        receiptItemReviseMaxSPPairs.Add(receipt, revise);
        MaxSPChangeEvent?.Invoke(maxSP);
        if (duration > 0) StartCoroutine(IEnumeratorHelper.After(RemoveItemReviseMaxSP, receipt, duration));
        return receipt;
    }

    private void RemoveActivityReviseMaxSP(long receipt)
    {
        if (receiptActivityReviseMaxSPPairs.ContainsKey(receipt))
        {
            receiptActivityReviseMaxSPPairs.Remove(receipt);
            MaxSPChangeEvent?.Invoke(maxSP);
        }
    }

    private void RemoveItemReviseMaxSP(long receipt)
    {
        if (receiptItemReviseMaxSPPairs.ContainsKey(receipt))
        {
            receiptItemReviseMaxSPPairs.Remove(receipt);
            MaxSPChangeEvent?.Invoke(maxSP);
        }
    }

    private void ReviseCurrentSP(float reviseValue, ReviseType reviseMode, ComputeMode computeMode)
    {
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = maxSP * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = currentSP * reviseValue;
                break;
            case ReviseType.PercentUsed:
                revise = (maxSP - currentSP) * reviseValue;
                break;
        }
        switch (computeMode)
        {
            case ComputeMode.Add:
                currentSP += revise;
                break;
            case ComputeMode.Set:
                if (revise <= 0)
                {
                    currentSP = Mathf.Min(currentSP, -revise);
                }
                else
                {
                    currentSP = Mathf.Max(currentSP, revise);
                }
                break;
        }
    }

    private long AddActivityReviseRecoverSP(float reviseValue, ReviseType reviseMode, float duration)
    {
        long receipt = System.DateTime.Now.ToBinary();
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = baseRecoverSP * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = recoverSP * reviseValue;
                break;
        }
        receiptActivityReviseRecoverSPPairs.Add(receipt, revise);
        if (duration > 0) StartCoroutine(IEnumeratorHelper.After(RemoveActivityReviseRecoverSP, receipt, duration));
        return receipt;
    }

    private long AddItemReviseRecoverSP(float reviseValue, ReviseType reviseMode, float duration)
    {
        long receipt = System.DateTime.Now.ToBinary();
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = baseRecoverSP * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = recoverSP * reviseValue;
                break;
        }
        receiptItemReviseRecoverSPPairs.Add(receipt, revise);
        if (duration > 0) StartCoroutine(IEnumeratorHelper.After(RemoveItemReviseRecoverSP, receipt, duration));
        return receipt;
    }

    private void RemoveActivityReviseRecoverSP(long receipt)
    {
        if (receiptActivityReviseRecoverSPPairs.ContainsKey(receipt)) receiptActivityReviseRecoverSPPairs.Remove(receipt);
    }

    private void RemoveItemReviseRecoverSP(long receipt)
    {
        if (receiptItemReviseRecoverSPPairs.ContainsKey(receipt)) receiptItemReviseRecoverSPPairs.Remove(receipt);
    }

    private long AddActivityReviseTimeScale(float reviseValue, ReviseType reviseMode, float duration)
    {
        long receipt = System.DateTime.Now.ToBinary();
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = baseTimeScale * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = timeScale * reviseValue;
                break;
        }
        receiptActivityReviseTimeScalePairs.Add(receipt, revise);
        SetAnimatorSpeed(timeScale);
        TimeScaleChangeEvent?.Invoke(timeScale);
        if (duration > 0) StartCoroutine(IEnumeratorHelper.After(RemoveActivityReviseTimeScale, receipt, duration));
        return receipt;
    }

    private long AddItemReviseTimeScale(float reviseValue, ReviseType reviseMode, float duration)
    {
        long receipt = System.DateTime.Now.ToBinary();
        float revise = reviseValue;
        switch (reviseMode)
        {
            case ReviseType.PercentBase:
                revise = baseTimeScale * reviseValue;
                break;
            case ReviseType.PercentCurrent:
                revise = timeScale * reviseValue;
                break;
        }
        receiptItemReviseTimeScalePairs.Add(receipt, revise);
        SetAnimatorSpeed(timeScale);
        TimeScaleChangeEvent?.Invoke(timeScale);
        if (duration > 0) StartCoroutine(IEnumeratorHelper.After(RemoveItemReviseTimeScale, receipt, duration));
        return receipt;
    }

    private void RemoveActivityReviseTimeScale(long receipt)
    {
        if (receiptActivityReviseTimeScalePairs.ContainsKey(receipt))
        {
            receiptActivityReviseTimeScalePairs.Remove(receipt);
            SetAnimatorSpeed(timeScale);
            TimeScaleChangeEvent?.Invoke(timeScale);
        }
    }

    private void RemoveItemReviseTimeScale(long receipt)
    {
        if (receiptItemReviseTimeScalePairs.ContainsKey(receipt))
        {
            receiptItemReviseTimeScalePairs.Remove(receipt);
            SetAnimatorSpeed(timeScale);
            TimeScaleChangeEvent?.Invoke(timeScale);
        }
    }

    private void SetAnimatorSpeed(float currentValue)
    {
        anim.speed = currentValue;
    }

    public static AttributesManager[] GetFactionAllAttrManager(PlayerFaction faction)
    {
        List<AttributesManager> factionAttrManager = new List<AttributesManager>();
        foreach (AttributesManager tempAttrManager in allAttrManager)
        {
            if (tempAttrManager.faction == faction)
            {
                factionAttrManager.Add(tempAttrManager);
            }
        }
        return factionAttrManager.ToArray();
    }

    public static PlayerFaction GetEnemyFaction(PlayerFaction myFaction)
    {
        PlayerFaction enemyFaction = PlayerFaction.Ghost;
        switch (myFaction)
        {
            case PlayerFaction.People:
                enemyFaction = PlayerFaction.Ghost;
                break;
            case PlayerFaction.Ghost:
                enemyFaction = PlayerFaction.People;
                break;
        }
        return enemyFaction;
    }

    public static AttributesManager FindNearbyPlayer(PlayerFaction targetFaction, Vector3 center, float radius)
    {
        AttributesManager resultAttrManager = null;
        AttributesManager[] teammateAttrManager = GetFactionAllAttrManager(targetFaction);
        bool onePeopleFaction = teammateAttrManager.Length <= 1;
        float currentSqrRadius = Mathf.Sqrt(radius);
        foreach (AttributesManager tempTeammate in teammateAttrManager)
        {
            Vector3 centerToTeammate = tempTeammate.transform.position - center;
            float sqrMagnitude = centerToTeammate.sqrMagnitude;
            if (!onePeopleFaction && sqrMagnitude != 0 && sqrMagnitude <= currentSqrRadius)
            {
                resultAttrManager = tempTeammate;
                currentSqrRadius = sqrMagnitude;
            }
            else if (onePeopleFaction)
            {
                resultAttrManager = tempTeammate;
            }
        }
        return resultAttrManager;
    }

    public static AttributesManager[] FindRangePlayer(PlayerFaction targetFaction, Vector3 center, float radius)
    {
        List<AttributesManager> rangeTeammateList = new List<AttributesManager>();
        AttributesManager[] teammateAttrManager = AttributesManager.GetFactionAllAttrManager(targetFaction);
        float sqrRadius = Mathf.Sqrt(radius);
        foreach (AttributesManager tempTeammate in teammateAttrManager)
        {
            Vector3 centerToTeammate = tempTeammate.transform.position - center;
            float sqrMagnitude = centerToTeammate.sqrMagnitude;
            if (sqrMagnitude <= sqrRadius) rangeTeammateList.Add(tempTeammate);
        }
        return rangeTeammateList.ToArray();
    }
}

//[System.Serializable]
//public class ReviseInfo
//{
//    public ReviseField reviseField = ReviseField.CurrentSP;
//    public float reviseValue = 0;
//    public ReviseType reviseMode = ReviseType.Normal;
//    public ComputeMode computeMode = ComputeMode.Add;
//    // 道具持续时间 (小于等于0时, 视为永久效果道具)
//    public float duration;
//}

public enum ReviseType
{
    Normal,
    PercentBase,
    PercentCurrent,
    PercentUsed
}

public enum ComputeMode
{
    Add,
    Set
}

public enum ReviseField
{
    MaxSP,
    CurrentSP,
    RecoverSP,
    TimeScale
}

public enum PlayerFaction
{
    People,
    Ghost
}
