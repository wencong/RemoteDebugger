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

	public static void SetValue<T>(this Component component, string propertyName, T value) {
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
        string value = string.Empty;

        if (component != null && propertys.Length != 0) {
            for (int i = 0; i < propertys.Length;++i) {
                RDProperty property = propertys[i];

                #region SetValue
                switch (property.szTypeName) {
                    case "System.Int32": {
                        if (!component.GetValue<int>(property.szName).Equals(property.value)) {
                            component.SetValue(property.szName, (int)property.value);
                        }
                        break;
                    }
                    case "System.Single": {
                        if (!component.GetValue<Single>(property.szName).Equals(property.value)) {
                            component.SetValue(property.szName, Single.Parse(property.value.ToString()));
                        }
                        break;
                    }
                    case "System.String": {
                        if (!property.szName.Equals("name")) {
                            value = component.GetValue<string>(property.szName);
                            
                            if (!component.GetValue<string>(property.szName).Equals(property.value)) {
                                component.SetValue(property.szName, (string)property.value);
                            }
                        }
                        break;
                    }
                    case "System.Boolean": {
                        if (!component.GetValue<bool>(property.szName).Equals(property.value)) {
                            component.SetValue(property.szName, (bool)property.value);
                        }
                        break;
                    }
                    case "UnityEngine.Vector2": {
                        if (!component.GetValue<Vector2>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (Vector2)property.value);
                            break;
                        }
                    case "UnityEngine.Vector3": {
                        if (!component.GetValue<Vector3>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (Vector3)property.value);
                            break;
                        }
                    case "UnityEngine.Vector4": {
                        if (!component.GetValue<Vector4>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (Vector4)property.value);
                            break;
                        }
                    case "UnityEngine.Rect": {
                        if (!component.GetValue<Rect>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (Rect)property.value);
                            break;
                        }
                    case "UnityEngine.Quaternion": {
                        if (!component.GetValue<Quaternion>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (Quaternion)property.value);
                            break;
                        }
                    case "UnityEngine.Material": {
                        //Material material = (Material)Resources.Load(property.value.ToString(), typeof(Material));
                        Material material = null;
                        List<Material> materialsList = Resources.FindObjectsOfTypeAll<Material>().ToList();
                        if (!property.value.ToString().Equals("null")) {
                            material = materialsList.Find(m => m.name == property.value.ToString());
                        }
                        component.SetValue(property.szName, material);
                        break;
                    }
                    case "UnityEngine.Material[]": {
                            string[] s = (property.value as string).Split(new char[] { ',' });
                            List<Material> materials = new List<Material>();
                            List<Material> materialsList = Resources.FindObjectsOfTypeAll<Material>().ToList();
                            for (int j = 0; j < s.Length; j++) {
                                if (s[j].Equals("null")) {
                                    materials.Add(null);
                                    continue;
                                }
                                else {
                                    Material material = null;
                                    material = materialsList.Find(m => m.name == s[j]);
                                    materials.Add(material);
                                }
                            }
                            Material[] MaterialArray = materials.ToArray();
                            component.SetValue(property.szName, MaterialArray);
                            break;
                        }
                    case "UnityEngine.Bounds": {
                        if (!component.GetValue<Bounds>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (Bounds)property.value);
                            break;
                        }

                    case "UnityEngine.JointSpring": {
                        if (!component.GetValue<JointSpring>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointSpring)property.value);
                            break;
                        }
                    case "UnityEngine.WheelFrictionCurve": {
                        if (!component.GetValue<WheelFrictionCurve>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (WheelFrictionCurve)property.value);
                            break;
                        }
                    case "UnityEngine.JointMotor": {
                        if (!component.GetValue<JointMotor>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointMotor)property.value);
                            break;
                        }
                    case "UnityEngine.JointLimits": {
                        if (!component.GetValue<JointLimits>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointLimits)property.value);
                            break;
                        }
                    case "UnityEngine.SoftJointLimitSpring": {
                        if (!component.GetValue<SoftJointLimitSpring>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (SoftJointLimitSpring)property.value);
                            break;
                        }
                    case "UnityEngine.SoftJointLimit": {
                        if (!component.GetValue<SoftJointLimit>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (SoftJointLimit)property.value);
                            break;
                        }
                    case "UnityEngine.JointDrive": {
                        if (!component.GetValue<JointDrive>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointDrive)property.value);
                            break;
                        }
                    case "UnityEngine.JointMotor2D": {
                        if (!component.GetValue<JointMotor2D>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointMotor2D)property.value);
                            break;
                        }
                    case "UnityEngine.JointAngleLimits2D": {
                        if (!component.GetValue<JointAngleLimits2D>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointAngleLimits2D)property.value);
                            break;
                        }
                    case "UnityEngine.JointTranslationLimits2D": {
                        if (!component.GetValue<JointTranslationLimits2D>(property.szName).Equals(property.value))
                                component.SetValue(property.szName, (JointTranslationLimits2D)property.value);
                            break;
                        }
                    case "UnityEngine.JointSuspension2D": {
                        if (!component.GetValue<JointSuspension2D>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (JointSuspension2D)property.value);
                            break;
                        }
                    case "UnityEngine.RectOffset": {
                        if (!component.GetValue<RectOffset>(property.szName).Equals(property.value))
                            component.SetValue(property.szName, (RectOffset)property.value);
                            break;
                        }
                }

                if (property.bIsEnum) {
                    Type EnumType = Type.GetType(property.szTypeName + ",UnityEngine");
                    if (EnumType == null) {
                        EnumType = Type.GetType(property.szTypeName);
                    }

                    if (EnumType == null) {
                        EnumType = Type.GetType(property.szTypeName + ",UnityEngine.UI");
                    }

                    if (EnumType == null) {
                        return;
                    }

                    Enum EnumProperty = (Enum)Enum.Parse(EnumType, property.value.ToString());
                    component.SetValue(property.szName, EnumProperty);
                }
                #endregion
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

            #region AddPropertyInfo

            for (int i = 0; i < propertyInfos.Length; ++i) {
                PropertyInfo pi = propertyInfos[i];
                if (pi.CanWrite && pi.CanRead) {
                    bool bRet = pi.PropertyType.IsSubclassOf(typeof(UnityEngine.Component));
                    if (bRet) {
                        continue;
                    }

                    //lstPropertys.Add(new RDProperty(component, pi));
                }
            }

            #endregion

            #region AddFieldInfo
            for (int i = 0; i < fieldInfos.Length; ++i) {
                FieldInfo fi = fieldInfos[i];

                if (fi.IsPublic && !fi.IsLiteral) {
                    /*
                    if (fi.FieldType == typeof(double)) {
                        if (Double.IsInfinity((double)fi.GetValue(component))) {
                            continue;
                        }
                    }

                    if (fi.FieldType == typeof(Single)) {
                        if (Single.IsInfinity((Single)fi.GetValue(component))) {
                            continue;
                        }
                    }
                    
                    if(FilterList.AvailableTypeList.Find(s => s.Equals(fi.FieldType.ToString())) != null
                        || fi.FieldType.IsEnum || fi.FieldType.IsPrimitive) {
                            lstPropertys.Add(new RDProperty(component, fi));
                    }*/
                    lstPropertys.Add(new RDProperty(component, fi));
                }

            }
            #endregion

        }
        catch (Exception ex) {
            Debug.Log(ex);
        }

        return lstPropertys.ToArray();
    }
}
