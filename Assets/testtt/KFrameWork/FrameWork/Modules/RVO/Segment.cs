using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KFrameWork
{

    public struct Segment : IEquatable<Segment>
    {
        public readonly KInt2 start;
        public readonly KInt2 end;
        public int uid ;

        public readonly long MinX;

        public readonly long MaxX;

        public readonly long MinY;

        public readonly long MaxY;

        private KInt2? cachedir;

        public KInt2 direction
        {
            get
            {
                if(cachedir == null)
                    cachedir = (end - start).normalized;
                return cachedir.Value;
            }
        }

        public Segment(KInt2 first, KInt2 second)
        {
            this.start = first;
            this.end = second;
            cachedir = null;
            uid = 0;


            this.MinX = KMath.Min(this.start.IntX,this.end.IntX);
            this.MaxX = KMath.Max(this.start.IntX,this.end.IntX);
            this.MinY = KMath.Min(this.start.IntY,this.end.IntY);
            this.MaxY = KMath.Max(this.start.IntY,this.end.IntY);
        }

        public Segment(KInt2 first, KInt2 second,int id)
        {
            this.start = first;
            this.end = second;
            cachedir = null;
            uid = id;

            this.MinX = KMath.Min(this.start.IntX,this.end.IntX);
            this.MaxX = KMath.Max(this.start.IntX,this.end.IntX);
            this.MinY = KMath.Min(this.start.IntY,this.end.IntY);
            this.MaxY = KMath.Max(this.start.IntY,this.end.IntY);
        }

        public bool isConnect(Segment other)
        {
            if(this.Equals(other))
            {
                return false;
            }

            if(this.start == other.start || this.end == other.end)
            {
                if(this.direction == other.direction)
                {
                    return false;
                }
                return true;
            }
            else if(this.start == other.end  || this.end == other.start)
            {
                if ( this.direction == -other.direction )
                {
                    return false;
                }
                return true;

            }
            return false;
        }

        static long cross (KInt2 p1, KInt2 p2, KInt2 p3)
        {
            return (p1.IntX * (p2.IntY - p3.IntY) + p2.IntX * (p3.IntY - p1.IntY) + p3.IntX * (p1.IntY - p2.IntY));
        }

        public static bool isConnectRetPoints(KInt2 selfstart,KInt2 selfend, KInt2 otherstart,KInt2 otherend,out KInt2 p1,out KInt2 p2,out KInt2 p3)
        {
            p1 = selfstart;
            p2 = selfend;
            p3 = selfstart;

            bool b1 = selfstart == otherstart;
            bool b2 = selfend == otherend;

            bool b3 = selfstart == otherend;
            bool b4 = selfend == otherstart;
            KInt2 dir =(otherend-otherstart).normalized;
            KInt2 selfdir =(selfend - selfstart).normalized;

            if( (b1 && b2 )|| (b3 && b4))
            {
                return false;
            }
            else if(b1 || b2)
            {
                p1 = b1 ? selfstart:selfend;
                p2 = b1 ? selfend :selfstart;
                p3 = b1 ? otherend:otherstart;

                if(selfdir == dir)
                {
                    return false;
                }
                return true;
            }
            else if(b3 || b4)
            {
                p1 = b3?selfstart:selfend;
                p2 = b3? selfend :selfstart;
                p3 = b3 ? otherstart:otherend;

                if ( selfdir == -dir )
                {
                    return false;
                }
                return true;

            }
            return false;
        }


        public static bool isConnectRetPoints(Segment self, Segment other,out KInt2 p1,out KInt2 p2,out KInt2 p3)
        {

            return isConnectRetPoints(self.start,self.end, other.start,other.end,out p1,out p2,out p3);
        }

        public static bool Intersect(KInt2 p1, KInt2 p2, KInt2 p3, KInt2 p4, out KInt2 result)
        {
            result = KInt2.zero;
            float s1_x = p2.x - p1.x;
            float s1_y = p2.y - p1.y;
            float s2_x = p4.x - p3.x;
            float s2_y = p4.y - p3.y;

            float v1 = -s2_x * s1_y + s1_x * s2_y;
            float v2 = -s2_x * s1_y + s1_x * s2_y;
            float s = (-s1_y * (p1.x - p3.x) + s1_x * (p1.y - p3.y)) /v1;
            float t = (s2_x * (p1.y - p3.y) - s2_y * (p1.x - p3.x)) /v2;

            if (s >= 0 && s <= 1 &&t >= 0 && t <= 1)
            {
                // Collision detected
                result = new KInt2((p1.x+ t * s1_x), +(p1.y  + t * s1_y));
                return true;
            }

            return false; // No collision
        }
            

        private static bool InRange(KInt2 p1,KInt2 k1,KInt2 k2)
        {
            if (( (p1.IntX >= k1.IntX && p1.IntX <= k2.IntX) || (p1.IntX >= k2.IntX && p1.IntX <= k1.IntX))
                && ((p1.IntY >= k1.IntY && p1.IntY <= k2.IntY) || (p1.IntY >= k2.IntY && p1.IntY <= k1.IntY)))
            {
                return true;
            }
            return false;
        }

        public bool ContainsSegment(Segment segment)
        {
            KInt2 selfdir = this.direction;
            KInt2 otherdir = segment.direction;
            if (selfdir == otherdir || selfdir == -otherdir)
            {
                return InRange(segment.start, this.start, this.end) && InRange(segment.end, this.start, this.end);
            }
            return false;
        }

        public bool ContainsPoint(KInt2 point)
        {
            return this.start == point || this.end == point;
        }


        public bool Equals(KInt2 first, KInt2 second)
        {
            if ((start == first && end == second) || (end == first && start == second))
            {
                return true;
            }
            return false;
        }

        public bool Equals(Segment other)
        {
            if ((start == other.start && end == other.end) || (end == other.start && start == other.end))
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("线段 Start:[{0}] End:[{1}] Uid:{2}",this.start,this.end,uid);
        }

    }
}


