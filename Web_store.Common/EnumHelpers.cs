using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Web_store.Common
{
    public static class EnumHelpers<T>
    {
        public static IList<T> GetValues(Enum value)
        {
            var enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static IList<string> GetDisplayValues(Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        private static string lookupResource(Type resourceManagerProvider, string resourceKey)
        {
            foreach (PropertyInfo staticProperty in resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (staticProperty.PropertyType == typeof(System.Resources.ResourceManager))
                {
                    var resourceManagerValue = staticProperty.GetValue(null, null);
                    if (resourceManagerValue is System.Resources.ResourceManager resourceManager)
                    {
                        return resourceManager.GetString(resourceKey) ?? resourceKey;
                    }
                }
            }

            return resourceKey; // Fallback with the key name
        }

        public static string GetDisplayValue(T value)
        {
            try
            {
                if (value == null) return "نا مشخص";

                string? valueString = value.ToString();
                if (string.IsNullOrEmpty(valueString)) return "نا مشخص";

                var fieldInfo = value.GetType().GetField(valueString);
                if (fieldInfo == null) return valueString;

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                if (descriptionAttributes == null || descriptionAttributes.Length == 0)
                    return valueString;

                var displayAttribute = descriptionAttributes[0];

                if (displayAttribute?.ResourceType != null && !string.IsNullOrEmpty(displayAttribute.Name))
                    return lookupResource(displayAttribute.ResourceType, displayAttribute.Name);

                return displayAttribute?.Name ?? valueString;
            }
            catch (Exception)
            {
                return "نا مشخص";
            }
        }
    }
}
