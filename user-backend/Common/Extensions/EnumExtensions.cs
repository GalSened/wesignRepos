using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Common.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string ToStringEnum(this Enum value)
        {
            // Get the enum type and value
            FieldInfo field = value.GetType().GetField(value.ToString());

            // Check if the EnumMember attribute is present
            EnumMemberAttribute attribute = (EnumMemberAttribute)Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute));

            return attribute != null ? attribute.Value : value.ToString();
        }

        public static T? EnumFromString<T>(string value) where T : struct, Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                // Check if the value matches the EnumMember value
                var attribute = (EnumMemberAttribute)Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute));
                if (attribute != null && attribute.Value == value)
                {
                    return (T)Enum.Parse(typeof(T), field.Name);
                }
            }
            return null;
        }
    }
}
