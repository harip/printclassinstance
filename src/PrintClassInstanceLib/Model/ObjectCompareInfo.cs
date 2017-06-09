using System.Collections.Generic;

namespace PrintClassInstanceLib.Model
{
    public class ObjectCompareInfo
    {
        public ObjectCompareInfo()
        {
            NoMatchList = new List<ObjectPropertyCompareInfo>();
        }

        public List<ObjectPropertyCompareInfo> NoMatchList { get; set; }
    }
}