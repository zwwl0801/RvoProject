//using System;
//using UnityEngine;
//#if UNITY_NAVMESH
//using UnityEngine.AI;
//#else
//using RVO;
//#endif

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace  KFrameWork
//{
//    public interface AgentAttribute
//    {

//#if UNITY_NAVMESH
//        /// <summary>
//        /// get the agentTypeID of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        int getagentTypeID();
//        /// <summary>
//        /// get the speed of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        KInt getagentSpeed();
//        /// <summary>
//        ///  Stop within this distance from the target position.
//        /// </summary>
//        /// <returns></returns>
//        KInt getagentstopdistance();
//        /// <summary>
//        /// get the height of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        KInt getagentHeight();
//        /// <summary>
//        /// get the  relative vertical displacement  of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        KInt getagentOffset();
//        /// <summary>
//        /// get angularSpeed of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        KInt getangularSpeed();
//#else
//        /// <summary>
//        /// get the number of a specified agent;
//        /// </summary>
//        /// <returns></returns>
//        int getagentNo();
//        /// <summary>
//        /// Returns the maximum neighbor distance of a specified agent.
//        /// </summary>
//        /// <returns></returns>
//        KInt getneighborDist();
//        /// <summary>
//        /// get the maximum neighbor count of a specified agent.
//        /// </summary>
//        /// <returns></returns>
//        int getmaxNeighbors();
//        /// <summary>
//        /// Returns the time horizon of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        KInt gettimeHorizon();
//        /// <summary>
//        /// Returns the time horizon with respect to obstacles of a* specified agent.
//        /// </summary>
//        /// <returns></returns>
//        KInt gettimeHorizonObst();

//        /// <summary>
//        /// get the maximum speed of a specified agent.
//        /// </summary>
//        /// <returns></returns>
//        KInt getmaxSpeed();

//        /// <summary>
//        /// update the ground height
//        /// </summary>
//        /// <returns></returns>
//        KInt UpdateGroundY(KInt2 pos);
//#endif

//        /// <summary>
//        /// set the feet position of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        void setFeetPosition(KInt3 feetpos);
//        /// <summary>
//        /// set the rotation of a specified agent
//        /// </summary>
//        /// <param name="ratation"></param>
//        void setRotation(KInt3 ratation);
//        /// <summary>
//        /// get the feet position of a specified agent
//        /// </summary>
//        /// <returns></returns>
//        KInt3 getFeetPosition();

//        /// <summary>
//        /// Returns the radius of a specified agent.
//        /// </summary>
//        /// <returns></returns>
//        KInt getradius();
//        /// <summary>
//        /// get the two-dimensional linear velocity of a specified * agent.
//        /// </summary>
//        /// <returns></returns>
//        KInt2 getdefaultvelocity();
//    }


//    public class UnityGameAgent<T> where T : MonoBehaviour, AgentAttribute
//    {
//        private T target;

//        private bool _enableMove = true;
//        public bool enableMove
//        {
//            get
//            {
//                return _enableMove;
//            }
//            set
//            {
//                if (_enableMove != value)
//                {
//                    _enableMove = value;
//#if UNITY_NAVMESH
//                    agent.updatePosition = value;
//#endif
//                    if(!value)
//                    {
//                        this.Stop();
//                    }
//                    else
//                    {
//                        this.ReStart();
//                    }
//                }
//            }
//        }

//        private bool _enableRat = true;

//        public bool enableRat
//        {
//            get
//            {
//                return _enableRat;
//            }
//            set
//            {
//                if (_enableRat != value)
//                {
//                    _enableRat = value;
//#if UNITY_NAVMESH
//                    agent.updateRotation = value;
//#endif
//                }
//            }
//        }

//#if UNITY_NAVMESH
//        public const int Humanoid = 0;

//        public const int walkablelayer = 0;
//        public const int Notwalkablelayer = 1;
//        public const int jumplayer = 2;

//        private NavMeshAgent agent;
//        private NavMeshPath _agentPath;

//        private NavMeshPath agentPath
//        {
//            get
//            {
//                if(_agentPath == null)
//                {
//                    _agentPath = new NavMeshPath();
//                }
//                return _agentPath;
//            }
//        }
//#else

//        private int agentNumber = 0;

//        private KInt groundY;
//#endif

//        private KInt3 lastPosition;

//        public void Create(T value)
//        {
//            target = value;
//            agentNumber = target.getagentNo();
//        }

//        public void AwakeAgent()
//        {
//            lastPosition = target.getFeetPosition();
//#if UNITY_NAVMESH
//            agent = target.gameObject.AddComponent<NavMeshAgent>();
//            agent.agentTypeID = target.getagentTypeID();
//            agent.speed = target.getagentSpeed().floatvalue;
//            agent.angularSpeed =target.getangularSpeed().floatvalue;
//            agent.radius = target.getradius().floatvalue;
//            agent.height = target.getagentHeight().floatvalue;
//            agent.baseOffset = target.getagentOffset().floatvalue;
//            agent.stoppingDistance =target.getagentstopdistance().floatvalue;
//            agent.autoBraking = false;
//            agent.autoRepath = true;
//            agent.autoTraverseOffMeshLink = false;
//            agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
//            agent.avoidancePriority = 50;
//            agent.areaMask = 1 << walkablelayer | 1 << jumplayer;

