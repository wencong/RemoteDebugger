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

    public static void SetAllChildrenProperty<T>(this GameObject gameObj, string pName, T pValue) {

        List<Transform> cTransforms = gameObj.GetComponentsInChildren<Transform>(true).ToList();

        foreach (Transform cTransform in cTransforms) {

            if (pName.Equals("activeSelf") && typeof(T).Equals(typeof(bool))) {

                bool active = pValue.ToString().Equals("True") ? true : false;
                cTransform.gameObject.SetActive(active);

            }
            else {
                cTransform.gameObject.SetValue(pName, pValue);
            }

        }

    }
}
