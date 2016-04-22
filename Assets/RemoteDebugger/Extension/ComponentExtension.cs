using UnityEngine;
using System;
using System.Collections;
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
}
