using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PrintClassInstanceLib.Extensions
{
    public static class ObjectTrackingExtensions
    {
        public static void Snapshot<T>(this object classInstance, ISnapshotContainer<T> dictionaryContainer)
        {
            switch (dictionaryContainer)
            {
                case DictionaryContainer dc:
                    dc.AddSnapshot(classInstance.Flatten().Result);
                    break;
                case JsonContainer jc:
                    jc.AddSnapshot(classInstance.FlattenedJson().Result);
                    break;
            }
        }
    }
    
    public interface ISnapshotContainer<T>
    {
        void AddSnapshot(T value);
        List<T> GetSnapshots();
    }

    public class DictionaryContainer : ISnapshotContainer<Dictionary<string,object>>
    {
        private readonly ConcurrentBag<Dictionary<string, object>> _snapshots;
        public DictionaryContainer()
        {
            _snapshots=new ConcurrentBag<Dictionary<string, object>>();
        }
        public void AddSnapshot(Dictionary<string, object> value)
        {
            _snapshots.Add(value);
        }
        public List<Dictionary<string, object>> GetSnapshots()
        {
            return _snapshots.ToList();
        }
    }
    
    public class JsonContainer : ISnapshotContainer<string>
    {
        private readonly ConcurrentBag<string> _snapshots;
        public JsonContainer()
        {
            _snapshots = new ConcurrentBag<string>();
        }
        public List<string> GetSnapshots()
        {
            return _snapshots.ToList();
        }
        public void AddSnapshot(string flattened)
        {
            _snapshots.Add(flattened);
        }
    }
}
