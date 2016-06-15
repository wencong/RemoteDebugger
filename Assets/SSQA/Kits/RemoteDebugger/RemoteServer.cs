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
    }
	
	// Update is called once per frame
	void Update () {
        MainServer.Instance.Update();
	}

    void OnDisable() {
        MainServer.Instance.UnInit();
    }
}
