using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Extensions
{
    public static class MathHelper
    {
        public static T Clamp<T>(this T source, T start, T end)
            where T : IComparable
        {
            bool isReversed = start.CompareTo(end) > 0;
            T smallest = isReversed ? end : start;
            T biggest = isReversed ? start : end;

            return source.CompareTo(smallest) < 0
                ? smallest
                : source.CompareTo(biggest) > 0
                    ? biggest
                    : source;
        }
    }
}
