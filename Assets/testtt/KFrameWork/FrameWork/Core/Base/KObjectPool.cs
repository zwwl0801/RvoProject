using System;
using System.Collections;
using System.Collections.Generic;
using KFrameWork;
using UnityEngine;

namespace KUtils
{
    /// <summary>
    /// 对象池接口，尽量不要在外部持有ipool对象的引用，有的话 请手动保证对象在被清楚的时候对象
    /// </summary>
    public interface IPool
    {
        void RemoveToPool();
    }

    /// <summary>
    /// 引用类型对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    public sealed class KObjectPool
    {
        private static KObjectPool _mIns;
        public static KObjectPool mIns
        {
            get
            {
                if (_mIns == null)
                    _mIns = new KObjectPool();
                return _mIns;
            }
        }

        public const int EachRemoveCount = 4;

        public const float RemoveDeltaTime = 30f;

        private Dictionary<Type, Queue<System.Object>> queue = new Dictionary<Type, Queue<System.Object>>(16);

        [FrameWorkStart]
        static void StartPoolSechedule(int value)
        {
            Schedule.mIns.ScheduleRepeatInvoke(0f, RemoveDeltaTime, -1, null, RmoveOld);
        }

        static void RmoveOld(System.Object o,int left)
        {
            var en = mIns.queue.GetEnumerator();
            while(en.MoveNext())
            {
                Queue<System.Object> objs = en.Current.Value;
                if(objs.Count >0)
                {
                    int removecnt = Mathf.Min(objs.Count, EachRemoveCount);
                    while(removecnt >0)
                    {
                        objs.Dequeue();
                        removecnt--;
                    }
                }
            }
        }

        public void Push(System.Object data)
        {
            if (data == null)
                return;

            Type tp = data.GetType();
            if (!this.queue.ContainsKey(tp))
            {
                this.queue.Add(tp, new Queue<System.Object>(8));
            }
            #if UNITY_EDITOR
            if(FrameWorkConfig.Open_DEBUG)
            {
                if(this.queue[tp].Contains(data))
                {
                    LogMgr.LogErrorFormat("重复添加 :{0}",data);
                }
            }
            #endif
            
            this.queue[tp].Enqueue(data);
            if (data is IPool)
            {
                (data as IPool).RemoveToPool();
            }

            if (FrameWorkConfig.Open_DEBUG)
            {
                LogMgr.LogFormat("{0}进入缓存池 ", data);
            }
        }
            
        public void Clear()
        {
            this.queue.Clear();
        }

        public object Pop(Type tp)
        {
            if (this.queue.ContainsKey(tp))
            {
                Queue<System.Object> list = this.queue[tp];

                if (list.Count == 0)
                {
                    return null;
                }
                else
                {
                    return list.Dequeue();
                }
            }

            return null;
        }

        /// <summary>
        /// Pop this instance.(不使用New（） 因为这个会调用反射的Activator.CreateInstance 产生GC)
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Pop<T>()
        {
            Type tp = typeof(T);
            return (T)Pop(tp);
        }

    }
}

