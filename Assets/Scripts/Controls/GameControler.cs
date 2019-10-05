using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SwitchFactionDelegate();
public class GameControler : MonoBehaviour
{
    public static GameControler ins { get; private set; }
    public event SwitchFactionDelegate SwitchFactionStartEvent;
    public event SwitchFactionDelegate SwitchFactionCompleteEvent;

    public GameObject playerPrefab;
    public GameObject[] playerGOs;
    public Transform spawnPoints;

    public float switchFactionUseTime = 5;
    public ReviseInfo[] changeToGhostRevises;
    public ReviseInfo[] changeToPeopleRevises;

    // Ghost抓到People获得的分数
    public int rewardPoint = 10;
    public int score { get; private set; }

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
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            int randomIndex = Random.Range(0, players.Length - 1);

            ChangeToGhost(players[randomIndex].GetComponent<AttributesManager>());
        }
        #endregion 阵营切换测试
    }

    private void ChangeToPeople(AttributesManager originalGhostAttrManager)
    {
        originalGhostAttrManager.faction = PlayerFaction.People;
        originalGhostAttrManager.RemoveAllItemRevise();
        foreach (ReviseInfo tempReviseInfo in changeToPeopleRevises)
        {
            long reviseReceipt = originalGhostAttrManager.AddItemRevise(tempReviseInfo);
        }
    }

    private void ChangeToGhost(AttributesManager originalPeopleAttrManager)
    {
        originalPeopleAttrManager.faction = PlayerFaction.Ghost;
        originalPeopleAttrManager.RemoveAllItemRevise();
        foreach (ReviseInfo tempReviseInfo in changeToGhostRevises)
        {
            long reviseReceipt = originalPeopleAttrManager.AddItemRevise(tempReviseInfo);
        }
        ActivityManager originalPeopleActivityManager = originalPeopleAttrManager.GetComponent<ActivityManager>();
        originalPeopleActivityManager.enabled = false;
        StartCoroutine(IEnumeratorHelper.After(() => { originalPeopleActivityManager.enabled = true; }, switchFactionUseTime));
    }

    public void SwitchFaction(AttributesManager originalGhostAttrManager, AttributesManager originalPeopleAttrManager)
    {
        SwitchFactionStartEvent?.Invoke();
        StartCoroutine(IEnumeratorHelper.After(() => { SwitchFactionCompleteEvent?.Invoke(); }, switchFactionUseTime));
        ChangeToPeople(originalGhostAttrManager);

        ChangeToGhost(originalPeopleAttrManager);
        AddScore(originalGhostAttrManager.gameObject);
    }

    public void AddScore(GameObject addScorePlayerGO)
    {
        addScorePlayerGO.GetComponent<PlayerState>().score += rewardPoint;
    }

    public GameObject SpawnPlayer(int netCode, string name)
    {
        int randomSpawnIndex = Random.Range(0, spawnPoints.childCount - 1);
        float randomYew = Random.Range(0f, 360f);
        GameObject resultGO = null;
        foreach (GameObject tempGO in playerGOs)
        {
            if (!tempGO.activeInHierarchy)
            {
                resultGO = tempGO;
                break;
            }
        }
        resultGO.transform.position = spawnPoints.GetChild(randomSpawnIndex).position;
        resultGO.transform.rotation = Quaternion.Euler(0, randomYew, 0);
        resultGO.SetActive(true);
        resultGO.name = name;
        resultGO.GetComponent<PlayerState>().netCode = netCode;
        return resultGO;
    }
}