//            KInt2 vel = target.getdefaultvelocity();
//            agent.velocity = new Vector3(vel.x,0, vel.y);
//            this.Teleport(target.getFeetPosition());
//#else
//            KInt2 feetpos = KInt2.ToInt2(target.getFeetPosition().IntX, target.getFeetPosition().IntZ);
//            Simulator.Instance.addAgent(feetpos, target.getneighborDist(), target.getmaxNeighbors(), target.gettimeHorizon(), target.gettimeHorizonObst(), target.getradius(), target.getmaxSpeed(), target.getdefaultvelocity());
//#endif
//        }

//        public KInt GetRadius()
//        {
//            return target.getradius();
//        }

//        public KInt2 getPrefVelocity()
//        {
//            if(this.enableMove == false)
//            {
//                return KInt2.zero;
//            }

//#if UNITY_NAVMESH
//            if (!agent.isActiveAndEnabled)
//            {
//                LogMgr.LogError("Not acticve");
//                return KInt2.zero;
//            }

//            return new KInt2( agent.desiredVelocity.x,agent.desiredVelocity.z);
//#else
//            return Simulator.Instance.getAgentPrefVelocity(agentNumber);
//#endif
//        }

//        public void Move(KInt3 gamevelocity)
//        {
//            if (this.enableMove == false)
//            {
//                return;
//            }
//#if UNITY_NAVMESH
//            if (!agent.isActiveAndEnabled)
//            {
//                LogMgr.LogError("Not acticve");
//                return ;
//            }

//            agent.velocity = gamevelocity;
//#else
//            Simulator.Instance.setAgentPrefVelocity(agentNumber, KInt2.ToInt2(gamevelocity.IntX, gamevelocity.IntZ));
//#endif
//        }

//        public void Teleport(KInt3 pos)
//        {
//            this.lastPosition = target.getFeetPosition();
//#if UNITY_NAVMESH
//            if (!agent.isActiveAndEnabled)
//            {
//                LogMgr.LogError("Not acticve");
//                return;
//            }

//            if (agent.updatePosition)
//            {
//                agent.ResetPath();
//                agent.velocity = Vector3.zero;
//                agent.nextPosition = pos;
//            }
//            else
//            {
//                LogMgr.LogErrorFormat("{0} cant update Position", target);
//            }
//#else
//            Simulator.Instance.setAgentPosition(this.agentNumber,KInt2.ToInt2(pos.IntX,pos.IntZ));
//#endif
//        }

//        public void Stop()
//        {

//#if UNITY_NAVMESH
//            if (!agent.isActiveAndEnabled)
//            {
//                LogMgr.LogError("Not acticve");
//                return;
//            }

//            agent.velocity = Vector3.zero;
//            agent.isStopped = true;
//#else
//            Simulator.Instance.setAgentPrefVelocity(agentNumber, KInt2.zero);
//#endif
//        }

//        public void ReStart()
//        {
//#if UNITY_NAVMESH
//            if (!agent.isActiveAndEnabled)
//            {
//                LogMgr.LogError("Not acticve");
//                return;
//            }
//            agent.isStopped = false;
//#endif
//        }

//        public void UpdateInFrame(int cnt)
//        {
//#if UNITY_NAVMESH
//            if(!agent.isOnNavMesh)
//            {
//                LogMgr.LogError("对象不再navmesh 上");
//            }

//            if(agent.isOnOffMeshLink)
//            {
//                LogMgr.LogError("对象在OffMeshLink上");
//            }

//            if(enableMove && agent.isActiveAndEnabled)
//            {
//                KInt3 finishpos = (KInt3)agent.nextPosition;
//                target.setFeetPosition(finishpos);
//                lastPosition = finishpos;

//                if (enableRat)
//                {
//                    target.setRotation((KInt3)agent.desiredVelocity);
//                }
//            }

//#else
//            if(enableMove)
//            {
//                KInt2 pos = Simulator.Instance.getAgentPosition(agentNumber);
//                //update y coordinate
//                this.groundY = target.UpdateGroundY(pos);

//                KInt3 finishpos = KInt3.ToInt3(pos.IntX, groundY * KInt3.divscale, pos.IntY);
//                target.setFeetPosition(finishpos);

//                if (enableRat)
//                {
//                    KInt2 vel = Simulator.Instance.getAgentPrefVelocity(agentNumber);
//                    if (Math.Abs(vel.x) > 0.01f && Math.Abs(vel.y) > 0.01f)
//                    {
//                        target.setRotation(new KInt3(vel.x, finishpos.y- lastPosition.y, vel.y));
//                    }
//                }

//                lastPosition = finishpos;
//            }
//#endif
//        }

//    }
//}



