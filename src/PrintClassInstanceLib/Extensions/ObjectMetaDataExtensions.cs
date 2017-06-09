using System.Collections.Generic;
using System.Linq;

namespace PrintClassInstanceLib.Extensions
{
    public static class ObjectMetaDataExtensions
    {
        public static string GetNamespace(this object classInstance)
        {
            return classInstance.GetType().GetNamespace();
        }

        public static List<string> GetBaseClassesNames(this object classInstance)
        {
            return  classInstance.GetType().GetBaseClassesNames();
        }

        public static IEnumerable<string> MemberNames(this object classInstance)
        {
            return classInstance.GetType().GetAllMemberNames();
        }

          
    }
}
