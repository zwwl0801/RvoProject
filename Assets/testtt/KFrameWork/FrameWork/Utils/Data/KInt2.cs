using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KFrameWork
{
    [Serializable]
    /// <summary>
    /// 剔除vector2操作中的隐式转换，提倡在kint系列中多用kint，而不是vector，减少vector的使用,减少数据差异的可能性 ，set, up 会自动进行转换，_x,Y,Z系列则直接覆盖
    /// </summary>
    public struct KInt2 : IEquatable<KInt2>
    {
        [SerializeField]
        private long _x;
        [SerializeField]
        private long _y;

        public const float scale = 1f /divscale;
        public const int divscale = 1000;

        public const long div2scale = divscale * divscale;

        public static readonly KInt2 zero = new KInt2(0, 0);
        public static readonly KInt2 one = new KInt2(1, 1);

        public static readonly KInt2 min = KInt2.ToInt2(long.MinValue,long.MinValue);
        public static readonly KInt2 max = KInt2.ToInt2(long.MaxValue, long.MaxValue);

        public float this[int idx]
        {
            get
            {
                if (idx > 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    if (idx == 0)
                    {
                        return this.x;
                    }
                    return y;
                }
            }
            set
            {
                if (idx > 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    if (idx == 0)
                    {
                         this._x = (int)(value * divscale);
                    }
                    this._y = (int)(value * divscale);
                }
            }
        }

        public float x
        {
            get
            {
                return _x * scale;
            }
        }

        public long IntX
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public float y
        {
            get
            {
                return _y * scale;
            }

        }

        public long IntY
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public float magnitude
        {
            get
            {
                float xx = _x * scale * _x * scale;
                float yy = _y * scale * _y * scale;
                return Mathf.Sqrt(xx + yy);
            }
        }

        public int IntMagnitude
        {
            get
            {
                return getx2y2()/divscale;
            }
        }

        public KInt2 normalized
        {
            get
            {
                int len = getx2y2();
                if (len == 0)
                {
                    return KInt2.zero;
                }

                KInt2 normlize = new KInt2();
                normlize._x = _x * divscale / len;
                normlize._y = _y * divscale / len;

                return normlize;
            }
        }


        public float sqrMagnitude
        {
            get
            {
                float xx = _x * scale * _x * scale;
                float yy = _y * scale * _y * scale;
                return xx + yy;
            }
        }

        public int IntsqrMagnitude
        {
            get
            {
                long xx = _x * _x / (div2scale);
                long yy = _y * _y / (div2scale);

                return (int)(xx + yy);
            }
        }

        private int getx2y2()
        {
            long xy = _x * _x + _y * _y;
            xy = xy < 0 ? -xy : xy;

            int v = KMath.isqrt(xy);
            return v;
        }

        private KInt2(int kx, int ky)
        {
            this._x = kx * divscale;
            this._y = ky * divscale;
        }

        public KInt2(float kx, float ky)
        {
            this._x = (int)(kx * divscale);
            this._y = (int)(ky * divscale);
        }

        public KInt2(Vector3 pos)
        {
            this._x = (int)(pos.x * divscale);
            this._y = (int)(pos.y * divscale);
        }

        public static KInt2 ToInt2(int x, int y)
        {
            KInt2 value = new KInt2();
            value._x = x;
            value._y = y;
            return value;
        }

        public static KInt2 ToInt2(long x, long y)
        {
            KInt2 value = new KInt2();
            value._x = x;
            value._y = y;
            return value;
        }

        public static KInt2 ToInt2(KInt x, KInt y)
        {
            KInt2 value = new KInt2();
            value._x = x.IntValue * divscale / KInt.divscale ;
            value._y = y.IntValue * divscale / KInt.divscale ;
            return value;
        }

        public void Copy(int px, int py)
        {
            this._x = px;
            this._y = py;
        }

        public void UpdateX(int dx)
        {
            this._x += dx * divscale;
        }

        public void UpdateX(short dx)
        {
            this._x += dx * divscale;
        }

        public void UpdateX(long dx)
        {
            this._x += (int)(dx * divscale);
        }

        public void UpdateX(float dx)
        {
            this._x += (int)(dx * divscale);
        }

        public void UpdateY(int dy)
        {
            this._y += dy * divscale;
        }

        public void UpdateY(short dy)
        {
            this._y += dy * divscale;
        }

        public void UpdateY(long dy)
        {
            this._y += (int)(dy * divscale);
        }

        public void UpdateY(float dy)
        {
            this._y += (int)(dy * divscale);
        }

        /// set
        /// 
        public void SetX(int dx)
        {
            this._x = dx * divscale;
        }

        public void SetX(short dx)
        {
            this._x = dx * divscale;
        }

        public void SetX(long dx)
        {
            this._x = (int)(dx * divscale);
        }

        public void SetX(float dx)
        {
            this._x = (int)(dx * divscale);
        }

        public void SetY(int dy)
        {
            this._y = dy * divscale;
        }

        public void SetY(short dy)
        {
            this._y = dy * divscale;
        }

        public void SetY(long dy)
        {
            this._y = (int)(dy * divscale);
        }

        public void SetY(float dy)
        {
            this._y = (int)(dy * divscale);
        }

        public static KInt2 Lerp(KInt2 a, KInt2 b, float f)
        {
            KInt2 data = new KInt2();
            data._x = (long)(a._x * (1f - f) + (b._x * f));
            data._y = (long)(a._y * (1f - f) + (b._y * f));
            return data;
        }

        public static KInt2 Lerp(KInt2 a, KInt2 b, int percent, int max)
        {
            KInt2 data = new KInt2();
            data._x = (a._x * (max - percent) / max + (b._x * percent) / max);
            data._y = (a._y * (max - percent) / max + (b._y * percent) / max);

            return data;
        }

        public static float Dot(KInt2 left, KInt2 right)
        {
            return left.x * right.x + left.y * right.y;
        }

        public static float Cross(KInt2 left, KInt2 right)
        {
            return left.x * right.y - left.y * right.x;
        }

        public static int IntDot(KInt2 left, KInt2 right)
        {
            return (int)((left._x * right._x + left._y * right._y) / div2scale);
        }

        public static int IntCross(KInt2 left, KInt2 right)
        {
            return (int)((left._x * right._y - left._y * right._x) / div2scale);
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", this._x * scale, this._y * scale);
        }

        public static long PerfectlyHashThem(long a, long b)
        {
            var A = (ulong)(a >= 0 ? 2 * a : -2 * a - 1);
            var B = (ulong)(b >= 0 ? 2 * b : -2 * b - 1);
            var C = (long)((A >= B ? A * A + A + B : A + B * B) / 2);
            return a < 0 && b < 0 || a >= 0 && b >= 0 ? C : -C - 1;
        }

        public override int GetHashCode()
        {
            return (int)PerfectlyHashThem(_x, _y);
           // return base.GetHashCode();
        }

        public bool Equals(KInt2 other)
        {
            if (this._x != other._x) return false;
            if (this._y != other._y) return false;
            return true;
        }

        public override bool Equals(System.Object obj)
        {
            return base.Equals(obj);
        }

        public static implicit operator Vector2(KInt2 data)
        {
            return new Vector2(data._x * scale, data._y * scale);
        }

        public static explicit operator KInt2(Vector2 data)
        {
            return new KInt2(data);
        }

        public static KInt2 operator -(KInt2 vector)
        {
            KInt2 ki2 = new KInt2();
            ki2._x = -vector._x;
            ki2._y = -vector._y;
            return ki2;
        }

        #region add
        public static KInt2 operator +(KInt2 left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = left._x + right._x;
            value._y = left._y + right._y;
            return value;
        }

        public static KInt2 operator +(KInt2 left, Vector3 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left._x + right.x * divscale);
            value._y = (long)(left._y + right.y * divscale);
            return value;
        }

        public static KInt2 operator +(Vector2 right, KInt2 left)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left._x + right.x * divscale);
            value._y = (long)(left._y + right.y * divscale);
            return value;
        }

        public static KInt2 operator +(KInt2 left, int right)
        {
            KInt2 value = new KInt2();
            value._x = left._x + right * divscale;
            value._y = left._y + right * divscale;
            return value;
        }

        public static KInt2 operator +(KInt2 left, short right)
        {
            KInt2 value = new KInt2();
            value._x = left._x + right * divscale;
            value._y = left._y + right * divscale;
            return value;
        }

        public static KInt2 operator +(KInt2 left, float right)
        {
            int r = (int)(right * divscale);
            KInt2 value = new KInt2();
            value._x = left._x + r;
            value._y = left._y + r;
            return value;
        }

        public static KInt2 operator +(KInt2 left, KInt right)
        {
            KInt2 value = new KInt2();
            value._x = left._x + right.IntValue / KInt.divscale * divscale;
            value._y = left._y + right.IntValue / KInt.divscale * divscale;
            return value;
        }

        public static KInt2 operator +(int left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = right._x + left * divscale;
            value._y = right._y + left * divscale;
            return value;
        }

        public static KInt2 operator +(short left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = right._x + left * divscale;
            value._y = right._y + left * divscale;
            return value;
        }

        public static KInt2 operator +(float left, KInt2 right)
        {
            int r = (int)(left * divscale);
            KInt2 value = new KInt2();
            value._x = right._x + r;
            value._y = right._y + r;
            return value;
        }

        public static KInt2 operator +(KInt left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = right._x + left.IntValue /KInt.divscale * divscale;
            value._y = right._y + left.IntValue / KInt.divscale * divscale;
            return value;
        }

        #endregion
        #region reduce
        public static KInt2 operator -(KInt2 left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = left._x - right._x;
            value._y = left._y - right._y;
            return value;
        }

        public static KInt2 operator -(KInt2 left, Vector3 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left._x - right.x * divscale);
            value._y = (long)(left._y - right.y * divscale);
            return value;
        }

        public static KInt2 operator -(Vector2 left, KInt2 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left.x * divscale - right._x);
            value._y = (long)(left.y * divscale - right._y);
            return value;
        }

        public static KInt2 operator -(KInt2 left, int right)
        {
            int r =(right * divscale);
            KInt2 value = new KInt2();
            value._x = left._x - r;
            value._y = left._y - r;
            return value;
        }

        public static KInt2 operator -(KInt2 left, short right)
        {
            int r = (right * divscale);
            KInt2 value = new KInt2();
            value._x = left._x - r;
            value._y = left._y - r;
            return value;
        }

        public static KInt2 operator -(KInt2 left, float right)
        {
            int r = (int)(right * divscale);
            KInt2 value = new KInt2();
            value._x = left._x - r;
            value._y = left._y - r;
            return value;
        }

        public static KInt2 operator -(KInt2 left, KInt right)
        {
            int r = (int)(right.IntValue  * divscale /KInt.divscale);
            KInt2 value = new KInt2();
            value._x = left._x - r;
            value._y = left._y - r;
            return value;
        }

        public static KInt2 operator -(int left, KInt2 right)
        {
            int r = (left * divscale);
            KInt2 value = new KInt2();
            value._x = right._x - r;
            value._y = right._y - r;
            return value;
        }

        public static KInt2 operator -(short left, KInt2 right)
        {
            int r = (left * divscale);
            KInt2 value = new KInt2();
            value._x = right._x - r;
            value._y = right._y - r;
            return value;
        }

        public static KInt2 operator -(float left, KInt2 right)
        {
            int r = (int)(left * divscale);
            KInt2 value = new KInt2();
            value._x = right._x - r;
            value._y = right._y - r;
            return value;
        }

        public static KInt2 operator -(KInt left, KInt2 right)
        {
            int r = (int)(left.IntValue * divscale / KInt.divscale);
            KInt2 value = new KInt2();
            value._x = right._x - r;
            value._y = right._y - r;
            return value;
        }
        #endregion

        #region multi

        public static KInt2 operator *(KInt2 left, KInt2 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = left._x * right._x / divscale;
            value._y = left._y * right._y / divscale;
            return value;
        }

        public static KInt2 operator *(KInt2 left, Vector2 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (int)(left._x * right.x);
            value._y = (int)(left._y * right.y);

            return value;
        }

        public static KInt2 operator *(Vector2 left, KInt2 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left.x * right._x);
            value._y = (long)(left.y * right._y);
            return value;
        }

        public static KInt2 operator *(KInt2 left, int right)
        {
            KInt2 value = new KInt2();
            value._x = left._x * right;
            value._y = left._y * right;
            return value;
        }

        public static KInt2 operator *(KInt2 left, short right)
        {
            KInt2 value = new KInt2();
            value._x = left._x * right;
            value._y = left._y * right;
            return value;
        }

        public static KInt2 operator *(KInt2 left, float right)
        {
            KInt2 value = new KInt2();
            value._x = (long)(left._x * right);
            value._y = (long)(left._y * right);
            return value;
        }

        public static KInt2 operator *(KInt2 left, KInt right)
        {
            KInt2 value = new KInt2();
            value._x =(left._x * right.IntValue / KInt.divscale);
            value._y = (left._y * right.IntValue / KInt.divscale);
            return value;
        }

        public static KInt2 operator *(int left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = right._x * left;
            value._y = right._y * left;
            return value;
        }

        public static KInt2 operator *(short left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = right._x * left;
            value._y = right._y * left;
            return value;
        }

        public static KInt2 operator *(float left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = (long)(right._x * left);
            value._y = (long)(right._y * left);
            return value;
        }

        public static KInt2 operator *(KInt left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = (right._x * left.IntValue/ KInt.divscale);
            value._y = (right._y * left.IntValue / KInt.divscale);
            return value;
        }

        #endregion

        #region div
        private static long Div(long left, long right)
        {
            if (left == 0)
            {
                return 0;
            }
            else if (right == 0)
            {
                LogMgr.LogError("DivideByZeroException: Division by zero");
                return long.MaxValue;
            }
            return left / right;
        }

        public static KInt2 operator /(KInt2 left, KInt2 right)
        {
            KInt2 value = new KInt2();
            value._x = Div(left._x * divscale, right._x);
            value._y = Div(left._y * divscale, right._y);
            return value;
        }

        public static KInt2 operator /(KInt2 left, Vector2 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left._x / right.x);
            value._y = (long)(left._y / right.y);
            return value;
        }

        public static KInt2 operator /(Vector2 left, KInt2 right)
        {
            //keep one scale
            KInt2 value = new KInt2();
            value._x = (long)(left.x * divscale / right._x);
            value._y = (long)(left.y * divscale / right._y);
            return value;
        }

        public static KInt2 operator /(KInt2 left, int right)
        {
            KInt2 value = new KInt2();
            value._x = left._x / right;
            value._y = left._y / right;
            return value;
        }

        public static KInt2 operator /(KInt2 left, short right)
        {
            KInt2 value = new KInt2();
            value._x = left._x / right;
            value._y = left._y / right;
            return value;
        }

        public static KInt2 operator /(KInt2 left, float right)
        {
            KInt2 value = new KInt2();
            value._x = (long)(left._x / right);
            value._y = (long)(left._y / right);
            return value;
        }

        public static KInt2 operator /(KInt2 left, KInt right)
        {
            KInt2 value = new KInt2();
            value._x = (left._x * KInt.divscale / right.IntValue);
            value._y = (left._y * KInt.divscale / right.IntValue);
            return value;
        }
        #endregion
        public static bool operator ==(KInt2 left, KInt2 right)
        {
            return left._x == right._x && left._y == right._y;
        }

        public static bool operator !=(KInt2 left, KInt2 right)
        {
            return !(left._x == right._x && left._y == right._y);
        }

    }

}

