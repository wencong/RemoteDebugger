using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using LitJsonEx;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RemoteDebugger {
    public class RDDataBase {
        /*
        public static string Serializer<T>(T obj) where T : IMetaObj {
            try {
                Type valueType = obj.GetValueType();

                if (valueType != null && !valueType.IsPrimitive && !valueType.Equals(typeof(string))) {
                    try {
                        if (obj.IsEnum()) {
                            obj.value = obj.value.ToString();
                        }
                        else if (obj.IsAsset()) {
                            if (obj.value != null) {
                            
                                if (valueType.IsArray) {
                                    int nLength = (obj.value as Array).Length;
                                    string[] arrayAsset = new string[nLength];
                                    for (int i = 0; i < nLength; i++) {
                                        UnityEngine.Object asset = (obj.value as Array).GetValue(i) as UnityEngine.Object;
                                        //arrayAsset[i] = asset.name;
                                        arrayAsset[i] = GetAssetPathOrName(asset);
                                    }
                                    obj.value = JsonMapper.ToJson(arrayAsset);
                                }
                                else {
                                    //obj.value = ((UnityEngine.Object)obj.value).name;
                                    obj.value = GetAssetPathOrName((UnityEngine.Object)obj.value);
                                }
                            }
                        }

                        else {
                            obj.value = JsonMapper.ToJson(obj.value);
                        }
                        obj.bIsSerialized = true;
                    }
                    catch {
                        //Debug.LogException(ex);
                        Debug.LogFormat("Serializer {0} failed...", obj.szValueTypeName);
                        obj.bIsSerialized = false;
                    }

                }
                return JsonMapper.ToJson(obj);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public static T Deserializer<T>(string data) where T : IMetaObj {
            try {
                IMetaObj ret = JsonMapper.ToObject<T>(data);
                Type valueType = ret.GetValueType();

                if (valueType != null && !valueType.IsPrimitive && !valueType.Equals(typeof(string))) {
                    if (ret.bIsSerialized) {
                        if (ret.IsEnum()) {
                            ret.value = (string)ret.value;
                        }
                        else if (ret.IsAsset()) {
                            if (ret.value != null) {
                                if (valueType.IsArray) {
                                    ret.value = JsonMapper.ToObject<string[]>((string)ret.value);
                                }
                                else {
                                    ret.value = (string)ret.value;
                                }
                            }
                        }
                        else {
                            ParameterInfo[] pinfos = null;
                            MethodInfo mi = typeof(JsonMapper).GetMethods().First(
                                    m => m.Name.Equals("ToObject") && m.IsGenericMethod
                                    && (pinfos = m.GetParameters()).Length == 1
                                    && pinfos[0].ParameterType.Equals(typeof(string))
                                    ).MakeGenericMethod(valueType);
                            ret.value = mi.Invoke(null, new System.Object[] { ret.value });
                        }
                    }
                }

                return (T)ret;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public static string SerializerArray<T>(T[] objs) where T : IMetaObj {
            try {
                string[] arraySer = new string[objs.Length];

                for (int i = 0; i < objs.Length; ++i) {
                    Type selfType = objs[i].GetSelfType();

                    MethodInfo mi = typeof(RDDataBase).GetMethod("Serializer").MakeGenericMethod(selfType);

                    arraySer[i] = mi.Invoke(null, new System.Object[] { objs[i] }) as string;
                }

                return JsonMapper.ToJson(arraySer);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public static T[] DeserializerArray<T>(string json) where T : IMetaObj {
            try {
                string[] arraySer = JsonMapper.ToObject<string[]>(json);

                T[] ret = new T[arraySer.Length];

                for (int i = 0; i < arraySer.Length; ++i) {
                    ret[i] = Deserializer<T>(arraySer[i]);
                }

                return ret;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }
        */
        public static string GetAssetPathOrName(UnityEngine.Object asset) {
            string szRet = string.Empty;

#if UNITY_EDITOR
            string szPath = AssetDatabase.GetAssetPath(asset);

            if (!string.IsNullOrEmpty(szPath)) {
                int nPos = szPath.IndexOf("Resources");
                szRet = szPath.Substring(nPos + "Resources".Length + 1);
                szRet = szRet.Substring(0, szRet.LastIndexOf("."));
            }
#else 
        szRet = asset.name;
#endif

            return szRet;
        }
    }
}