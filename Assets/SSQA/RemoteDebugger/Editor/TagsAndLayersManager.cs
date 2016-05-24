using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;

public class TagsAndLayersManager{
    public static bool HasTag(string tag) {
        for (int i = 0; i < InternalEditorUtility.tags.Length; i++) {
            if (InternalEditorUtility.tags[i].Equals(tag)) {
                return true;
            }
        }
        return false;
    }

    public static void creatTag(string tag) {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tm_Property = tagManager.GetIterator();
        while (tm_Property.NextVisible(true)) {
            if (tm_Property.name == "tags") {
                for (int i = 0; i < tm_Property.arraySize; i++) {
                    SerializedProperty m_Tag = tm_Property.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(tm_Property.GetArrayElementAtIndex(i).stringValue)) {
                        m_Tag.stringValue = tag;
                        tagManager.ApplyModifiedProperties();
                        return;
                    }
                }
            }
        }
    }

    public static bool HasLayer(int layer) {
        for (int i = 0; i < InternalEditorUtility.layers.Length; i++) {
            /*if (i == layer) {
                return true;
            }*/
            Debug.Log(InternalEditorUtility.GetLayerName(5));
            //Debug.Log(InternalEditorUtility.layers[i]);
        }
        return false;
    }

    public static void creatLayer(string layer) {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tm_Property = tagManager.GetIterator();
        while (tm_Property.NextVisible(true)) {
            if (tm_Property.name.StartsWith("User Layer")) {
                if (tm_Property.type == "string") {
                    if (string.IsNullOrEmpty(tm_Property.stringValue)) {
                        tm_Property.stringValue = layer;
                        tagManager.ApplyModifiedProperties();
                        return;
                    }
                }
            }
        }
    }

	
}
