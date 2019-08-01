using System;
using System.Collections;
using System.Collections.Generic;
using Lean;
using RVO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using Random = System.Random;
using KFrameWork;

public class GameMainManager : SingletonBehaviour<GameMainManager>
{
    public GameObject agentPrefab;

    public bool draw;

    [HideInInspector]
    public Vector2 mousePosition;

    private Plane m_hPlane = new Plane(Vector3.up, Vector3.zero);
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();

    public int cnt = 1;

    // Use this for initialization
    void Start()
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.SetSingleTonMode(true);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, KInt2.zero);

        for (int i = 0; i < cnt; ++i)
        {
            CreatAgent();
        }

        Simulator.Instance.SetNumWorkers(1);
    }


    private void UpdateMousePosition()
    {
        Vector3 position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (m_hPlane.Raycast(mouseRay, out rayDistance))
            position = mouseRay.GetPoint(rayDistance);
        //位置
        mousePosition.x = position.x;
        mousePosition.y = position.z;
    }

    void DeleteAgent()
    {
        int agentNo = Simulator.Instance.queryNearAgent((KInt2)mousePosition, 1.5f);
        if (agentNo == -1 || !m_agentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        LeanPool.Despawn(m_agentMap[agentNo].gameObject);
        m_agentMap.Remove(agentNo);
    }

    public float neighborDist = 15;
    public int maxNeighbors = 10;
    public float timeHorizon = 5;
    public float timeHorizonObst = 5;
    public float radius = 2;
    public float maxSpeed = 2;
    public KInt2 velocity;

    /// <summary>
    /// 创建agent
    /// 实例化一个gameagent，放入到字典m_agentMap中
    /// </summary>
    void CreatAgent()
    {
        int sid = Simulator.Instance.addAgent((KInt2)mousePosition, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity);
        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x, 0, mousePosition.y), Quaternion.identity);
            GameAgent ga = go.GetComponent<GameAgent>();
            //验证对象ga是否为空
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
        }
    }

    // Update is called once per frame
    /// <summary>
    /// 点击鼠标上弹触发
    /// </summary>
    private void Update()
    {
        UpdateMousePosition();
        //点击鼠标上弹触发
        if (Input.GetMouseButtonUp(0))
        {
            if (Input.GetKey(KeyCode.Delete))
            {
                //鼠标左键 + 删除键
                DeleteAgent();
            }
            else
            {
                //鼠标左键
                CreatAgent();
            }
        }

        Simulator.Instance.doStep_Update();

    }

    void OnDrawGizmos()
    {
        if (draw)
            Simulator.Instance.DrawObstacles();
    }
}