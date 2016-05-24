using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using LitJson;


/*static public class HandleData {
    static public List<string> ResourcesFolderPaths = new List<string>();
    static public List<CompProperty> GetComponentProperty(Component component) {
        List<PropertyInfo> pis = new List<PropertyInfo>();
        List<FieldInfo> fis = new List<FieldInfo>();
        Component comp = component;
        Type T = comp.GetType();
        pis = T.GetProperties().ToList();
        fis = T.GetFields().ToList();

        for (int i = pis.Count - 1; i >= 0; i--) {
            if (!pis[i].CanWrite) {
                pis.Remove(pis[i]);
                continue;
            }
            
        }
        
        List<CompProperty> CompPropertyInfo = new List<CompProperty>();
        CompProperty tempCompPropertyInfo = new CompProperty();
        PropertyToCompProperty convert = new PropertyToCompProperty();
        for (int i = 0; i < pis.Count; i++) {
            if (T.ToString() == "UnityEngine.BillboardRenderer" && (pis[i].PropertyType == typeof(Material) || pis[i].PropertyType == typeof(Material).MakeArrayType()))
                continue;
            else tempCompPropertyInfo = convert.PropertyInfoToCompProperty(pis[i], comp);
            if (tempCompPropertyInfo != null) {
                CompPropertyInfo.Add(tempCompPropertyInfo);
            }
        }
        for (int i = 0; i < fis.Count; i++) {
            tempCompPropertyInfo = convert.FieldInfoToCompProperty(fis[i], comp);
            if (tempCompPropertyInfo != null) {
                CompPropertyInfo.Add(tempCompPropertyInfo);
            }
                
        }
        return CompPropertyInfo;
    }

    static public void SetComponentProperty(CompProperty compProperty, Component comp) {
        switch (compProperty.type) {
            case "System.Int32": {
                if (!comp.GetValue<int>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (int)compProperty.value);
                break;
            }
            case "System.Single": {
                if (!comp.GetValue<Single>(compProperty.name).Equals(compProperty.value))
                        comp.SetValue(compProperty.name, Single.Parse(compProperty.value.ToString()));
                    break;
            }
            case "System.String": {
                if (!comp.GetValue<string>(compProperty.name).Equals(compProperty.value)) {
                        comp.SetValue(compProperty.name, (string)compProperty.value);
                    }
                    break;
                }
            case "System.Boolean": {
                if (!comp.GetValue<bool>(compProperty.name).Equals(compProperty.value)) {
                        comp.SetValue(compProperty.name, (bool)compProperty.value);
                    }
                    break;
            }
            case "UnityEngine.Vector2": {
                if (!comp.GetValue<Vector2>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (Vector2)compProperty.value);
                    break;
            }
            case "UnityEngine.Vector3": {
                if (!comp.GetValue<Vector3>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (Vector3)compProperty.value);
                    break;
                }
            case "UnityEngine.Vector4": {
                if (!comp.GetValue<Vector4>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (Vector4)compProperty.value);
                    break;
                }
            case "UnityEngine.Rect": {
                if (!comp.GetValue<Rect>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (Rect)compProperty.value);
                    break;
                }
            case "UnityEngine.Quaternion": {
                if (!comp.GetValue<Quaternion>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (Quaternion)compProperty.value);
                        break;
                }
            case "UnityEngine.Material": {
                Material material = (Material)Resources.Load(compProperty.value.ToString(), typeof(Material));
                comp.SetValue(compProperty.name, material);
                    break;
            }
            case "UnityEngine.Material[]": {
                string[] s = (compProperty.value as string).Split(new char[] { ',' });
                List<Material> materials = new List<Material>();
                List<Material> materialsList = Resources.FindObjectsOfTypeAll<Material>().ToList();
                for (int i = 0; i < s.Length; i++) {
                    if (s[i].Equals("null")) {
                        materials.Add(null);
                        continue;
                    }
                    else {
                        Material material = null;
                        material = materialsList.Find(m => m.name == s[i]);
                        materials.Add(material);
                    }
                }
                Material[] MaterialArray = materials.ToArray();
                comp.SetValue(compProperty.name, MaterialArray);
                break;
            }
            case "UnityEngine.Bounds": {
                if (!comp.GetValue<Bounds>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (Bounds)compProperty.value);
                break;
                }
            
            case "UnityEngine.JointSpring": {
                if (!comp.GetValue<JointSpring>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (JointSpring)compProperty.value);
                break;
                }
            case "UnityEngine.WheelFrictionCurve": {
                if (!comp.GetValue<WheelFrictionCurve>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (WheelFrictionCurve)compProperty.value);
                break;
                }
            case "UnityEngine.JointMotor": {
                if (!comp.GetValue<JointMotor>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (JointMotor)compProperty.value);
                break;
                }
            case "UnityEngine.JointLimits": {
                if (!comp.GetValue<JointLimits>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (JointLimits)compProperty.value);
                    break;
                }
            case "UnityEngine.SoftJointLimitSpring": {
                if (!comp.GetValue<SoftJointLimitSpring>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (SoftJointLimitSpring)compProperty.value);
                    break;
                }
            case "UnityEngine.SoftJointLimit": {
                if (!comp.GetValue<SoftJointLimit>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (SoftJointLimit)compProperty.value);
                    break;
                }
            case "UnityEngine.JointDrive": {
                if (!comp.GetValue<JointDrive>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (JointDrive)compProperty.value);
                    break;
                }
            case "UnityEngine.JointMotor2D": {
                    if (!comp.GetValue<JointMotor2D>(compProperty.name).Equals(compProperty.value))
                        comp.SetValue(compProperty.name, (JointMotor2D)compProperty.value);
                    break;
                }
            case "UnityEngine.JointAngleLimits2D": {
                if (!comp.GetValue<JointAngleLimits2D>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (JointAngleLimits2D)compProperty.value);
                    break;
                }
            case "UnityEngine.JointTranslationLimits2D": {
                if (!comp.GetValue<JointTranslationLimits2D>(compProperty.name).Equals(compProperty.value))
                        comp.SetValue(compProperty.name, (JointTranslationLimits2D)compProperty.value);
                    break;
                }
            case "UnityEngine.JointSuspension2D": {
                if (!comp.GetValue<JointSuspension2D>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (JointSuspension2D)compProperty.value);
                    break;
                }
            case "UnityEngine.RectOffset": {
                if (!comp.GetValue<RectOffset>(compProperty.name).Equals(compProperty.value))
                    comp.SetValue(compProperty.name, (RectOffset)compProperty.value);
                    break;
                }
        }
        if (compProperty.isEnum) {
            compProperty.type = compProperty.type.Replace("+", ".");
            Type EnumType = Type.GetType(compProperty.type + ",UnityEngine");
            if (EnumType == null) {
                return;
            }
            Enum EnumProperty = (Enum)Enum.Parse(EnumType, compProperty.value.ToString());
            comp.SetValue(compProperty.name, EnumProperty);
        }
    }
    static public List<CompNode> GetAllCompNode(GameObject gameObj) {
        Component[] comps = gameObj.GetComponents<Component>();
        ComponentToCompNode convert = new ComponentToCompNode();
        List<CompNode> compNodes = new List<CompNode>();
        foreach (Component comp in comps) {
            compNodes.Add(convert.compInfoToCompNode(comp));
        }
        return compNodes;
    }
    static public void GetAllFolderPath(string BasePath, string RelativePath, bool HasFindResourcesFold) {
        DirectoryInfo DI = new DirectoryInfo(BasePath);
        RelativePath = RelativePath + DI.Name + "/";
        if (DI.Name.Equals("Resources") && !HasFindResourcesFold) {
            HasFindResourcesFold = true;
            RelativePath = "";
        }
        if(HasFindResourcesFold)
            ResourcesFolderPaths.Add(RelativePath);
        List<DirectoryInfo> AllDi = DI.GetDirectories().ToList();
        foreach (DirectoryInfo di in AllDi) {
            GetAllFolderPath(di.FullName, RelativePath, HasFindResourcesFold);
        }
    } 

}*/