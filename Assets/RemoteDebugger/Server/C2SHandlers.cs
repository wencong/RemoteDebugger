using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class C2SHandlers {
	//private int buff_reverse_size = 200;
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
            net_server.RegisterHandler(NetCmd.C2S_QueryComponent, C2S_QueryComponents);
            net_server.RegisterHandler(NetCmd.C2S_GetComponentProperty, C2SGetComponentProperty);
            net_server.RegisterHandler(NetCmd.C2S_EnableComponent, C2SEnableComponent);
            net_server.RegisterHandler(NetCmd.C2S_CustomComponent, C2SCustomComponent);
            this.net_server = net_server;
        }
    }

    private bool C2S_QueryAllObjs(NetCmd cmd, Cmd c) {
        GameRunTimeDataSet.InitDataSet();

        GameObject[] arrGameObject = GameObject.FindObjectsOfType<GameObject>();
        RDGameObject[] rdGameObjects = new RDGameObject[arrGameObject.Length];

        // create all ObjNode
        for (int i = 0; i < arrGameObject.Length; ++i) {
            GameObject obj = arrGameObject[i];
            GameRunTimeDataSet.AddGameObject(obj);

            rdGameObjects[i] = new RDGameObject(obj);
        }

        Cmd usCmd = new Cmd();

        try {
            string data = RDDataBase.Serializer<RDGameObject[]>(rdGameObjects);
            usCmd.WriteNetCmd(NetCmd.S2C_CmdQueryAllObjs);
            usCmd.WriteString(data);
        }
        catch (Exception ex) {
            usCmd.WriteNetCmd(NetCmd.S2C_Log);
            usCmd.WriteString(ex.ToString());
        }
        
        this.net_server.SendCommand(usCmd);

        return true;
    }

    private bool C2S_SetObjectActive(NetCmd cmd, Cmd c) {
        int id = c.ReadInt32();
        bool active = c.ReadInt32() == 1 ? true : false;

        GameObject gameObject = null;
        GameRunTimeDataSet.ms_gameObjectDict.TryGetValue(id, out gameObject);
        gameObject.SetActive(active);

        Cmd usCmd = new Cmd();
        usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjActive);
        usCmd.WriteInt32(id);
        usCmd.WriteInt32(gameObject.activeSelf ? 1 : 0);
        this.net_server.SendCommand(usCmd);
        return true;
    }

    private bool C2S_QueryComponents(NetCmd cmd, Cmd c) {
        string data = c.ReadString();

        RDGameObject rd = RDDataBase.Deserializer<RDGameObject>(data);

        GameObject gameobject = null;

        if (GameRunTimeDataSet.TryGetGameObject(rd.nInstanceID, out gameobject)) {

            Component[] comps = gameobject.GetComponents<Component>();
            RDComponent[] rdComps = new RDComponent[comps.Length];

            for (int i = 0; i < comps.Length; ++i) {
                rdComps[i] = new RDComponent(comps[i]);
            }

            string szCompsInfo = RDDataBase.Serializer<RDComponent[]>(rdComps);

            Cmd usCmd = new Cmd();
            usCmd.WriteNetCmd(NetCmd.S2C_QueryComponent);
            usCmd.WriteString(szCompsInfo);
            this.net_server.SendCommand(usCmd);
        }

        return true;
    }

    private bool C2SGetComponentProperty(NetCmd cmd, Cmd c) {
        //Util.Log(this.net_server, "C2SGetComponentProperty");

        int id = c.ReadInt32();
        Component comp = null;
        GameRunTimeDataSet.ms_componentDict.TryGetValue(id, out comp);
        

        if (comp == null) {
            Util.Log(this.net_server, string.Format("componet {0} is null, total componentDict size: {1} ", id, GameRunTimeDataSet.ms_componentDict.Count));
        }
            
        
        //Util.Log(this.net_server, CompPropertyList.ToString());
        
        //Util.Log(this.net_server, string.Format("{0} contains {1} propertys", comp.name,  CompPropertyList.Count));
        try {

            List<CompProperty> CompPropertyList = HandleData.GetComponentProperty(comp);
            foreach (CompProperty Property in CompPropertyList) {
                Cmd usCmd = new Cmd();
                usCmd.WriteNetCmd(NetCmd.S2C_GetComponentProperty);
                string Json_Property = JsonMapper.ToJson(Property);
                usCmd.WriteString(Json_Property);
                if (Property.isUnityBaseType) {
                    string Json_Value = JsonMapper.ToJson(Property.value);
                    usCmd.WriteString(Json_Value);
                }
                this.net_server.SendCommand(usCmd);
            }
        }
        catch (Exception ex) {
            Debug.Log(ex);
        }
        
        return true;
    }

    private bool C2SEnableComponent(NetCmd cmd, Cmd c) {
        int componentID = c.ReadInt32();
        int enabled = c.ReadInt32();
        Component comp = null;
        GameRunTimeDataSet.ms_componentDict.TryGetValue(componentID, out comp);
        bool enable = enabled == 1 ? true : false;

        try {
            comp.SetValue<bool>("enabled", enable);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }

        return true;
    }
    private bool C2SCustomComponent(NetCmd cmd, Cmd c){
        int id = c.ReadInt32();
        string data = c.ReadString();
        CompProperty compProperty = JsonMapper.ToObject<CompProperty>(data);
        if (compProperty.isUnityBaseType) {
            string value = c.ReadString();
            compProperty.UnityBaseTypeDeserialize(value);
        }
        Component comp = null;
        GameRunTimeDataSet.ms_componentDict.TryGetValue(id, out comp);
        try {
            HandleData.SetComponentProperty(compProperty, comp);
        }
        catch (Exception ex) {
            Util.Log(this.net_server, ex.ToString());
        }
        return true;
    }
}

