using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5_3_2 
using UnityEngine.SceneManagement;
#endif

using LitJsonEx;

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

            //net_server.RegisterHandler(NetCmd.C2S_CmdSetObjStatic, C2S_SetObjectStatic);
            //net_server.RegisterHandler(NetCmd.C2S_CmdSetObjTag, C2S_SetObjectTag);
            net_server.RegisterHandler(NetCmd.C2S_CmdSetObjLayer, C2S_SetObjectLayer);

            net_server.RegisterHandler(NetCmd.C2S_QueryComponent, C2S_QueryComponents);
            net_server.RegisterHandler(NetCmd.C2S_GetComponentProperty, C2S_GetComponentProperty);
            net_server.RegisterHandler(NetCmd.C2S_EnableComponent, C2S_EnableComponent);
            net_server.RegisterHandler(NetCmd.C2S_ModifyComponentProperty, C2S_ModifyComponentProperty);
            net_server.RegisterHandler(NetCmd.C2S_CustomCmd, C2S_CustomCmd);

            CustomCmdExecutor.Instance.Init();

            this.net_server = net_server;
        }
    }

    private bool C2S_QueryAllObjs(NetCmd cmd, Cmd c) {
        GameRunTimeDataSet.InitDataSet();

        List<GameObject> _RootGameObjects = new List<GameObject>();
#if UNITY_5_3_2 
        Scene currentScene = SceneManager.GetActiveScene();
        _RootGameObjects = currentScene.GetRootGameObjects().ToList();
#else
        Transform[] arrTransforms = Transform.FindObjectsOfType<Transform>();
        for (int i = 0; i < arrTransforms.Length; ++i) {
            Transform tran = arrTransforms[i];
            if (tran.parent == null) {
                _RootGameObjects.Add(tran.gameObject);
            }
        }
#endif
        List<RDGameObject> rdGameObjects = new List<RDGameObject>();

        try {
            for (int i = 0; i < _RootGameObjects.Count; i++) {
                GameObject _root = _RootGameObjects[i];
                Transform[] trans = _root.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < trans.Length; ++j ) {
                    Transform tran = trans[j];
                    rdGameObjects.Add(new RDGameObject(tran.gameObject));
                    GameRunTimeDataSet.AddGameObject(tran.gameObject);
                }
            }
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }

        try {
            string rdGameObjList = RDDataBase.SerializerArray<RDGameObject>(rdGameObjects.ToArray());
            Cmd usCmd = new Cmd(rdGameObjList.Length);

            usCmd.WriteNetCmd(NetCmd.S2C_CmdQueryAllObjs);
            usCmd.WriteString(rdGameObjList);
            this.net_server.SendCommand(usCmd);
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());

            return false;
        }

        return true;
    }

    private bool C2S_SetObjectActive(NetCmd cmd, Cmd c) {
        try {
            string szRecv = c.ReadString();

            RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

            GameObject gameObject = null;
            GameRunTimeDataSet.TryGetGameObject(rdGameObj.nInstanceID, out gameObject);

            gameObject.SetActive(rdGameObj.bActive);

            string szSend = RDDataBase.Serializer<RDGameObject>(new RDGameObject(gameObject));

            Cmd usCmd = new Cmd(szSend.Length);
            usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjActive);
            usCmd.WriteString(szSend);
            this.net_server.SendCommand(usCmd);
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }

        return true;
    }

    /*
    private bool C2S_SetObjectStatic(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();
        int HandleFlag = c.ReadInt32();

        RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

        GameObject gameObject = null;
        GameRunTimeDataSet.TryGetGameObject(rdGameObj.nInstanceID, out gameObject);

        try {
            switch (HandleFlag) {
                case 0: {
                        gameObject.SetAllChildrenProperty("isStatic", rdGameObj.bStatic);
                        break;
                    }
                case 1: {
                        gameObject.isStatic = rdGameObj.bStatic;
                        break;
                    }
            }
        }
        catch(Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }

        List<RDGameObject> rdGameObjs = new List<RDGameObject>();

        List<Transform> cTransforms = gameObject.GetComponentsInChildren<Transform>(true).ToList();

        foreach (Transform cTransform in cTransforms) {
            rdGameObjs.Add(new RDGameObject(cTransform.gameObject));
        }

        string szRdGameObjs = RDDataBase.Serializer<List<RDGameObject>>(rdGameObjs);

        Cmd usCmd = new Cmd();
        usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjStatic);
        usCmd.WriteString(szRdGameObjs);
        this.net_server.SendCommand(usCmd);

        return true;

    }

    private bool C2S_SetObjectTag(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();
        int HandleFlag = c.ReadInt32();

        RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

        GameObject gameObject = null;
        GameRunTimeDataSet.TryGetGameObject(rdGameObj.nInstanceID, out gameObject);

        try {
            switch (HandleFlag) {
                case 0: {
                        gameObject.SetAllChildrenProperty("tag", rdGameObj.szTag);
                        break;
                    }
                case 1: {
                        gameObject.tag = rdGameObj.szTag;
                        break;
                    }
            }
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }

        List<RDGameObject> rdGameObjs = new List<RDGameObject>();

        List<Transform> cTransforms = gameObject.GetComponentsInChildren<Transform>(true).ToList();

        foreach (Transform cTransform in cTransforms) {
            rdGameObjs.Add(new RDGameObject(cTransform.gameObject));
        }

        string szRdGameObjs = RDDataBase.Serializer<List<RDGameObject>>(rdGameObjs);

        Cmd usCmd = new Cmd(szRdGameObjs.Length);
        usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjTag);
        usCmd.WriteString(szRdGameObjs);
        this.net_server.SendCommand(usCmd);
        return true;

    }
     * */

    private bool BatchModify<T1, T2>(T1[] objs, string szName, T2 value) {
        try {
            for (int i = 0; i < objs.Length; ++i) {
                T1 obj = objs[i];

                PropertyInfo propertyInfo = obj.GetType().GetProperty(szName);
                FieldInfo fieldInfo = obj.GetType().GetField(szName);

                if (propertyInfo != null) {
                    propertyInfo.SetValue(obj, value, null);
                }
                else if (fieldInfo != null) {
                    fieldInfo.SetValue(obj, value);
                }
                else {
                    return false;
                }
            }
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
            return false;
        }
        return true;
    }

    private bool C2S_SetObjectLayer(NetCmd cmd, Cmd c) {
        try {
            string szRecv = c.ReadString();
            BatchOption eBatchOption = (BatchOption)c.ReadInt32();

            RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

            GameObject gameObject = null;
            GameRunTimeDataSet.TryGetGameObject(rdGameObj.nInstanceID, out gameObject);

            GameObject[] arrayGameObjectBeModify = null;

            switch (eBatchOption) {
                case BatchOption.eOnlySelf: {
                    //gameObject.layer = rdGameObj.nLayer;
                    arrayGameObjectBeModify = new GameObject[1] { gameObject };
                    break;
                }
                case BatchOption.eContainChildren: {
                    //gameObject.SetValueBatch<int>("layer", rdGameObj.nLayer);
                    arrayGameObjectBeModify = gameObject.GetAllChildren();
                    break;
                }
            }
            /*
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }

        try {*/
            BatchModify<GameObject, int>(arrayGameObjectBeModify, "layer", rdGameObj.nLayer);

            RDGameObject[] rdGameObjs = new RDGameObject[arrayGameObjectBeModify.Length];

            for (int i = 0; i < rdGameObjs.Length; ++i ) {
                rdGameObjs[i] = new RDGameObject(arrayGameObjectBeModify[i]);
            }

            string szRdGameObjs = RDDataBase.SerializerArray(rdGameObjs);

            Cmd usCmd = new Cmd(szRdGameObjs.Length);
            usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjLayer);
            usCmd.WriteString(szRdGameObjs);
            this.net_server.SendCommand(usCmd);
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }

        return true;
    }

    private bool C2S_QueryComponents(NetCmd cmd, Cmd c) {
        try {
            string szRecv = c.ReadString();

            RDGameObject rd = RDDataBase.Deserializer<RDGameObject>(szRecv);

            GameObject gameobject = null;

            if (GameRunTimeDataSet.TryGetGameObject(rd.nInstanceID, out gameobject)) {

                Component[] comps = gameobject.GetComponents<Component>();
                RDComponent[] rdComps = new RDComponent[comps.Length];

                for (int i = 0; i < comps.Length; ++i) {
                    rdComps[i] = new RDComponent(comps[i]);
                    GameRunTimeDataSet.AddComponent(comps[i]);
                }

                string szCompsInfo = RDDataBase.SerializerArray(rdComps);

                Cmd usCmd = new Cmd(szCompsInfo.Length);
                usCmd.WriteNetCmd(NetCmd.S2C_QueryComponent);
                usCmd.WriteString(szCompsInfo);
                this.net_server.SendCommand(usCmd);
            }
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }
        
        return true;
    }

    private bool C2S_GetComponentProperty(NetCmd cmd, Cmd c) {
        try {
            string szRecv = c.ReadString();

            RDComponent rdComp = RDDataBase.Deserializer<RDComponent>(szRecv);

            UnityEngine.Component component = null;
            if (!GameRunTimeDataSet.TryGetComponent(rdComp.nInstanceID, out component)) {
                return false;
            }
            
            RDProperty[] rdPropertys = component.GetPropertys();
            
            string szSend = RDDataBase.SerializerArray<RDProperty>(rdPropertys);
            
            Cmd usCmd = new Cmd(new byte[szSend.Length + 200]);

            usCmd.WriteNetCmd(NetCmd.S2C_GetComponentProperty);

            usCmd.WriteString(szSend);

            this.net_server.SendCommand(usCmd);
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
            return false;
        }
        
        return true;
    }

    private bool C2S_EnableComponent(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();

        RDComponent rdComp = null;

        rdComp = RDDataBase.Deserializer<RDComponent>(szRecv);

        Component comp = null;
        GameRunTimeDataSet.ms_componentDict.TryGetValue(rdComp.nInstanceID , out comp);

        try {
            //comp.SetValue<bool>("enabled", rdComp.bEnable);
            ComponentExtension.SetValue<bool>(comp, "enabled", rdComp.bEnable);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }

        return true;
    }
    private bool C2S_ModifyComponentProperty(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();

        try {
            RDProperty[] rdPropertys = RDDataBase.DeserializerArray<RDProperty>(szRecv);
            Component component = null;

            if (!GameRunTimeDataSet.TryGetComponent(rdPropertys[0].nComponentID, out component)) {
                return false;
            }

            component.SetPropertys(rdPropertys);
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }
        return true;
    }

    private bool C2S_CustomCmd(NetCmd cmd, Cmd c) {
        try {
            string szRecv = c.ReadString();
            string[] arrayCmd = szRecv.Split();

            if (arrayCmd.Length == 0) {
                return false;
            }

            bool ret = CustomCmdExecutor.Instance.Execute(arrayCmd);
            if (!ret) {
                net_server.LogMsgToClient(string.Format("Custom Cmd: {0} execute failed", arrayCmd[0]));
            }
            else {
                net_server.LogMsgToClient(string.Format("Custom Cmd: {0} execute Success", arrayCmd[0]));
            }
            return ret;
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
            return true;
        }
        
    }
}

