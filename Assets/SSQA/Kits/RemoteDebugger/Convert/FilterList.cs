using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

static public class FilterList {
    static public List<string> AvailableTypeList = null;
    static public List<string> m_AvailableTypeList = null;
    static public List<string> HideComponent = null;
    static public Dictionary<string, List<string>> HidePropertyList = null;
    static public void InitializeDict(){
        AvailableTypeList = new List<string>();
        m_AvailableTypeList = new List<string>();
        HideComponent = new List<string>();
        HidePropertyList = new Dictionary<string, List<string>>();
    }
    static public void ClearList() {
        AvailableTypeList.Clear();
        m_AvailableTypeList.Clear();
        HideComponent.Clear();
        HidePropertyList.Clear();
    }
    static public List<string> readTextFileToList(string fileName) {
        TextAsset txtFile;
        txtFile = (TextAsset)Resources.Load(fileName.Replace(".txt", ""));

        string m_list = txtFile.text;
        List<string> list = m_list.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
        return list;
    }
    static public void ListToAvailableTypeList(List<string> list) {
        AvailableTypeList = list;
    }
    static public void ListTo_m_AvailableTypeList(List<string> list) {
        m_AvailableTypeList = list;
    }
    static public void ListToHideComponent(List<string> list) {
        HideComponent = list;
    }
    static public void ListToPropertyHideList(List<string> list){
        List<string> compProperty = new List<string>();
        string compName;
        foreach (string s in list) {
            if (s == "#####") {
                if (compProperty.Count >= 2) {
                    compName = compProperty[0];
                    compProperty.RemoveAt(0);
                    List<string> str = new List<string>(compProperty);
                    if (!HidePropertyList.ContainsKey(compName))
                        HidePropertyList.Add(compName, str);
                    compProperty.Clear();
                    continue;
                }
            }
            compProperty.Add(s);
        }
    }
    
}
