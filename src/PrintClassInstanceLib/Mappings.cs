using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        public Mappings<T> Map(Expression<Func<T, object>> func, string newName)
        {
            if (!(func.Body is MemberExpression member) || string.IsNullOrEmpty(newName)) return this;
            if (!string.IsNullOrEmpty(member.Member.Name) && !MapList.ContainsKey(member.Member.Name))
            {
                MapList.Add(member.Member.Name, newName);
            }
            return this;
        }
    }
}
