using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;

namespace KFrameWork
{
    public class LoadKdtree : UnityMonoBehaviour {

        public KdtreeAsset asset;

        protected override  void Start()
        {
            this.Load(asset);
        }

        public void Load(KdtreeAsset treeasset)
        {
            if(treeasset != null)
            {
                float time = Time.realtimeSinceStartup;
                Simulator.Instance.CreateKdtreeFromAsset(treeasset);
                LogMgr.LogFormat("Load  cost :{0}",Time.realtimeSinceStartup - time);
               
                this.asset = treeasset;
            }
        }



    } 
}


