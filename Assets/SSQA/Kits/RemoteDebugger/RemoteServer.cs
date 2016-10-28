using UnityEngine;
using System.Collections;
using System;

namespace RemoteDebugger {
    public class RemoteServer : MonoBehaviour {
        public static bool enable = false;

        public int port = 4996;
        // Use this for initialization
        void Start() {
        }

        void OnEnable() {
            MainServer.Instance.Init(port);
        }

        // Update is called once per frame
        void Update() {
            MainServer.Instance.Update();
        }

        void OnDisable() {
            MainServer.Instance.UnInit();
        }
    }
}