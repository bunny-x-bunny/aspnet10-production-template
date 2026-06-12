using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions {
    public static class QueryParamsExt {
        public static Task<string> ToUrlEncodedString(this Dictionary<string, string> self)
            => new FormUrlEncodedContent(self).ReadAsStringAsync();
    }
}
