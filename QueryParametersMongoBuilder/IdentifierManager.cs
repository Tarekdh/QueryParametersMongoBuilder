using System;
using System.Linq;
using System.Reflection;

namespace QueryParametersMongoBuilder.Models
{
    public static class IdentifierManager<T>
    {
        public static string GetIdentifier(string id)
        {
            return typeof(T)
                    .GetProperties()
                    .FirstOrDefault(x => x.Name.ToLower() == id.ToLower()).Name;
            
        }
        public static PropertyInfo GetPropertyInfo(string id)
        {
            return typeof(T)
                    .GetProperties()
                    .FirstOrDefault(x => x.Name.ToLower() == id.ToLower());

        }
        public static bool IsIdentifier(string fieldName)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var isIdentifier = properties.FirstOrDefault(x => x.Name.ToLower() == fieldName.ToLower()) != null;
            return isIdentifier;
        }
        public static PropertyInfo GetPopertyFromType(Type type,string fieldName)
        {
            var properties = type.GetProperties();
            var propertyInfo = properties.FirstOrDefault(x => x.Name.ToLower() == fieldName.ToLower());
            return propertyInfo;
        }
    }

}
