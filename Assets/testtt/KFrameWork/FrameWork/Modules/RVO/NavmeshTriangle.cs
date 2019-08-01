using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KFrameWork
{
    public class NavmeshTriangle
    {
#if UNITY_EDITOR
        public int uid;
#endif
        /// <summary>
        /// 三角形中点
        /// </summary>
        public KInt3 averagePos;

        public KInt2 xzPos;
        //vert idx for 0
        public int v0;
        //vert idx for 1
        public int v1;
        //vert idx for 2
        public int v2;

        public KInt3 v03;

        public KInt2 v03xz;

        public KInt3 v13;

        public KInt2 v13xz;

        public KInt3 v23;

        public KInt2 v23xz;

        /// <summary>
        /// 连接点
        /// </summary>
        public List<NavmeshTriangle> connections;

        public List<bool> shared;

        public Segment GetSegment(int idx)
        {
            if(idx >2 || idx <0)
            {
                throw new FrameWorkArgumentException("Get Segment Error");
            }

            if(idx ==0)
            {
                return new Segment(v03xz,v13xz);
            }
            else if(idx== 1)
            {
                return new Segment(v13xz,v23xz);
            }
            return new Segment(v23xz,v03xz);
        }

        public void GetSegmentWithPoint(int idx,out KInt2 p1,out KInt2 p2)
        {
            if(idx >2 || idx <0)
            {
                throw new FrameWorkArgumentException("Get Segment Error");
            }

            if(idx ==0)
            {
                p1 =v03xz ;
                p2 =v13xz;
            }
            else if(idx== 1)
            {
                p1 =v13xz ;
                p2= v23xz;
            }
            p1 =v23xz ;
            p2 =v03xz;
        }

        public void GetSegmentWithPlace(int idx,out int p1,out int p2)
        {
            if(idx >2 || idx <0)
            {
                throw new FrameWorkArgumentException("Get Segment Error");
            }

            if(idx ==0)
            {
                p1 =0 ;
                p2 =1;
            }
            else if(idx== 1)
            {
                p1 =1 ;
                p2= 2;
            }
            p1 =2;
            p2 =0;
        }

        public void CreateSharedInfo()
        {
            shared = new List<bool>(3);
            for(int i =0; i < 3;++i)
                shared.Add(false);

            for (int k = 0; k < this.connections.Count; ++k) {
                int hitidx;
                if (this.isSharedEdge (this.connections [k], out hitidx) ) {
                    shared[hitidx] = true;
                }
            }
        }


        public int SharedEdge(NavmeshTriangle other)
        {
            int a, b;

            GetPortal(other, out a, out b);
            return a;
        }

        public int GetVertexIndex(int i)
        {
            return i == 0 ? v0 : (i == 1 ? v1 : v2);
        }

        public KInt3 GetVertex(int i)
        {
            return i == 0 ? v03 : (i == 1 ? v13 : v23);
        }

        public KInt2 GetXZVertex(int i)
        {
            return i == 0 ? v03xz : (i == 1 ? v13xz : v23xz);
        }

        public bool isSharedEdge(NavmeshTriangle other, out int idx)
        {
            for (int i = 0; i < 3; ++i)
            {
                KInt2 p1 = GetXZVertex(i);
                KInt2 p2 = GetXZVertex((i+1 )%3);
                for (int k = 0; k < 3; ++k)
                {
                    KInt2 p3 = other.GetXZVertex(k);
                    KInt2 p4 = other.GetXZVertex((k+1 )%3);
                    if ( (p1 == p3 && p2== p4) || (p1 == p4 && p2 == p3))
                    {
                        idx =i;
                        return true;
                    }
                }
            }
            idx =-1;
            return false;

        }

        public void GetPortal(NavmeshTriangle other, out int aIndex, out int bIndex)
        {
            int first = -1;
            int second = -1;

            for (int a = 0; a < 3; a++)
            {
                int va = GetVertexIndex(a);
                for (int b = 0; b < 3; b++)
                {
                    if (va == other.GetVertexIndex((b + 1) % 3) && GetVertexIndex((a + 1) % 3) == other.GetVertexIndex(b))
                    {
                        first = a;
                        second = b;
                        a = 3;
                        break;
                    }
                }
            }

            aIndex = first;
            bIndex = second;
        }
    }

}

