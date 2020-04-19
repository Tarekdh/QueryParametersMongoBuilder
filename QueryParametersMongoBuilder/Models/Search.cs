using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QueryParametersMongoBuilder.Models
{
    public class Search<T>
    {
        public string Key { get; set; }
        public string Op { get; set; }
        public dynamic Value { get; set; }
        public Type PropertyType { get; set; }

        public Search(string str) 
        {
            var arr = str.Trim().Split(" ").ToList().Where(x=>x !="").ToArray();
            if (arr.Length >= 3)
            {
                Key = arr[0].Trim();
                Op = arr[1].Trim();
                Value = BsonNull.Value;
                if (!str.ToLower().Contains("null"))
                {
                    Value = str.Replace(Key, "").Replace(Op, "").Trim();
                }
                
                KeyValueFromString();
            }
        }
        private void KeyValueFromString()
        {
            if (IdentifierManager<T>.IsIdentifier(Key))
            {
                var prop = IdentifierManager<T>.GetPropertyInfo(Key);
                Key = prop.Name;
                PropertyType = prop.PropertyType;
                
            }
            else
            {
                var newKey = "";
                var properties = Key.Split(".").ToList();
                Type currentType = typeof(T);
                PropertyInfo newProperty = null;
                foreach (var item in properties)
                {
                    newProperty = IdentifierManager<T>.GetPopertyFromType(currentType, item);
                    if (newProperty == null)
                    {
                        currentType = null;
                        newKey = Key;
                        //newKey += newKey == "" ? item : $".{ item}";
                        break;
                    }
                    newKey += newKey == "" ? newProperty.Name : $".{ newProperty.Name}";
                    currentType =newProperty.PropertyType;
                    if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        currentType = currentType.GetGenericArguments()[0];
                    }
                }
                Key = newKey;
                PropertyType = currentType;
            }
            SetValue();
        }
        private void SetValue()
        {
            if(Value == null)
            {
                Value = BsonNull.Value;
                return;
            }
            if (PropertyType == null)
            {
                return;
            }
            if (Value == "[]")
            {
                Value = new List<string> { };
                return;
            }
            
            if (PropertyType == typeof(DateTime) | PropertyType == typeof(List<DateTime>))
            {
                if (Op.Contains("in"))
                {
                List<string> strArray = StringToArray(Value);
                Value = strArray.Select(x => new BsonDateTime(DateTime.Parse(x)));
                    return;
                }
                Value = new BsonDateTime(DateTime.Parse(Value));
                return;
            }
            if (PropertyType == typeof(bool) | PropertyType == typeof(List<bool>))
            {
                if (Op.Contains("in"))
                {
                    List<string> strArray = StringToArray(Value);
                    Value = strArray.Select(x => bool.Parse(x));
                    return;
                }
                Value = bool.Parse(Value);
                return;
            }
            if (PropertyType == typeof(double) | PropertyType == typeof(List<double>))
            {
                if (Op.Contains("in"))
                {
                    List<string> strArray = StringToArray(Value);
                    Value = strArray.Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture));
                    return;
                }
                Value = double.Parse(Value, System.Globalization.CultureInfo.InvariantCulture);
                return;
            }
            if (PropertyType == typeof(ObjectId) | PropertyType == typeof(List<ObjectId>))
            {
                if (Op.Contains("in"))
                {
                    List<string> strArray = StringToArray(Value);
                    Value = strArray.Select(x => new ObjectId(x));
                    return;
                }
                Value = new ObjectId(Value);
                return;
            }
            if (PropertyType.IsEnum | PropertyType == typeof(int) | PropertyType == typeof(List<int>))
            {
                if (Op.Contains("in"))
                {
                    List<string> strArray = StringToArray(Value);
                    Value = strArray.Select(x => int.Parse(x));
                    return;
                }
                int.TryParse(Value, out int number);
                Value = number;
                return;
            }
            if (PropertyType == typeof(MongoDBRef) | PropertyType == typeof(List<MongoDBRef>))
            {
                Key = $"{Key}.$id";
                if (Op.Contains("in"))
                {
                    List<string> strArray = StringToArray(Value);
                    Value = strArray;
                    return;
                }
            }

        }

        private List<string> StringToArray(string str)
        {
            if (str != "[]")
            {
                var res = str.Replace("[", "").Replace("]","").Split(",").ToList();
                return res;
            }
            return null;
}


    }
}
