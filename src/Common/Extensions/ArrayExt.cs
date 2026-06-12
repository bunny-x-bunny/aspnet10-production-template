using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions {
    public static class ArrayExt {
        public static T[] Shuffle<T>(this T[] array) {
            Random random = new();
            return [.. array.OrderBy(x => random.Next())];
        }
    }
}
