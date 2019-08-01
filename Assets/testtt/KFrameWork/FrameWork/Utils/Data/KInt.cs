using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KFrameWork
{
    [Serializable]
    public struct KInt : IEquatable<KInt>,IComparable<KInt>,IComparable
    {
        public const int divscale = 1000;

        private const int div2scale = divscale * divscale;

        private long longvalue;

        public static KInt MaxValue = new KInt(int.MaxValue);

        public static KInt Zero = new KInt(0);

        public long IntValue
        {
            get
            {
                return longvalue;
            }
        }

        public float floatvalue
        {
            get
            {
                return (float)longvalue / divscale;
            }
        }

        public int IntSqr
        {
            get
            {
                return (int)(longvalue * longvalue / div2scale);
            }
        }

        public KInt IntSqrt
        {
            get
            {
                long value = KMath.isqrt(longvalue * divscale);
                return new KInt(value );
            }
        }

        public static KInt Max(KInt left, KInt right)
        {
            if (left > right)
            {
                return left;
            }
            return right;
        }

        public static KInt Min(KInt left, KInt right)
        {
            if (left > right)
            {
                return right;
            }
            return left;
        }

        private KInt(long value)
        {
            this.longvalue = value;
        }

        public override string ToString()
        {
            return string.Format("[{0}]",this.floatvalue);
        }

        public static KInt ToInt(int data)
        {
            KInt value = new KInt(data);
            return value;
        }

        public static KInt ToInt(long data)
        {
            KInt value = new KInt(data);
            return value;
        }

        public static implicit operator KInt(float data)
        {
            KInt newdata = new KInt();
            newdata.longvalue = Mathf.RoundToInt(data * divscale);
            return newdata;
        }

        #region add
        public static KInt operator +(KInt left, KInt right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue + right.longvalue;
            return value;
        }

        public static KInt operator +(KInt left, int right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue + right * divscale;
            return value;
        }

        public static KInt operator +(KInt left, short right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue + right * divscale;
            return value;
        }

        public static KInt operator +(KInt left, long right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue + right * divscale;
            return value;
        }

        public static int operator +(int left, KInt right)
        {
            return (int)((left * divscale + right.longvalue)/divscale);
        }

        public static short operator +(short left, KInt right)
        {
            return (short)((left * divscale + right.longvalue) / divscale);
        }

        public static long operator +(long left, KInt right)
        {
            return (long)((left * divscale + right.longvalue) / divscale);
        }
        #endregion
        #region reduce

        public static KInt operator -(KInt left, KInt right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue - right.longvalue;
            return value;
        }

        public static KInt operator -(KInt left, int right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue - right * divscale;
            return value;
        }

        public static KInt operator -(KInt left, short right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue - right * divscale;
            return value;
        }

        public static KInt operator -(KInt left, long right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue - right * divscale;
            return value;
        }

        public static int operator -(int left, KInt right)
        {
            return (int)((left * divscale - right.longvalue) / divscale);
        }

        public static short operator -(short left, KInt right)
        {
            return (short)((left * divscale - right.longvalue) / divscale);
        }

        public static long operator -(long left, KInt right)
        {
            return (long)((left * divscale - right.longvalue) / divscale);
        }

        #endregion

        #region multi

        public static KInt operator *(KInt left, KInt right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue * right.longvalue / divscale;
            return value;
        }

        public static KInt operator *(KInt left, int right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue * right;
            return value;
        }

        public static KInt operator *(KInt left, short right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue * right;
            return value;
        }

        public static KInt operator *(KInt left, long right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue * right;
            return value;
        }

        public static int operator *(int left, KInt right)
        {
            return (int)((left *  right.longvalue) / divscale);
        }

        public static short operator *(short left, KInt right)
        {
            return (short)((left *  right.longvalue) / divscale);
        }

        public static long operator *(long left, KInt right)
        {
            return (int)((left * right.longvalue) / divscale);
        }

        #endregion

        #region div

        public static KInt operator /(KInt left, KInt right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue * divscale / right.longvalue;
            return value;
        }

        public static KInt operator /(KInt left, short right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue / right;
            return value;
        }

        public static KInt operator /(KInt left, int right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue / right;
            return value;
        }

        public static KInt operator /(KInt left, long right)
        {
            KInt value = new KInt();
            value.longvalue = left.longvalue / right;
            return value;
        }

        public static short operator /(short left, KInt right)
        {
            return (short)(left * divscale / right.longvalue);
        }

        public static int operator /(int left, KInt right)
        {
            return (int)(left * divscale / right.longvalue);
        }

        public static long operator /(long left, KInt right)
        {
            return (long)(left * divscale / right.longvalue);
        }

        #endregion
        #region >
        public static bool operator >(KInt left, KInt right)
        {
            return left.longvalue > right.longvalue;
        }

        public static bool operator >(KInt left, short right)
        {
            return left.longvalue > right * divscale;
        }

        public static bool operator >(short left, KInt right)
        {
            return left * divscale > right ;
        }

        public static bool operator >(int left, KInt right)
        {
            return left * divscale > right;
        }

        public static bool operator >(long left, KInt right)
        {
            return left * divscale > right;
        }

        public static bool operator >(KInt left, int right)
        {
            return left.longvalue > right * divscale;
        }

        public static bool operator >(KInt left, long right)
        {
            return left.longvalue > right * divscale;
        }
        #endregion

        #region <
        public static bool operator <(KInt left, KInt right)
        {
            return left.longvalue < right.longvalue;
        }

        public static bool operator <(KInt left, short right)
        {
            return left.longvalue < right * divscale;
        }

        public static bool operator <(KInt left, int right)
        {
            return left.longvalue < right * divscale;
        }

        public static bool operator <(KInt left, long right)
        {
            return left.longvalue < right * divscale;
        }

        public static bool operator <(short left, KInt right)
        {
            return left * divscale < right.longvalue;
        }

        public static bool operator <(int left, KInt right)
        {
            return left * divscale < right.longvalue;
        }

        public static bool operator <(long left, KInt right)
        {
            return left * divscale < right.longvalue;
        }

        #endregion
        #region >=
        public static bool operator >=(KInt left, KInt right)
        {
            return left.longvalue >= right.longvalue;
        }

        public static bool operator >=(KInt left, int right)
        {
            return left.longvalue >= right * divscale;
        }

        public static bool operator >=(KInt left, short right)
        {
            return left.longvalue >= right * divscale;
        }

        public static bool operator >=(KInt left, long right)
        {
            return left.longvalue >= right * divscale;
        }

        public static bool operator >=(short left, KInt right)
        {
            return left * divscale >= right.longvalue;
        }

        public static bool operator >=(int left, KInt right)
        {
            return left * divscale >= right.longvalue;
        }

        public static bool operator >=(long left, KInt right)
        {
            return left * divscale >= right.longvalue;
        }
        #endregion

        #region <=

        public static bool operator <=(KInt left, KInt right)
        {
            return left.longvalue <= right.longvalue;
        }

        public static bool operator <=(KInt left, int right)
        {
            return left.longvalue <= right * divscale;
        }

        public static bool operator <=(KInt left, short right)
        {
            return left.longvalue <= right * divscale;
        }

        public static bool operator <=(KInt left, long right)
        {
            return left.longvalue <= right * divscale;
        }

        public static bool operator <=(short left, KInt right)
        {
            return left * divscale <= right.longvalue;
        }

        public static bool operator <=(int left, KInt right)
        {
            return left * divscale <= right.longvalue;
        }

        public static bool operator <=(long left, KInt right)
        {
            return left * divscale <= right.longvalue;
        }
        #endregion

        public static KInt operator -(KInt value)
        {
            KInt data = new KInt();
            data.longvalue = -value.longvalue;
            return data;
        }


        public static bool operator ==(KInt left, KInt right)
        {
            return left.longvalue == right.longvalue;
        }

        public static bool operator !=(KInt left, KInt right)
        {
            return left.longvalue != right.longvalue;
        }

        public bool Equals(KInt other)
        {
            return this.longvalue == other.longvalue;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(KInt value)
        {
            if (this == value)
            {
                return 0;
            }
            if (this > value)
            {
                return 1;
            }
            return -1;
        }

        public int CompareTo(object obj)
        {
            if (obj is KInt)
            {
                KInt value = (KInt)obj;
                if (this == value)
                {
                    return 0;
                }
                if (this > value)
                {
                    return 1;
                }
            } 


            return -1;
        }
    }


}

