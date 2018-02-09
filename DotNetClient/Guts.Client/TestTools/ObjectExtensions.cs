﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Guts.Client.TestTools
{
    public static class ObjectExtensions
    {
        public static bool HasPrivateField<T>(this Object obj)
        {
            return HasPrivateField<T>(obj, field => true);
        }

        public static bool HasPrivateField<T>(this Object obj, Func<FieldInfo, bool> filterFunc)
        {
            var objectType = obj.GetType();
            var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.FieldType == typeof(T));

            return fields.Any(filterFunc);
        }

        public static bool HasPrivateFieldValue<T>(this Object obj, Func<T, bool> filterFunc)
        {
            var objectType = obj.GetType();
            var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.FieldType == typeof(T));

            try
            {
                var values = fields.Select(field => (T)field.GetValue(obj));
                return values.Any(filterFunc);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T GetPrivateFieldValue<T>(this Object oject)
        {
            return GetPrivateFieldValue<T>(oject, (FieldInfo field) => true);
        }

        public static T GetPrivateFieldValueByName<T>(this Object oject, string fieldName)
        {
            return GetPrivateFieldValue<T>(oject, field => field.Name.ToLower() == fieldName.ToLower());
        }

        public static T GetPrivateFieldValue<T>(this Object oject, Func<FieldInfo, bool> filterFunc)
        {
            var objectType = oject.GetType();
            var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.FieldType == typeof(T));

            var theField = fields.FirstOrDefault(filterFunc);

            if (theField == null) throw new FieldAccessException("Could not find a matching field");

            return (T)theField.GetValue(oject);
        }

        public static IEnumerable<T> GetAllPrivateFieldValues<T>(this Object obj)
        {
            var objectType = obj.GetType();
            var fields = objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.FieldType == typeof(T));

            return fields.Select(field => (T)field.GetValue(obj));
        }
    }
}
