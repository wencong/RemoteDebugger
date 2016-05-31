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
        typeof(Rigidbody)
    };

    public static Type GetTypeByName(string szTypeName) {
        Type T = null;

        if (szTypeName.Contains("UnityEngine")) {
            T = Type.GetType(szTypeName + ",UnityEngine");
        }
        else {
            T = Type.GetType(szTypeName);
        }
        return T;
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

