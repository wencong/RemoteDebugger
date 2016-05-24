using UnityEngine;
using System.Collections;
using System;

public class RemoteServer : MonoBehaviour {
	public int port = 4996;
	// Use this for initialization
	void Start () {
	}

    void OnEnable() {
		MainServer.Instance.Init(port);
        FilterList.InitializeDict();
        FilterList.ListToAvailableTypeList(FilterList.readTextFileToList("FilterList/AvailableTypeList.txt"));
        FilterList.ListTo_m_AvailableTypeList(FilterList.readTextFileToList("FilterList/m_AvailableTypeList.txt"));
        FilterList.ListToHideComponent(FilterList.readTextFileToList("FilterList/HideComponent.txt"));
        FilterList.ListToPropertyHideList(FilterList.readTextFileToList("FilterList/HidePropertyList.txt"));
    }
	
	// Update is called once per frame
	void Update () {
        MainServer.Instance.Update();
	}

    void OnDisable() {
        MainServer.Instance.UnInit();
        FilterList.ClearList();
    }
}
