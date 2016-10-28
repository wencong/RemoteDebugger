﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RemoteDebugger {
    public static class ComponentExtension {
        public static bool ContainProperty(this Component component, string propertyName) {
            if (component != null && !string.IsNullOrEmpty(propertyName)) {
                PropertyInfo _findedPropertyInfo = component.GetType().GetProperty(propertyName);
                return (_findedPropertyInfo != null);
            }
            return false;
        }

        public static T GetValue<T>(this Component component, string propertyName) {
            if (component != null && !string.IsNullOrEmpty(propertyName)) {
                PropertyInfo propertyInfo = component.GetType().GetProperty(propertyName);
                if (propertyInfo != null) {
                    return (T)propertyInfo.GetValue(component, null);
                }
            }
            return default(T);
        }

        public static void SetValue<T>(Component component, string propertyName, T value) {
            if (component != null && !string.IsNullOrEmpty(propertyName)) {
                PropertyInfo propertyInfo = component.GetType().GetProperty(propertyName);
                FieldInfo fieldInfo = component.GetType().GetField(propertyName);

                if (propertyInfo != null) {
                    propertyInfo.SetValue(component, value, null);
                }
                else if (fieldInfo != null) {
                    fieldInfo.SetValue(component, value);
                }
            }
        }

        public static void SetPropertys(this Component component, PropertyObj[] propertys) {
            if (component != null && propertys.Length != 0) {
                for (int i = 0; i < propertys.Length; ++i) {
                    PropertyObj property = propertys[i];

                    // do not modify compnent's name
                    if (property.m_szValueName == "name") {
                        continue;
                    }

                    Type t = Util.GetTypeByName(property.m_szTypeName);
                    if (t == null) {
                        Debug.LogWarningFormat("GetTypeByName({0}) is null", property.m_szTypeName);
                        continue;
                    }

                    try {
                        MethodInfo mi = typeof(ComponentExtension).GetMethod("SetValue").MakeGenericMethod(t);
                        System.Object[] parmas = null;

                        if (t.Equals(typeof(System.Single))) {
                            parmas = new System.Object[] { component, property.m_szValueName, Single.Parse(property.m_value.ToString()) };
                        }
                        else if (t.Equals(typeof(System.UInt32))) {
                            parmas = new System.Object[] { component, property.m_szValueName, UInt32.Parse(property.m_value.ToString()) };
                        }
                        else if (t.Equals(typeof(System.UInt64))) {
                            parmas = new System.Object[] { component, property.m_szValueName, UInt64.Parse(property.m_value.ToString()) };
                        }
                        else if (t.Equals(typeof(System.UInt16))) {
                            parmas = new System.Object[] { component, property.m_szValueName, UInt16.Parse(property.m_value.ToString()) };
                        }
                        else {
                            parmas = new System.Object[] { component, property.m_szValueName, property.m_value };
                        }
                        mi.Invoke(null, parmas);
                    }

                    catch (Exception ex) {
                        Debug.LogException(ex);
                        Debug.LogErrorFormat("Set Component:{0} property:{1} Failed, Type:{2}", component.name, property.m_szValueName, t.ToString());
                        //throw (new Exception(string.Format("Set Component:{0} property:{1} Failed, Type:{2}", component.name, property.szName, t.ToString())));
                    }
                }
            }
        }


        public static PropertyObj[] GetPropertys(this Component component) {
            List<PropertyObj> lstPropertys = new List<PropertyObj>();

            try {
                PropertyInfo[] propertyInfos = component.GetType().GetProperties(BindingFlags.Public |
                                                                                 BindingFlags.Instance |
                                                                                 BindingFlags.SetProperty |
                                                                                 BindingFlags.GetProperty);

                FieldInfo[] fieldInfos = component.GetType().GetFields(BindingFlags.Public |
                                                                       BindingFlags.Instance |
                                                                       BindingFlags.SetField |
                                                                       BindingFlags.GetField);

                for (int i = 0; i < propertyInfos.Length; ++i) {
                    PropertyInfo pi = propertyInfos[i];

                    //Debug.LogError("Property:" + pi.Name);

                    if (pi.CanWrite && pi.CanRead) {
                        // call getter with these Property name will create new object;
                        if (pi.Name == "mesh" || pi.Name == "material" || pi.Name == "materials") {
                            continue;
                        }

                        System.Object obj = pi.GetValue(component, null);
                        if (obj is System.Collections.ICollection) {
                            continue;
                        }

                        lstPropertys.Add(new PropertyObj(component, pi));
                    }
                    else {

                    }
                }

                for (int i = 0; i < fieldInfos.Length; ++i) {
                    FieldInfo fi = fieldInfos[i];
                    //Debug.LogError("Field:" + fi.Name);
                    lstPropertys.Add(new PropertyObj(component, fi));
                }
            }

            catch (Exception ex) {
                Debug.Log(ex);
            }

            return lstPropertys.ToArray();
        }
    }
}