using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using System;

namespace KFrameWork
{
    [System.Serializable]
    public class KdtreeAsset : ScriptableObject {
        [System.NonSerialized]
        public KdtreeObstacleTreeNode obstacleTree_;

        public List<KdtreeObstacleTreeNode> treenodes = new List<KdtreeObstacleTreeNode>();
        public List<KdtreeObstacle> obstacles = new List<KdtreeObstacle>();

        #region  asset-kdtree to kdtree


        public KdTree CopythisToKdtree(IList<Obstacle> target)
        {
            KdTree  tree = new KdTree();

            Dictionary<int,KdtreeObstacle> dic =new Dictionary<int,KdtreeObstacle>();
            for(int i =0; i < obstacles.Count;++i)
            {
                dic[obstacles[i].id_] =obstacles[i];
            }

            Dictionary<int,KdtreeObstacleTreeNode> treedic = new Dictionary<int, KdtreeObstacleTreeNode>();
            for(int i =0; i < treenodes.Count;++i)
            {
                treedic[treenodes[i].id] =treenodes[i];
            }

            Dictionary<int,Obstacle> obsdic = new Dictionary<int, Obstacle>();

            KdTree.ObstacleTreeNode treenode = Assign2Kdtree(treedic[0],dic,treedic,obsdic);
            ScriptCommand setnode_cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.SET_KDTREE_TREENODE);
            setnode_cmd.CallParams.WriteObject(tree);
            setnode_cmd.CallParams.WriteObject(treenode);
            setnode_cmd.ExcuteAndRelease();

            //assign obstacles
            target.Clear();
            var en = obsdic.GetEnumerator();
            while(en.MoveNext())
            {
                target.Add(en.Current.Value);
            }

            return tree;
        }

        private KdTree.ObstacleTreeNode Assign2Kdtree(KdtreeObstacleTreeNode treenode,Dictionary<int,KdtreeObstacle> dic,Dictionary<int,KdtreeObstacleTreeNode> treedic,Dictionary<int,Obstacle> obsdic)
        {
            if(treenode == null || treenode.id == -1)
                return null;
            
            KdTree.ObstacleTreeNode kdnode = new KdTree.ObstacleTreeNode();
            kdnode.obstacle_ = CreateObstacle(dic[treenode.obstacleID],dic,obsdic);

            KdTree.ObstacleTreeNode left =Assign2Kdtree(treenode.leftID != -1 ? treedic[treenode.leftID]:null ,dic,treedic,obsdic);
            KdTree.ObstacleTreeNode right = Assign2Kdtree(treenode.rightID != -1 ?treedic[treenode.rightID]:null, dic,treedic,obsdic);

            ScriptCommand setleft_cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.SET_LEFT_LEAF);
            ScriptCommand setright_cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.SET_RIGHT_LEAF);
            setleft_cmd.CallParams.WriteObject(kdnode);
            setleft_cmd.CallParams.WriteObject(left);
            setleft_cmd.ExcuteAndRelease();

            setright_cmd.CallParams.WriteObject(kdnode);
            setright_cmd.CallParams.WriteObject(right);
            setright_cmd.ExcuteAndRelease();

            return kdnode;

        }

        private Obstacle CreateObstacle(KdtreeObstacle obs,Dictionary<int,KdtreeObstacle> dic,Dictionary<int,Obstacle> obsdic)
        {
            if(obs != null)
            {
                Obstacle target = new Obstacle();
                target.id_ = obs.id_;
                obsdic[obs.id_] = target;

                if(obs.nextID != -1)
                {
                    if(obsdic.ContainsKey(obs.nextID))
                    {
                        target.next_ = obsdic[obs.nextID];
                    }
                    else
                    {
                        target.next_ =CreateObstacle(dic[obs.nextID],dic,obsdic);
                    }
                }
                else
                {
                    target.next_ = null;
                }


                if(obs.previousID != -1)
                {
                    if(obsdic.ContainsKey(obs.previousID))
                    {
                        target.previous_ = obsdic[obs.previousID];
                    }
                    else
                    {
                        target.previous_ =CreateObstacle(dic[obs.previousID],dic,obsdic);
                    }
                }
                else
                {
                    target.previous_ = null;
                }

                target.direction_ = obs.direction_;
                target.point_ = obs.point_;
                target.convex_ = obs.convex_;
                return target;
            }

            return null;
        }
        #endregion


        #region kdtree to asset-kdtree
        public void CreateKdtree(KdTree tree,IList<Obstacle> obslist)
        {
            treenodes.Clear();
            obstacles.Clear();

            Dictionary<int,KdtreeObstacle> dic =new Dictionary<int,KdtreeObstacle>();
            for(int i =0; i < obslist.Count;++i)
            {
                Obstacle obs = obslist[i];

                KdtreeObstacle kdobs = new KdtreeObstacle();
                kdobs.convex_ =obs.convex_;
                kdobs.id_ =obs.id_;
                kdobs.point_ =obs.point_;
                kdobs.direction_ =obs.direction_;
                kdobs.nextID =obs.next_.id_;
                kdobs.previousID =obs.previous_.id_;
                obstacles.Add(kdobs);

                dic[obs.id_]= kdobs;
            }


            obstacleTree_ = AssignTreeNode(tree.obstacleTree_,dic);

            LogMgr.Log("dic "+ dic.Count);
        }

        private KdtreeObstacleTreeNode AssignTreeNode(KdTree.ObstacleTreeNode kdnode, Dictionary<int,KdtreeObstacle> obslist)
        {
            if(kdnode == null || kdnode.obstacle_ == null)
            {
                return  null;
            }
            KdtreeObstacleTreeNode treenode = new KdtreeObstacleTreeNode();
            treenode.id = this.treenodes.Count;
            treenodes.Add(treenode);

            treenode.obstacle_ = obslist[kdnode.obstacle_.id_];
            treenode.obstacleID = treenode.obstacle_ .id_;

            treenode.left_ = AssignTreeNode(kdnode.left_,obslist);
            treenode.right_ = AssignTreeNode(kdnode.right_,obslist);

            if(treenode.left_  != null)
            {
                treenode.leftID = treenode.left_.id;
            }
            else
            {
                treenode.leftID =-1;
            }

            if(treenode.right_  != null)
            {
                treenode.rightID = treenode.right_.id;
            }
            else
            {
                treenode.rightID =-1;
            }
            return treenode;

        }
            
        #endregion

    }
}



