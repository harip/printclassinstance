using System.Collections.Generic;

namespace PrintClassInstanceLib.Model
{
    public class ObjectCompareInfo
    {
        public List<ObjectPropertyCompareInfo> NoMatchList { get; set; } = new List<ObjectPropertyCompareInfo>();
    }
}