using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RandomFortress
{
    public static class Converter
    {
        public static T ConvertToObject<T>(Dictionary<object, object> data) where T : new()
        {
            T obj = new T();
            Type type = typeof(T);

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldName = field.Name;

                // dictionary에서 필드 이름과 일치하는 key 찾기 (대소문자 구분 없음)
                object value = null;
                bool found = false;
                foreach (var key in data.Keys)
                {
                    if (key.ToString().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = data[key];
                        found = true;
                        break;
                    }
                }

                if (found && value != null)
                {
                    Type fieldType = field.FieldType;

                    try
                    {
                        if (fieldType.IsPrimitive || fieldType == typeof(string))
                        {
                            field.SetValue(obj, Convert.ChangeType(value, fieldType));
                        }
                        else if (fieldType.IsEnum)
                        {
                            field.SetValue(obj, Enum.Parse(fieldType, value.ToString()));
                        }
                        else if (typeof(IList).IsAssignableFrom(fieldType))
                        {
                            IList list;
                            Type elementType;

                            if (fieldType.IsArray)
                            {
                                // 배열 처리
                                elementType = fieldType.GetElementType();
                                var valueList = value as IList;

                                if (valueList != null)
                                {
                                    list = Array.CreateInstance(elementType, valueList.Count);
                                    for (int i = 0; i < valueList.Count; i++)
                                    {
                                        var item = Convert.ChangeType(valueList[i], elementType);
                                        ((Array)list).SetValue(item, i);
                                    }
                                }
                                else
                                {
                                    // 데이터가 없으면 길이 0의 배열 생성
                                    list = Array.CreateInstance(elementType, 0);
                                }
                            }
                            else
                            {
                                // List 처리
                                list = (IList)Activator.CreateInstance(fieldType);
                                elementType = fieldType.GetGenericArguments()[0];
                                var valueList = value as IList;

                                if (valueList != null)
                                {
                                    foreach (var item in valueList)
                                    {
                                        list.Add(Convert.ChangeType(item, elementType));
                                    }
                                }
                            }

                            field.SetValue(obj, list);
                        }
                        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
                        {
                            // Dictionary 처리
                            IDictionary dict = (IDictionary)Activator.CreateInstance(fieldType);
                            var valueDict = value as IDictionary;

                            if (valueDict != null)
                            {
                                Type keyType = fieldType.GetGenericArguments()[0];
                                Type valueType = fieldType.GetGenericArguments()[1];

                                foreach (DictionaryEntry kvp in valueDict)
                                {
                                    var keyObj = Convert.ChangeType(kvp.Key, keyType);
                                    var valueObj = Convert.ChangeType(kvp.Value, valueType);
                                    dict.Add(keyObj, valueObj);
                                }
                            }

                            field.SetValue(obj, dict);
                        }
                        else
                        {
                            // 중첩된 객체 처리
                            var nestedData = value as Dictionary<object, object>;
                            if (nestedData != null)
                            {
                                MethodInfo method = typeof(Converter).GetMethod("ConvertToObject")
                                    .MakeGenericMethod(fieldType);
                                object nestedObj = method.Invoke(null, new object[] { nestedData });
                                field.SetValue(obj, nestedObj);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 타입 변환 오류 처리
                        Debug.LogError($"필드 '{fieldName}' 변환 오류: {ex.Message}");
                    }
                }
                else
                {
                    // 데이터가 없을 경우, 컬렉션 초기화 또는 기본값 유지
                    if (typeof(IList).IsAssignableFrom(field.FieldType))
                    {
                        if (field.FieldType.IsArray)
                        {
                            // 배열의 경우 길이 0의 배열 생성
                            Type elementType = field.FieldType.GetElementType();
                            field.SetValue(obj, Array.CreateInstance(elementType, 0));
                        }
                        else
                        {
                            // List의 경우 빈 인스턴스 생성
                            field.SetValue(obj, Activator.CreateInstance(field.FieldType));
                        }
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(field.FieldType))
                    {
                        // Dictionary의 경우 빈 인스턴스 생성
                        field.SetValue(obj, Activator.CreateInstance(field.FieldType));
                    }
                    // 기타 타입은 기본값 유지
                }
            }

            return obj;
        }
    }
}