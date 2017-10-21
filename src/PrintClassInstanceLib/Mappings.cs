using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace PrintClassInstanceLib
{
    public interface IMappings
    {
        Dictionary<string, string> MapList { get; set; }
    }

    public class Mappings<T> : IMappings
    {
        public Mappings()
        {
            MapList=new Dictionary<string, string>();
        }
        public Dictionary<string, string> MapList { get; set; }
        public Mappings<T> Map(Expression<Func<T,object>> func, string newName)
        {
            if (string.IsNullOrEmpty(newName)) return this;
            switch (func.Body)
            {
                case MemberExpression mem:
                    if (GetMapping(MapList,mem.Member))
                    {
                        MapList.Add(mem.Member.Name, newName);
                    }
                    break;
                case UnaryExpression ue:
                    var operand = (MemberExpression) ue.Operand;
                    if (operand == null) break;

                    if (GetMapping(MapList, operand.Member))
                    {
                        MapList.Add(operand.Member.Name, newName);
                    }
                    break;
            }
            bool GetMapping(Dictionary<string, string> existingList, MemberInfo mInfo)
            {
                return !string.IsNullOrEmpty(mInfo.Name) && !existingList.ContainsKey(mInfo.Name);
            }
            return this;
        }
    }
}
