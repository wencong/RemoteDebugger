using System;
using System.Collections.Generic;
using UnityEngine;

public class S2CHandlers {
    private static S2CHandlers inst = null;
    public static S2CHandlers Instance {
        get {
            if (inst == null) {
                inst = new S2CHandlers();
            }
            return inst;
        }
    }

    public delegate void DelegateMethod();
    public DelegateMethod OnUpdateData = null;

    public void Init(NetClient net_client) {
        if (net_client != null) {
            net_client.RegisterHandler(NetCmd.S2C_CmdQueryAllObjs, S2CQueryAllObjs);
            net_client.RegisterHandler(NetCmd.S2C_CmdSetObjActive, S2CSetObjActive);
            net_client.RegisterHandler(NetCmd.S2C_QueryComponent, S2CQueryComponent);
            net_client.RegisterHandler(NetCmd.S2C_EnableComponent, S2CEnableComponent);
        }
    }

    public bool S2CQueryAllObjs(NetCmd cmd, Cmd c) {
        string data = c.ReadString();

        ShowPanelDataSet.ms_rootObj = Util.String2ObjNode(data);

        if (OnUpdateData != null) {
            OnUpdateData();
        }

        return true;
    }

    public bool S2CSetObjActive(NetCmd cmd, Cmd c) {
        Int16 ret = c.ReadInt16();
        if (ret == 0) {
            Debug.LogError("Set Active false");
        }
        else {
            string data = c.ReadString();
            ObjNode[] arr_nodes = Util.String2NodeArray(data);
            for (int i = 0; i < arr_nodes.Length; ++i) {
                ObjNode node = arr_nodes[i];
                ObjNode cacheNode = null;

                ShowPanelDataSet.ms_objNodeDict.TryGetValue(node.instance_id, out cacheNode);
                if (cacheNode != null) {
                    cacheNode.active = node.active;
                }
            }
        }
        return true;
    }

    public bool S2CQueryComponent(NetCmd cmd, Cmd c) {
        Int16 ret = c.ReadInt16();
        if (ret == 0) {
            return false;
        }

        int id = c.ReadInt32();
        ObjNode cacheNode = null;

        ShowPanelDataSet.ms_objNodeDict.TryGetValue(id, out cacheNode);

        if (cacheNode != null) {
            string data = c.ReadString();
            cacheNode.self_componets = Util.String2Comps(data);
        }

        if (OnUpdateData != null) {
            OnUpdateData();
        }

        return true;
    }

    public bool S2CEnableComponent(NetCmd cmd, Cmd c) {

        return true;
    }
}
