using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Coding
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Whether object is numeric or not, call for any object
        /// </summary>
        public static bool IsNumeric(this object x) { return x == null ? false : IsNumeric(x.GetType()); }

        /// <summary>
        /// Whether type is numeric or not
        /// </summary>
        public static bool IsNumeric(Type type) { return IsNumeric(type, Type.GetTypeCode(type)); }

        /// <summary>
        /// Simple extension method imitating the "is" operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Is(this Type a, Type b)
        {
            return a.IsAssignableFrom(b) || a.IsSubclassOf(b);
        }

        /// <summary>
        /// Whether the Type and TypeCode pair is numeric or not
        /// </summary>
        public static bool IsNumeric(Type type, TypeCode typeCode)
        {
            return (typeCode == TypeCode.Decimal ||
                   (type.IsPrimitive && typeCode !=
                       TypeCode.Object && typeCode !=
                       TypeCode.Boolean && typeCode !=
                       TypeCode.Char
                   ));
        }

        /// <summary>
        /// Get all types this object inherits
        /// </summary>
        public static IEnumerable<Type> GetTypes(this Type type)
        {
            // is there any base type?
            if (type == null) yield break;
            yield return type;
            if (type.BaseType == null) yield break;
            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }

        /// <summary>
        /// Get name of the type
        /// </summary>
        public static string GetName(this Type T, bool full)
        {
            if (full) return T.FullName;
            else return T.Name;
        }
    }
}
