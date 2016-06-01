using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using LitJsonEx;


public static class Util {
    public static Type[] arrayAssetType = new Type[] {
        typeof(Material),
        typeof(Material[]),
        typeof(Mesh),
        typeof(Shader),
        typeof(Texture),
        typeof(Rigidbody),
        typeof(Transform)
    };

    public static Type GetTypeByName(string szTypeName) {
        Type retType = null;

        Assembly currentAssemly = Assembly.GetExecutingAssembly();
        retType = currentAssemly.GetType(szTypeName);
        if (retType != null) {
            return retType;
        }

        AssemblyName[] assemblyNames = currentAssemly.GetReferencedAssemblies();
        for (int i = 0; i < assemblyNames.Length; ++i) {
            Assembly assembly = Assembly.Load(assemblyNames[i]);
            retType = assembly.GetType(szTypeName);
            if (retType != null) {
                return retType;
            }
        }

        return null;
    }

    public static bool IsAsset(string szTypeName) {
        Type t = GetTypeByName(szTypeName);

        if (t == null) {
            return false;
        }

        for (int i = 0; i < arrayAssetType.Length; ++i) {
            if (t.Equals(arrayAssetType[i]) || t.IsSubclassOf(arrayAssetType[i])) {
                return true;
            }
        }
        return false;
    }
}

