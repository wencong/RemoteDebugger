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
                
                // do not modify compnent's name
                if (property.szName == "name") {
                    continue;
                }

                Type t = Util.GetTypeByName(property.szValueTypeName);
                if (t == null) {
                    Debug.LogWarningFormat("GetTypeByName({0}) is null", property.szValueTypeName);
                    continue;
                }

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
                            //Debug.LogFormat("Todo: Set Asset value: {0} {1}", property.szValueTypeName, (string)property.value);
                            //MethodInfo mi = typeof(RD).GetMethod("Load").MakeGenericMethod(t);

                            //System.Object o = mi.Invoke(RD.Instance, new System.Object[] { property.value });

                            if (t == typeof(Material)) {
                                Material mat = Resources.Load<Material>((string)property.value);
                                if (mat != null) {
                                    SetValue<Material>(component, property.szName, mat);
                                }
                                else {
                                    Debug.LogErrorFormat("Load {0} Failed! Start search Resources", property.value);

                                    Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
                                    for (int j = 0; j < mats.Length; ++j) {
                                        if (mats[j].name == (string)property.value) {
                                            SetValue<Material>(component, property.szName, mats[j]);
                                            break;
                                        }
                                    }
                                }
                            }

                            else if (t == typeof(Mesh)) {
                                // todo
                            }
                        }
                    }

                    else {
                        MethodInfo mi = typeof(ComponentExtension).GetMethod("SetValue").MakeGenericMethod(t);
                        System.Object[] parmas = null;

                        /*
                        if (t.Equals(typeof(string))) {
                            parmas = new System.Object[] { component, property.szName, property.value };
                        }
                        else {
                            MethodInfo par = t.GetMethod("Parse");
                            System.Object o = par.Invoke(null, new System.Object[] { property.value.ToString() });
                            parmas = new System.Object[] { component, property.szName, o };
                        }
                        */
                        
                        if (t.Equals(typeof(System.Single))) {
                            parmas = new System.Object[] { component, property.szName, Single.Parse(property.value.ToString()) };
                        }
                        else if (t.Equals(typeof(System.UInt32))) {
                            parmas = new System.Object[] { component, property.szName, UInt32.Parse(property.value.ToString()) };
                        }
                        else if (t.Equals(typeof(System.UInt64))) {
                            parmas = new System.Object[] { component, property.szName, UInt64.Parse(property.value.ToString()) };
                        }
                        else if (t.Equals(typeof(System.UInt16))) {
                            parmas = new System.Object[] { component, property.szName, UInt16.Parse(property.value.ToString()) };
                        }
                        else {
                            parmas = new System.Object[] { component, property.szName, property.value };
                        }
                        mi.Invoke(null, parmas);
                    }
                }
                catch {
                    Debug.LogErrorFormat("Set Component:{0} property:{1} Failed, Type:{2}", component.name, property.szName, t.ToString());
                    //throw (new Exception(string.Format("Set Component:{0} property:{1} Failed, Type:{2}", component.name, property.szName, t.ToString())));
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

                    // call getter with these names will instantiate new object;
                    if (Util.IsAsset(pi.PropertyType) && ( pi.Name == "mesh" || pi.Name == "material" || pi.Name == "materials")) {
                        continue;
                    }

                    System.Object obj = pi.GetValue(component, null);
                    if (obj is System.Collections.ICollection) {
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

                    System.Object obj = fi.GetValue(component);
                    if (obj is System.Collections.ICollection) {
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
