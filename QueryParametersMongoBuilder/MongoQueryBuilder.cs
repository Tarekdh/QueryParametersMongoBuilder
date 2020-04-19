using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using QueryParametersMongoBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryParametersMongoBuilder
{
    public class MongoQueryBuilder<T>
    {
        public int Top { get; set; }
        public int Skip { get; set; }
        public SortDefinition<T> Sort { get; set; }
        public FilterDefinition<T> Filter { get; set; }
        private IHttpContextAccessor _httpContextAccessor;
        public MongoQueryBuilder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
        }
        public void Build()
        {
                BuildSort();
                BuildPagination();
                BuildFilters();
        }

        private void BuildSort() {
            if (_httpContextAccessor.HttpContext.Request.Query.ContainsKey("$orderBy"))
            {
                try
                {
                    var sortBuilder = Builders<T>.Sort;
                    var sortStr = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("$orderBy").ToString();
                    foreach (var s in sortStr.Split(","))
                    {
                        var items = s.Replace("desc","").Replace("asc","").Replace(" ","").Split("/");
                        var keys = new List<string> { };
                        foreach (var item in items)
                        {
                            var key = "";
                            var properties = item.Split(".").ToList();
                            Type currentType = typeof(T);
                            PropertyInfo newProperty = null;
                            foreach (var i in properties)
                            {
                                newProperty = IdentifierManager<T>.GetPopertyFromType(currentType, i);
                                if (newProperty == null)
                                {
                                    key += key == "" ? i : $".{ i}";
                                }
                                key += key == "" ? newProperty.Name : $".{ newProperty.Name}";
                                currentType = newProperty.PropertyType;
                                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    currentType = currentType.GetGenericArguments()[0];
                                }
                            }
                            keys.Add(key);
                        }

                        if (s.Contains("desc"))
                        {
                            foreach (var k in keys)
                            {
                                Sort = Sort != null ? Sort.Descending(k) : sortBuilder.Descending(k);
                            }
                        }
                        else
                        {
                            foreach (var k in keys)
                            {
                                Sort = Sort != null ? Sort.Ascending(k) : sortBuilder.Ascending(k);
                            }
                        }
                    }

                }
                catch (Exception)
                {

                }
            }
        }
        private void BuildPagination() {
            
            
            if (_httpContextAccessor.HttpContext.Request.Query.ContainsKey("$skip"))
            {
                Skip = int.Parse(_httpContextAccessor.HttpContext.Request.Query["$skip"]);
            }
            if (_httpContextAccessor.HttpContext.Request.Query.ContainsKey("$top"))
            {
                Top = int.Parse(_httpContextAccessor.HttpContext.Request.Query["$top"]);
            }
        }
        private void BuildFilters()
        {
            if (_httpContextAccessor.HttpContext.Request.Query.ContainsKey("$filter"))
            {
                var filterStr = _httpContextAccessor.HttpContext.Request.Query["$filter"].ToString();
                Filter = Join(filterStr);
            }

        }

        public FilterDefinition<T> Join(string str)
        {
            FilterDefinition<T> filter = null;
            if (str.Contains("$or"))
            {
                foreach (var item in str.Split("$or"))
                {
                    if (filter == null)
                    {
                        filter = Join(item);
                        continue;
                    }
                    var newFiler = Join(item);
                    filter = newFiler != null ? filter | newFiler : filter;
                }
            }
            else if (str.Contains("$and"))
            {
                foreach (var item in str.Split("$and"))
                {
                    if (filter == null)
                    {
                        filter = Join(item);
                        continue;
                    }
                    var newFiler = Join(item);
                    filter = newFiler !=null ? filter & newFiler : filter;
                }
            }
            else
            {
                filter = BuildOne(str);
            }

            return filter;
            
        }

        private FilterDefinition<T> BuildOne(string str)
        {
            var builder = Builders<T>.Filter;
            FilterDefinition<T> filter = null;
            var search = new Search<T>(str);
            if (search.Key !=  null)
            {
                switch (search.Op)
                {
                    case "eq":
                        if (search.PropertyType == typeof(string))
                        {
                            filter = builder.Regex(search.Key, new BsonRegularExpression(new Regex(search.Value, RegexOptions.IgnoreCase)));
                            break;
                        }
                        filter = builder.Eq(search.Key, search.Value);
                        break;
                    case "ne":
                        if (search.PropertyType == typeof(string))
                        {
                            filter = builder.Regex(search.Key, new BsonRegularExpression(new Regex(search.Value, RegexOptions.IgnoreCase)));
                            filter = builder.Not(filter);
                            break;
                        }
                        filter = builder.Ne(search.Key, search.Value);
                        break;
                    case "gt":
                        filter = builder.Gt(search.Key, search.Value);
                        break;
                    case "gte":
                        filter = builder.Gte(search.Key, search.Value);
                        break;
                    case "lt":
                        filter = builder.Lt(search.Key, search.Value);
                        break;
                    case "lte":
                        filter = builder.Lte(search.Key, search.Value);
                        break;
                    case "in":
                        filter = builder.In(search.Key, search.Value);
                        break;
                    case "nin":
                        filter = builder.Nin(search.Key, search.Value);
                        break;
                }
            }
            return filter;

        }

        
    }
}
