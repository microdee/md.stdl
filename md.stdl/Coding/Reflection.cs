using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Coding
{
    /// <summary>
    /// Extension methods relating to reflection and types
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Whether object is numeric or not, call for any object
        /// </summary>
        public static bool IsNumeric(this object x) { return x != null && IsNumeric(x.GetType()); }

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

        /// <summary>
        /// Assign an object to a member based on a predicate function operating on member attributes
        /// </summary>
        /// <typeparam name="TAttr">Type of attribute</typeparam>
        /// <param name="target">Object containing the target property</param>
        /// <param name="source">Desired value of the target property</param>
        /// <param name="predicate">Filter for attribute</param>
        /// <param name="inherit">Take custom attributes inheritance into account. False by default</param>
        /// <param name="enumerableFill">If target is enumerable then fill with delegate. if null enumerables are ignored</param>
        /// <returns>True if everything was fine</returns>
        public static bool AttributeConditionalFieldAssignment<TAttr>(this object target, object source,
            Func<TAttr, bool> predicate, bool inherit = false,
            Action<object, object, FieldInfo> enumerableFill = null) where TAttr : Attribute
        {
            try
            {
                var field =
                (
                    from f in target.GetType().GetFields()
                    where f.GetCustomAttributes(typeof(TAttr), inherit).Length > 0
                    let attr = (TAttr)(f.GetCustomAttributes(typeof(TAttr), inherit)[0])
                    where predicate(attr)
                    select f
                ).First();
                if (field.FieldType.IsEnumerable())
                {
                    enumerableFill?.Invoke(target, source, field);
                    return true;
                }
                else
                {
                    field.SetValue(target, source);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Assign an object to a member based on a predicate function operating on member attributes
        /// </summary>
        /// <typeparam name="TAttr">Type of attribute</typeparam>
        /// <param name="target">Object containing the target property</param>
        /// <param name="source">Desired value of the target property</param>
        /// <param name="predicate">Filter for attribute</param>
        /// <param name="inherit">Take custom attributes inheritance into account. False by default</param>
        /// <param name="enumerableFill">If target is enumerable then fill with delegate. if null enumerables are ignored</param>
        /// <returns>True if everything was fine</returns>
        public static bool AttributeConditionalPropertyAssignment<TAttr>(this object target, object source,
            Func<TAttr, bool> predicate, bool inherit = false,
            Action<object, object, PropertyInfo> enumerableFill = null) where TAttr : Attribute
        {
            try
            {
                var prop =
                (
                    from p in target.GetType().GetProperties()
                    where p.GetCustomAttributes(typeof(TAttr), inherit).Length > 0
                    let attr = (TAttr)(p.GetCustomAttributes(typeof(TAttr), inherit)[0])
                    where predicate(attr)
                    select p
                ).First();
                if (prop.PropertyType.IsEnumerable())
                {
                    enumerableFill?.Invoke(target, source, prop);
                    return true;
                }
                else
                {
                    prop.SetValue(target, source);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Shortcut to get the custom attributes of a property
        /// </summary>
        /// <typeparam name="TAttr">Type of attribute</typeparam>
        /// <param name="target">Object containing the target property</param>
        /// <param name="name">The name of the target property</param>
        /// <param name="inherit">Take custom attributes inheritance into account. False by default</param>
        /// <returns>IEnumerable of attributes</returns>
        public static IEnumerable<TAttr> GetAttributesOfProperty<TAttr>(this object target, string name, bool inherit = false) where TAttr : Attribute
        {
            return target.GetType().GetProperty(name)?.GetCustomAttributes(typeof(TAttr), inherit).Cast<TAttr>() ?? Enumerable.Empty<TAttr>();
        }

        /// <summary>
        /// Is type enumerable?
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type t)
        {
            return t.GetInterface("IEnumerable") != null && t != typeof(string);
        }
    }
}
