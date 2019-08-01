﻿//#define VEXE
using UnityEngine;
using System.Collections;
#if VEXE
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
#endif

#if UNITY_5_5 || UNITY_5_4 || UNITY_5_6 
using UnityEngine.SceneManagement;
#endif

namespace KFrameWork
{
    #if VEXE
    public abstract class UnityMonoBehaviour : BaseBehaviour {

        public bool Open_Log =true;

        protected static bool m_isFrameWorkInited =false;

        protected void DestroySelf()
        {
            #if UNITY_EDITOR
            DestroyImmediate(this);
            #else
            Destroy(this);
            #endif
        }

        protected override void Awake()
        {
            if(LogMgr.OpenLog != Open_Log)
            {
                LogMgr.OpenLog = Open_Log;
            }
        }

        protected override void Start () {
            if(!m_isFrameWorkInited)
            {
                GameObject fk = GameObject.Find("KFrameWork");
                if(fk != null && fk.GetComponent<MainLoop>() != null)
                {
                    m_isFrameWorkInited = true;
                }
                else if(fk != null)
                {
                    fk.AddComponent<MainLoop>();
                    m_isFrameWorkInited = true;
                }
                else
                {
                    fk = new GameObject();
                    fk.AddComponent<MainLoop>();
                    m_isFrameWorkInited =true;
                }
            }
        }

        protected override void OnLevelWasLoaded(int level)
        {
            if(!m_isFrameWorkInited && FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("框架为启用，或者未启用base.start的框架对象检查",gameObject);
            }
        }

        protected override void OnTriggerEnter(Collider other){}
        protected override void OnTriggerExit(Collider other){}
        protected override void Update () {}
        protected override void FixedUpdate(){}
        protected override void LateUpdate(){}
        protected override void OnApplicationQuit(){}
        protected override void OnApplicationPause(){}
        protected override void OnEnable(){}
        protected override void OnDisable(){}
        protected override void OnDestroy(){}
    }
    #else
    public abstract class UnityMonoBehaviour : MonoBehaviour
    {

        protected static bool m_isFrameWorkInited = false;

        protected void DestroySelf ()
        {
#if UNITY_5_5 || UNITY_5_4 || UNITY_5_6
            SceneManager.sceneLoaded -= this.OnSceneLoad;
#endif
#if UNITY_EDITOR
            DestroyImmediate(this);
            #else
            Destroy(this);
            #endif
        }

        protected virtual void Awake ()
        {
            if (!m_isFrameWorkInited) {
                MainLoop loop = MainLoop.getInstance();
                if (loop != null) {
                    m_isFrameWorkInited = true;
                } else
                {
                    MainLoop.CreateInstance();
                }
            }

#if UNITY_5_5 || UNITY_5_4 || UNITY_5_6
            SceneManager.sceneLoaded += this.OnSceneLoad;
#endif

        }

        [FrameWorkStartAttribute]
        private static void MainLoopStart(int p)
        {
            m_isFrameWorkInited =true;
        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void Start ()
        {

        }

#if UNITY_5_5 || UNITY_5_4 || UNITY_5_6
        private void OnSceneLoad( Scene scene,LoadSceneMode mode)
        {
            if (!m_isFrameWorkInited && FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogErrorFormat("框架为启用，或者未启用base.start的框架对象检查", gameObject);
            }
        }
#else
        private void OnLevelWasLoaded (int level)
        {
            if (!m_isFrameWorkInited && FrameWorkConfig.Open_DEBUG) {
                LogMgr.LogErrorFormat ("框架为启用，或者未启用base.start的框架对象检查", gameObject);
            }
        }
#endif

    }
#endif
    }


