using System;
using UnityEngine;
using System.Collections.Generic;

public class C2SHandlers {
	private int buff_reverse_size = 200;
    private static C2SHandlers inst = null;
    public static C2SHandlers Instance {
        get {
            if (inst == null) {
                inst = new C2SHandlers();
            }
            return inst;
        }
    }

    private NetServer net_server = null;
    public void Init(NetServer net_server) {
        if (net_server != null) {
            net_server.RegisterHandler(NetCmd.C2S_CmdQueryAllObjs, C2S_QueryAllObjs);
            net_server.RegisterHandler(NetCmd.C2S_CmdSetObjActive, C2S_SetObjectActive);
            net_server.RegisterHandler(NetCmd.C2S_QueryComponent, C2SQueryComponents);
            net_server.RegisterHandler(NetCmd.C2S_EnableComponent, C2SEnableComponent);
            this.net_server = net_server;
        }
    }

    private void _QueryGameObject(GameObject gameObject, ObjNode objInfo) {
        objInfo.name = gameObject.name;
        objInfo.instance_id = gameObject.GetInstanceID();
        objInfo.active = gameObject.activeSelf;
        if (gameObject.transform.parent == null) {
            objInfo.parent_id = 0;
        }
        else {
            objInfo.parent_id = gameObject.transform.parent.gameObject.GetInstanceID();
        }

        GameObject go = null;
        if (!GameRunTimeDataSet.ms_gameObjectDict.TryGetValue(objInfo.instance_id, out go)) {
            GameRunTimeDataSet.ms_gameObjectDict.Add(objInfo.instance_id, gameObject);
        }
        
        for(int i = 0; i < gameObject.transform.childCount; ++i) {
            GameObject child_gameobject = gameObject.transform.GetChild(i).gameObject;
            ObjNode child_objinfo = new ObjNode();
            _QueryGameObject(child_gameobject, child_objinfo);
            objInfo.AddChild(child_objinfo);
        }
    }

    private bool C2S_QueryAllObjs(NetCmd cmd, Cmd c) {
        if (GameRunTimeDataSet.ms_gameObjectDict == null) {
            GameRunTimeDataSet.ms_gameObjectDict = new Dictionary<int, GameObject>();
        }
        else {
            GameRunTimeDataSet.ms_gameObjectDict.Clear();
        }

        if (GameRunTimeDataSet.ms_nodeDict == null) {
            GameRunTimeDataSet.ms_nodeDict = new Dictionary<int, ObjNode>();
        }
        else {
            GameRunTimeDataSet.ms_nodeDict.Clear();
        }
        UnityEngine.Object[] objs = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));

        // create all ObjNode
        for (int i = 0; i < objs.Length; ++i) {
            GameObject gameObject = objs[i] as GameObject;
            ObjNode node = new ObjNode();
            node.name = gameObject.name;
            node.instance_id = gameObject.GetInstanceID();
            node.active = gameObject.activeSelf;
            node.expand = false;
            GameRunTimeDataSet.ms_nodeDict.Add(node.instance_id, node);
            GameRunTimeDataSet.ms_gameObjectDict.Add(node.instance_id, gameObject);
        }

        // attach child node/ create node tree
        ObjNode rootNode = new ObjNode(0, "root", true);
        rootNode.parent_id = -1;

        for (int i = 0; i < objs.Length; ++i) {
            GameObject gameObject = objs[i] as GameObject;
            ObjNode child = null;
            ObjNode parent = null;

            if (gameObject.transform.parent == null) {
                parent = rootNode;
            }
            else {
                int parent_id = gameObject.transform.parent.gameObject.GetInstanceID();
                GameRunTimeDataSet.ms_nodeDict.TryGetValue(parent_id, out parent);
            }

            int self_id = gameObject.GetInstanceID();
            GameRunTimeDataSet.ms_nodeDict.TryGetValue(self_id, out child);

            if (parent != null || child != null) {
                parent.AddChild(child);
            }
        }

        string data = Util.ObjNode2String(rootNode);

		byte[] by_send_buff = new byte[data.Length + buff_reverse_size];
		Cmd usCmd = new Cmd(by_send_buff);

        usCmd.WriteNetCmd(NetCmd.S2C_CmdQueryAllObjs);
        usCmd.WriteString(data);
        this.net_server.SendCommand(usCmd);

        return true;
    }

    private bool C2S_SetObjectActive(NetCmd cmd, Cmd c) {
        string data = c.ReadString();
        char[] split = {','};
        string[] info = data.Split(split);

        int id = int.Parse(info[0]);
        string name = info[1];
        bool active = info[2] == "1" ? true : false;

        GameObject gameObject = null;
        GameRunTimeDataSet.ms_gameObjectDict.TryGetValue(id, out gameObject);

		Cmd usCmd = null;
		if (gameObject == null || gameObject.name != name) {
			usCmd = new Cmd();
			usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjActive);
			usCmd.WriteInt16(1);
		}
		else {
			gameObject.SetActive(active);
			ObjNode node = new ObjNode();
			_QueryGameObject(gameObject, node);
			string str_ret = Util.ObjNode2String(node);
			
			byte[] by_send_buff = new byte[str_ret.Length + buff_reverse_size];
			
			usCmd = new Cmd(by_send_buff);
			usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjActive);
			usCmd.WriteInt16(1);
			usCmd.WriteString(str_ret);
		}
       
        this.net_server.SendCommand(usCmd);

        return true;
    }

    private bool C2SQueryComponents(NetCmd cmd, Cmd c) {
        Cmd usCmd = new Cmd();
        usCmd.WriteNetCmd(NetCmd.S2C_QueryComponent);

        int id = c.ReadInt32();

        GameObject gameObject = null;
        GameRunTimeDataSet.ms_gameObjectDict.TryGetValue(id, out gameObject);

        if (gameObject == null) {
            usCmd.WriteInt16(0);
        }
        else {
            usCmd.WriteInt16(1);
            usCmd.WriteInt32(id);

            Component[] comps = gameObject.GetComponents<Component>();
            string data = Util.Comps2String(comps);

            usCmd.WriteString(data);
        }

        this.net_server.SendCommand(usCmd);

        return true;
    }

    private bool C2SEnableComponent(NetCmd cmd, Cmd c) {
        int gameObjectID = c.ReadInt32();
        int componentID = c.ReadInt32();
        int enabled = c.ReadInt32();

        GameObject gameObject = null;
        GameRunTimeDataSet.ms_gameObjectDict.TryGetValue(gameObjectID, out gameObject);
        if (gameObject != null) {
            Component[] comps = gameObject.GetComponents<Component>();
            for (int i = 0; i < comps.Length; ++i) {
                Component comp = comps[i];
                if (componentID == comp.GetInstanceID()) {
					bool enable = enabled == 1 ? true : false;

                    try {
						comp.SetValue<bool>("enabled", enable);
						/*
                        Type t = comp.GetType();
                        if (t.IsSubclassOf(typeof(Renderer))) {
                            (comp as Renderer).enabled = enable;
                        }
                        else if (t.IsSubclassOf(typeof(MonoBehaviour))) {
                            (comp as MonoBehaviour).enabled = enable;
                        }
                        else if (t.IsSubclassOf(typeof(Behaviour))) {
                            (comp as Behaviour).enabled = enable;
                        }
                        else if (t.IsSubclassOf(typeof(Collider))) {
                            (comp as Collider).enabled = enable;
                        }
                        else {
                            Debug.LogError("Unsupport Component!!!");
                        }
                        */
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        return true;
    }
}

