using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SwitchFactionDelegate();
public class GameControler : MonoBehaviour
{
    public static GameControler ins { get; private set; }
    public event SwitchFactionDelegate SwitchFactionStartEvent;
    public event SwitchFactionDelegate SwitchFactionCompleteEvent;
    public event System.Action<GameObject, int> PlayerPointChangedEvent;

    public float switchFactionUseTime = 5;
    public ReviseInfo[] changeToGhostRevises;
    public ReviseInfo[] changeToPeopleRevises;

    // Ghost抓到People获得的分数
    public int rewardPoint = 10;
    private Dictionary<GameObject, int> playerPointPairs = new Dictionary<GameObject, int>();

    #region 阵营切换测试
    public GameObject originalGhostGO;
    public GameObject originalPeopleGO;
    #endregion 阵营切换测试

    private void Awake()
    {
        ins = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject tempPlay in players)
        {
            playerPointPairs.Add(tempPlay, 0);
        }
    }

    private void Update()
    {
        #region 阵营切换测试
        //if (Input.GetKeyDown(KeyCode.C) && originalGhostGO != null && originalPeopleGO != null)
        //{
        //    SwitchFaction(originalGhostGO, originalPeopleGO);
        //    GameObject tempGO = originalGhostGO;
        //    originalGhostGO = originalPeopleGO;
        //    originalPeopleGO = tempGO;
        //}
        #endregion 阵营切换测试
    }

    //public void SwitchFaction(GameObject originalGhostAttrManager, GameObject originalPeopleAttrManager)
    public void SwitchFaction(AttributesManager originalGhostAttrManager, AttributesManager originalPeopleAttrManager)
    {
        SwitchFactionStartEvent?.Invoke();
        StartCoroutine(IEnumeratorHelper.After(() => { SwitchFactionCompleteEvent?.Invoke(); }, switchFactionUseTime));
        //AttributesManager originalGhostAttrManager = originalGhostAttrManager.GetComponent<AttributesManager>();
        originalGhostAttrManager.faction = PlayerFaction.People;
        originalGhostAttrManager.RemoveAllItemRevise();
        foreach (ReviseInfo tempReviseInfo in changeToPeopleRevises)
        {
            long reviseReceipt = originalGhostAttrManager.AddItemRevise(tempReviseInfo);
        }

        //AttributesManager originalPeopleAttrManager = originalPeopleAttrManager.GetComponent<AttributesManager>();
        originalPeopleAttrManager.faction = PlayerFaction.Ghost;
        originalPeopleAttrManager.RemoveAllItemRevise();
        foreach(ReviseInfo tempReviseInfo in changeToGhostRevises)
        {
            long reviseReceipt = originalPeopleAttrManager.AddItemRevise(tempReviseInfo);
        }
        ActivityManager originalPeopleActivityManager = originalPeopleAttrManager.GetComponent<ActivityManager>();
        originalPeopleActivityManager.enabled = false;
        StartCoroutine(IEnumeratorHelper.After(() => { originalPeopleActivityManager.enabled = true; }, switchFactionUseTime));
        //AddPoint(originalGhostAttrManager, rewardPoint);
        AddPoint(originalGhostAttrManager.gameObject, rewardPoint);
    }

    public void AddPoint(GameObject player, int point)
    {
        int currentPoint;
        if(playerPointPairs.TryGetValue(player, out currentPoint))
        {
            currentPoint += point;
            PlayerPointChangedEvent?.Invoke(player, currentPoint);
        }
    }
}
