using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace PrintClassInstanceLib
{
    public interface IMappings
    {
        Dictionary<string, object> MapList { get; set; }
    }

    public class Mappings<T> : IMappings
    {
        public Mappings()
        {
            MapList=new Dictionary<string, object>();
        }
        public Dictionary<string, object> MapList { get; set; }
        public Mappings<T> Map(Expression<Func<T,object>> func, object toMapvalue)
        {
            if (toMapvalue==null) return this;
            switch (func.Body)
            {
                case MemberExpression mem:
                    if (GetMapping(MapList,mem.Member))
                    {
                        MapList.Add(mem.Member.Name, toMapvalue);
                    }
                    break;
                case UnaryExpression ue:
                    var operand = (MemberExpression) ue.Operand;
                    if (operand == null) break;

                    if (GetMapping(MapList, operand.Member))
                    {
                        MapList.Add(operand.Member.Name, toMapvalue);
                    }
                    break;
            }
            bool GetMapping(Dictionary<string, object> existingList, MemberInfo mInfo)
            {
                return !string.IsNullOrEmpty(mInfo.Name) && !existingList.ContainsKey(mInfo.Name);
            }
            return this;
        }
    }
}
