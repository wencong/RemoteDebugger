using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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

    public static void SetPropertys(this Component component, RDProperty[] propertys) {
        if (component != null && propertys.Length != 0) {
            for (int i = 0; i < propertys.Length;++i) {
                RDProperty property = propertys[i];

                Type t = Util.GetTypeByName(property.szValueTypeName);

                try {
                    if (property.IsEnum()) {
                        Enum eValue = (Enum)Enum.Parse(t, property.value.ToString());
                        SetValue(component, property.szName, eValue);
                    }

                    else if (property.IsAsset()) {
                        if (t.IsArray) {
                            Debug.LogFormat("Todo: Set Asset value: {0} {1}", property.szValueTypeName, property.value.ToString());
                        }
                        else {
                            Debug.LogFormat("Todo: Set Asset value: {0} {1}", property.szValueTypeName, (string)property.value);
                        }
                    }

                    else {
                        MethodInfo mi = typeof(ComponentExtension).GetMethod("SetValue").MakeGenericMethod(t);

                        if (t.Equals(typeof(System.Single))) {
                            mi.Invoke(null, new System.Object[] { component, property.szName, Single.Parse(property.value.ToString()) });
                        }
                        else {
                            mi.Invoke(null, new System.Object[] { component, property.szName, property.value });
                        }
                    }
                    
                }
                catch (Exception ex) {
                    throw (ex);
                }
            }
        }
    }


    public static RDProperty[] GetPropertys(this Component component) {
        List<RDProperty> lstPropertys = new List<RDProperty>();

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
                if (pi.CanWrite && pi.CanRead) {
                    bool bRet = pi.PropertyType.IsSubclassOf(typeof(UnityEngine.Component));
                    if (bRet) {
                        continue;
                    }
                    
                    lstPropertys.Add(new RDProperty(component, pi));
                }
            }

            for (int i = 0; i < fieldInfos.Length; ++i) {
                FieldInfo fi = fieldInfos[i];

                if (fi.IsPublic && !fi.IsLiteral) {
                    bool bRet = fi.FieldType.IsSubclassOf(typeof(UnityEngine.Component));
                    if (bRet) {
                        continue;
                    }

                    lstPropertys.Add(new RDProperty(component, fi));
                }
            }

        }
        catch (Exception ex) {
            Debug.Log(ex);
        }

        return lstPropertys.ToArray();
    }
}
