using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class GameObjectExtension{

    public static void SetValue<T>(this GameObject gameObj, string propertyName, T value) {
        if (gameObj != null && !string.IsNullOrEmpty(propertyName)) {
            PropertyInfo propertyInfo = gameObj.GetType().GetProperty(propertyName);
            FieldInfo fieldInfo = gameObj.GetType().GetField(propertyName);
            if (propertyInfo != null) {
                propertyInfo.SetValue(gameObj, value, null);
            }
            else if (fieldInfo != null) {
                fieldInfo.SetValue(gameObj, value);
            }
        }
    }

    public static void SetValueBatch<T>(this GameObject gameObj, string szName, T value) {
        Transform[] arrTransforms = gameObj.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < arrTransforms.Length; ++i) {
            Transform tran = arrTransforms[i];
            tran.gameObject.SetValue<T>(szName, value);
        }
    }

    public static GameObject[] GetAllChildren(this GameObject gameObject) {
        Transform[] arrayTrans = gameObject.GetComponentsInChildren<Transform>(true);

        GameObject[] retObjs = new GameObject[arrayTrans.Length];

        for (int i = 0; i < arrayTrans.Length; ++i) {
            retObjs[i] = arrayTrans[i].gameObject;
        }

        return retObjs;
    }
}
