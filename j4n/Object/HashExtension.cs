using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Object
{
    public static class HashExtension
    {
        public static int GetHash(this string str)
        {
            var hash = 0;
            var chars = str.ToCharArray();
            var pow = chars.Any() ? chars.Count() - 1 : 0;
            foreach (var c in chars)
            {
                hash += (int)IntPower(c * 31, (short)pow--);
            }
            return hash;
        }

        public static long IntPower(int x, short power)
        {
            if (power == 0) return 1;
            if (power == 1) return x;
            // ----------------------
            int n = 15;
            while ((power <<= 1) >= 0) n--;

            long tmp = x;
            while (--n > 0)
                tmp = tmp * tmp *
                     (((power <<= 1) < 0) ? x : 1);
            return tmp;
        }

        public static int hashCode(this string str)
        {
            int h = 0, off = 0;
            var chars = str.ToCharArray();
            var len = chars.Count();

            for (var i = 0; i < len; i++)
            {
                h = 31 * h + chars[off++];
            }
            return h;
        }
    }
}

