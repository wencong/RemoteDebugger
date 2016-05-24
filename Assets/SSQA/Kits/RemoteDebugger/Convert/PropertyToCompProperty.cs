using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;

/*public class PropertyToCompProperty {
    public CompProperty PropertyInfoToCompProperty(PropertyInfo PI, Component comp) {
            CompProperty compPropertyInfo = new CompProperty();
            compPropertyInfo.compType = comp.GetType().ToString();
            compPropertyInfo.type = PI.PropertyType.ToString();
            compPropertyInfo.name = PI.Name;

            if (PI.PropertyType.IsEnum) {
                compPropertyInfo.value = PI.GetValue(comp, null);
                compPropertyInfo.value = compPropertyInfo.value.ToString();
                compPropertyInfo.isEnum = true;
            }

            else if (PI.PropertyType == typeof(Material)) {
                compPropertyInfo.value = (comp as Renderer).sharedMaterial;
                if (compPropertyInfo.value != null) {
                    compPropertyInfo.value = (compPropertyInfo.value as Material).name.Replace(" (Instance)", "");
                }
                else compPropertyInfo.value = "null";
            }

            else if (PI.PropertyType == typeof(Material).MakeArrayType()) {
                compPropertyInfo.value = (comp as Renderer).sharedMaterials;
                compPropertyInfo.arraySize = (compPropertyInfo.value as Array).Length;
                string s = "";
                string materials = "";
                for (int i = 0; i < compPropertyInfo.arraySize; i++) {
                    if ((compPropertyInfo.value as Array).GetValue(i) != null) {
                        s = ((compPropertyInfo.value as Array).GetValue(i) as Material).name.Replace(" (Instance)", "");
                    }
                    else s = "null";
                    materials = materials + s + ",";
                }
                materials = materials.Substring(0, materials.Length - 1);
                compPropertyInfo.value = materials;
            }

            else if (PI.PropertyType.IsValueType && !PI.PropertyType.IsPrimitive) {
                compPropertyInfo.isUnityBaseType = true;
                System.Object value = PI.GetValue(comp, null);
                compPropertyInfo.value = JsonMapper.ToJson(value);
            }

            else compPropertyInfo.value = PI.GetValue(comp, null);
        
            return compPropertyInfo;
    }
    
    public CompProperty FieldInfoToCompProperty(FieldInfo FI, Component comp) {
        CompProperty compPropertyInfo = new CompProperty();
        compPropertyInfo.compType = comp.GetType().ToString();
        compPropertyInfo.type = FI.FieldType.ToString();
        compPropertyInfo.name = FI.Name;
        if (FI.FieldType.IsEnum) {
            compPropertyInfo.value = FI.GetValue(comp);
            compPropertyInfo.value = compPropertyInfo.value.ToString();
            compPropertyInfo.isEnum = true;
        }
        else if (FI.FieldType == typeof(Material)) {
            compPropertyInfo.value = (comp as Renderer).sharedMaterial;
            if (compPropertyInfo.value != null) {
                compPropertyInfo.value = (compPropertyInfo.value as Material).name.Replace(" (Instance)", "");
            }
            else compPropertyInfo.value = "null";
        }
        else if (FI.FieldType == typeof(Material).MakeArrayType()) {
            compPropertyInfo.value = (comp as Renderer).sharedMaterials;
            compPropertyInfo.arraySize = (compPropertyInfo.value as Array).Length;
            string s = "";
            string materials = null;
            for (int i = 0; i < compPropertyInfo.arraySize; i++) {
                if ((compPropertyInfo.value as Array).GetValue(i) != null) {
                    s = ((compPropertyInfo.value as Array).GetValue(i) as Material).name.Replace(" (Instance)", "");
                }
                else s = "null";
                materials = materials + s + ",";
            }
            materials = materials.Substring(0, materials.Length - 1);
            compPropertyInfo.value = materials;
        }
        else if (FI.FieldType.IsValueType && !FI.FieldType.IsPrimitive) {
            compPropertyInfo.isUnityBaseType = true;
            compPropertyInfo.value = FI.GetValue(comp);
        }
        else compPropertyInfo.value = FI.GetValue(comp);
        return compPropertyInfo;
    }
}*/


