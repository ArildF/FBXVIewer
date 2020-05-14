using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace FBXViewer
{
    public static class Extensions
    {
        public static object? GetValueSafe(this PropertyInfo pi, object obj)
        {
            try
            {
                return pi.GetValue(obj);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        public static object? GetValueSafe(this FieldInfo fi, object obj)
        {
            try
            {
                return fi.GetValue(obj);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        public static IEnumerable<INode> PrimitiveProperties(this object obj)
        {
            var props = obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(pi => pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string) || pi.PropertyType.IsEnum)
                .OrderBy(pi => pi.Name);

            foreach (var prop in props)
            {
                yield return new PrimitivePropertyNode(prop.Name, prop.GetValueSafe(obj));
            }
            var fields = obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.FieldType.IsPrimitive || fi.FieldType == typeof(string) || fi.FieldType.IsEnum)
                .OrderBy(pi => pi.Name);
            foreach (var field in fields)
            {
                yield return new PrimitivePropertyNode(field.Name, field.GetValueSafe(obj));
            }
        } 
    }
}