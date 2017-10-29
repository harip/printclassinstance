using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PrintClassInstanceLib.Extensions;

namespace PrintClassInstanceLib
{
    public static class InstanceUtils
    {
        public static T CreateInstance<T>(IMappings mappings = null)
        {
            var newObject = (T)Activator.CreateInstance(typeof(T));
            var setErrors = new List<string>();
            if (mappings == null || !mappings.MapList.Any()) return newObject;
            try
            {
                var memberInfos = typeof(T).GetAllMemberNames();
                mappings.MapList.ForEach(d =>
                {
                    if (!memberInfos.Contains(d.Key)) return;
                    var error=newObject.SetMemberValue(d.Key, d.Value);
                    if (!string.IsNullOrEmpty(error)) setErrors.Add(error);
                });

                if (setErrors.Any())
                {
                    //Add logger
                }
                return newObject;
            }
            catch (Exception ex)
            {
                //Need to add logger
                return newObject;
            }
        }
    }
}
