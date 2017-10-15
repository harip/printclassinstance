using System.Collections.Concurrent;
namespace PrintClassInstanceLib.Model
{
    public class ObjectCompareInfo
    {
        public ConcurrentBag<ObjectPropertyCompareInfo> NoMatchList { get; set; } = new ConcurrentBag<ObjectPropertyCompareInfo>();
    }
}