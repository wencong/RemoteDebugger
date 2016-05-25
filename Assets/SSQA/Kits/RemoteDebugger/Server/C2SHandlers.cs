using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            net_server.RegisterHandler(NetCmd.C2S_CmdSetObjStatic, C2S_SetObjectStatic);
            net_server.RegisterHandler(NetCmd.C2S_CmdSetObjTag, C2S_SetObjectTag);
            net_server.RegisterHandler(NetCmd.C2S_CmdSetObjLayer, C2S_SetObjectLayer);
            net_server.RegisterHandler(NetCmd.C2S_QueryComponent, C2S_QueryComponents);
            net_server.RegisterHandler(NetCmd.C2S_GetComponentProperty, C2SGetComponentProperty);
            net_server.RegisterHandler(NetCmd.C2S_EnableComponent, C2SEnableComponent);
            net_server.RegisterHandler(NetCmd.C2S_CustomComponent, C2SCustomComponent);
            this.net_server = net_server;
        }
    }

    private bool C2S_QueryAllObjs(NetCmd cmd, Cmd c) {
        GameRunTimeDataSet.InitDataSet();
#if UNITY_5_3_2 
        Scene currentScene = SceneManager.GetActiveScene();
        List<GameObject> listGameObject = currentScene.GetRootGameObjects().ToList();

#elif UNITY_5_3_2_OR_NEWER
        Scene currentScene = SceneManager.GetActiveScene();
        List<GameObject> listGameObject = currentScene.GetRootGameObjects().ToList();

#else
        List<GameObject> listGameObject = GameObject.FindObjectsOfType<GameObject>().ToList();
#endif

        List<RDGameObject> rdGameObjects = new List<RDGameObject>();
        try {
            for (int i = 0; i < listGameObject.Count; i++) {

                List<Transform> cTransforms = listGameObject[i].GetComponentsInChildren<Transform>(true).ToList();

                foreach (Transform cTransform in cTransforms) {

                    if (listGameObject.Find(obj => obj.GetInstanceID().Equals(cTransform.gameObject.GetInstanceID())) == null)
                        listGameObject.Add(cTransform.gameObject);
                }
            }

            foreach (GameObject gameObj in listGameObject) {

                GameRunTimeDataSet.AddGameObject(gameObj);

                rdGameObjects.Add(new RDGameObject(gameObj));

            }
        }
        catch (Exception ex) {
            net_server.LogMsgToClient(ex.ToString());
        }
        
        try {
            string rdGameObjList = RDDataBase.Serializer<RDGameObject[]>(rdGameObjects.ToArray());
            Cmd usCmd = new Cmd(new byte[rdGameObjList.Length + 200]);

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
        string szRecv = c.ReadString();
        int HandleFlag = c.ReadInt32();

        RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

        GameObject gameObject = null;
        GameRunTimeDataSet.TryGetGameObject(rdGameObj.nInstanceID, out gameObject);

        try {
            switch (HandleFlag) {
                case 0: {
                        gameObject.SetAllChildrenProperty("activeSelf", rdGameObj.bActive);
                        break;
                    }
                case 1: {
                        gameObject.SetActive(rdGameObj.bActive);
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
        
        Cmd usCmd = new Cmd();
        usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjActive);
        usCmd.WriteString(szRdGameObjs);
        this.net_server.SendCommand(usCmd);

        return true;
    }

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
                        gameObject.SetAllChildrenProperty("tag", rdGameObj.bTag);
                        break;
                    }
                case 1: {
                        gameObject.tag = rdGameObj.bTag;
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

        Cmd usCmd = new Cmd();
        usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjTag);
        usCmd.WriteString(szRdGameObjs);
        this.net_server.SendCommand(usCmd);
        return true;

    }

    private bool C2S_SetObjectLayer(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();
        int HandleFlag = c.ReadInt32();

        RDGameObject rdGameObj = RDDataBase.Deserializer<RDGameObject>(szRecv);

        GameObject gameObject = null;
        GameRunTimeDataSet.TryGetGameObject(rdGameObj.nInstanceID, out gameObject);

        try{
            
            switch (HandleFlag) {
                case 0: {
                        gameObject.SetAllChildrenProperty("layer", rdGameObj.bLayer);
                        break;
                    }
                case 1: {
                        gameObject.layer = rdGameObj.bLayer;
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

        Cmd usCmd = new Cmd();
        usCmd.WriteNetCmd(NetCmd.S2C_CmdSetObjLayer);
        usCmd.WriteString(szRdGameObjs);
        this.net_server.SendCommand(usCmd);

        return true;
    }

    private bool C2S_QueryComponents(NetCmd cmd, Cmd c) {
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

            string szCompsInfo = RDDataBase.Serializer<RDComponent[]>(rdComps);

            Cmd usCmd = new Cmd();
            usCmd.WriteNetCmd(NetCmd.S2C_QueryComponent);
            usCmd.WriteString(szCompsInfo);
            this.net_server.SendCommand(usCmd);
        }

        return true;
    }

    private bool C2SGetComponentProperty(NetCmd cmd, Cmd c) {
        try {
            string szRecv = c.ReadString();

            RDComponent rdComp = RDDataBase.Deserializer<RDComponent>(szRecv);

            UnityEngine.Component component = null;
            if (!GameRunTimeDataSet.TryGetComponent(rdComp.nInstanceID, out component)) {
                return false;
            }

            RDProperty[] rdPropertys = component.GetAllProperty();

            
            string szSend = RDDataBase.Serializer<RDProperty[]>(rdPropertys);

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

    private bool C2SEnableComponent(NetCmd cmd, Cmd c) {
        string szRecv = c.ReadString();

        RDComponent rdComp = null;

        rdComp = RDDataBase.Deserializer<RDComponent>(szRecv);

        Component comp = null;
        GameRunTimeDataSet.ms_componentDict.TryGetValue(rdComp.nInstanceID , out comp);

        try {
            comp.SetValue<bool>("enabled", rdComp.bEnable);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }

        return true;
    }
    private bool C2SCustomComponent(NetCmd cmd, Cmd c){

        string szRecv = c.ReadString();

        try {
            RDProperty[] rdPropertys = RDDataBase.Deserializer<RDProperty[]>(szRecv);
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
}

