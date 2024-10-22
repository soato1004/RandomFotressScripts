using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomFortress
{
    public static class JsonParser
    {
        public static T ParseJson<T>(string jsonString) where T : new()
        {
            try
            {
                Debug.Log($"Starting to parse JSON for type {typeof(T).Name}");
                Debug.Log($"JSON content: {jsonString}");  // 전체 JSON 내용 로깅

                var result = new T();
                var jObject = JObject.Parse(jsonString);

                foreach (var property in typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    try
                    {
                        if (!jObject.ContainsKey(property.Name))
                        {
                            Debug.LogWarning($"Property '{property.Name}' not found in JSON");
                            continue;
                        }

                        var value = jObject[property.Name];
                        Debug.Log($"Parsing property: {property.Name}, Type: {property.FieldType.Name}, JSON Value: {value}");
                        var convertedValue = ConvertJTokenToType(value, property.FieldType, property.Name);
                        property.SetValue(result, convertedValue);
                        Debug.Log($"Successfully set value for property: {property.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing property '{property.Name}': {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ParseJson<{typeof(T).Name}>: {ex.Message}");
                throw;
            }
        }

        private static object ConvertJTokenToType(JToken token, Type targetType, string propertyName)
        {
            try
            {
                // Debug.Log($"Converting JToken to {targetType.Name} for property {propertyName}. Token: {token}");

                if (token.Type == JTokenType.Null)
                {
                    Debug.Log($"Null value encountered for property {propertyName}");
                    return null;
                }
                
                // GameRank enum을 사용하는 특별한 처리
                if (targetType == typeof(int) && (propertyName == "soloRank" || propertyName == "pvpRank"))
                {
                    return ConvertToGameRankValue(token);
                }


                if (targetType == typeof(string))
                    return token.ToString();

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, token.ToString());

                if (targetType.IsValueType)
                    return Convert.ChangeType(token, targetType);

                if (targetType == typeof(List<AdDebuff>))
                    return ParseAdDebuffList(token);

                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                    return ParseGenericList(token, targetType);

                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return ParseGenericDictionary(token, targetType);

                if (targetType.IsClass)
                    return JsonConvert.DeserializeObject(token.ToString(), targetType);

                throw new ArgumentException($"Unsupported type: {targetType.Name} for property {propertyName}");
            }
            catch (JsonReaderException jex)
            {
                Debug.LogError($"JSON parsing error for property {propertyName}: {jex.Message}. Path: {jex.Path}, LineNumber: {jex.LineNumber}, LinePosition: {jex.LinePosition}.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error converting JToken to {targetType.Name} for property {propertyName}: {ex.Message}");
                return null;
            }
        }

        private static int ConvertToGameRankValue(JToken token)
        {
            if (token.Type == JTokenType.Integer)
            {
                int rankValue = token.Value<int>();
                if (Enum.IsDefined(typeof(GameRank), rankValue))
                {
                    return rankValue;
                }
            }
            else if (token.Type == JTokenType.String)
            {
                string rankString = token.Value<string>();
                if (Enum.TryParse(rankString, true, out GameRank rank))
                {
                    return (int)rank;
                }
            }

            Debug.LogWarning($"Invalid GameRank value: {token}. Using default value Beginner (0).");
            return 0; // Default to Beginner
        }
        
        private static List<AdDebuff> ParseAdDebuffList(JToken token)
        {
            // Debug.Log($"Parsing AdDebuff list. Token: {token}");
            var list = new List<AdDebuff>();
            var jObject = token as JObject;
            if (jObject != null)
            {
                foreach (var property in jObject.Properties())
                {
                    try
                    {
                        var adDebuff = new AdDebuff
                        {
                            type = (AdRewardType)Enum.Parse(typeof(AdRewardType), property.Name),
                            endTime = property.Value.ToString()
                        };
                        list.Add(adDebuff);
                        // Debug.Log($"Added AdDebuff: Type = {adDebuff.type}, EndTime = {adDebuff.endTime}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing AdDebuff property {property.Name}: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Expected JObject for AdDebuff list, but got: {token.Type}");
            }
            return list;
        }

        private static object ParseGenericList(JToken token, Type targetType)
        {
            // Debug.Log($"Parsing generic list of type {targetType.GetGenericArguments()[0].Name}. Token: {token}");
            var listType = typeof(List<>).MakeGenericType(targetType.GetGenericArguments()[0]);
            var list = (System.Collections.IList)Activator.CreateInstance(listType);
            var elementType = targetType.GetGenericArguments()[0];

            // 객체인 경우 단일 항목으로 처리
            if (token.Type == JTokenType.Object)
            {
                try
                {
                    var convertedItem = ConvertJTokenToType(token, elementType, "SingleItem");
                    list.Add(convertedItem);
                    // Debug.Log($"Added single object as list item: {convertedItem}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing single object as list item: {ex.Message}. Item: {token}");
                }
                return list;
            }

            // 배열이 아닌 경우 빈 리스트 반환
            if (token.Type != JTokenType.Array)
            {
                Debug.LogWarning($"Expected array for list, but got: {token.Type}. Returning empty list.");
                return list;
            }

            foreach (var item in token)
            {
                try
                {
                    var convertedItem = ConvertJTokenToType(item, elementType, "ListItem");
                    list.Add(convertedItem);
                    // Debug.Log($"Added item to list: {convertedItem}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing list item: {ex.Message}. Item: {item}");
                }
            }

            return list;
        }

        private static object ParseGenericDictionary(JToken token, Type targetType)
        {
            // Debug.Log($"Parsing generic dictionary of type {targetType.GetGenericArguments()[0].Name}, {targetType.GetGenericArguments()[1].Name}. Token: {token}");
            var dictType = typeof(Dictionary<,>).MakeGenericType(targetType.GetGenericArguments());
            var dict = (System.Collections.IDictionary)Activator.CreateInstance(dictType);
            var keyType = targetType.GetGenericArguments()[0];
            var valueType = targetType.GetGenericArguments()[1];

            if (token.Type != JTokenType.Object)
            {
                Debug.LogError($"Expected object for dictionary, but got: {token.Type}");
                return dict;
            }

            foreach (var item in (JObject)token)
            {
                try
                {
                    var key = Convert.ChangeType(item.Key, keyType);
                    var value = ConvertJTokenToType(item.Value, valueType, item.Key);
                    dict.Add(key, value);
                    // Debug.Log($"Added dictionary entry: Key = {key}, Value = {value}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing dictionary entry for key {item.Key}: {ex.Message}. Value: {item.Value}");
                }
            }

            return dict;
        }
    }
}