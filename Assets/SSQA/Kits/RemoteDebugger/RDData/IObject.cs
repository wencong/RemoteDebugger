using System;
using UnityEngine;
using System.Collections;
using System.Reflection;
using LitJsonEx;

public abstract class IObject {
    public bool m_bSelect = false;
    public string m_szTypeName;
    public abstract bool CanSerializer();
    public abstract string Serializer();
    public abstract IObject DeSerializer(string szMsg);
}

public class GameObj : IObject {
    public string m_szName;
    public int m_nInstanceID;
    public int m_nParentID;
    public int[] m_childrenID;

    public int m_nLayer;
    public string m_szTag;
    public bool m_bActive;
    public bool m_bStatic;

    public GameObj(GameObject gameObject) {
        Transform trans = gameObject.transform;

        this.m_szName = gameObject.name;
        this.m_nInstanceID = gameObject.GetInstanceID();
        
        if (trans.parent != null) {
            this.m_nParentID = trans.parent.gameObject.GetInstanceID();
        }
        else {
            this.m_nParentID = -1;
        }

        this.m_childrenID = new int[trans.childCount];
        for (int i = 0, count = trans.childCount; i < count; ++i) {
            this.m_childrenID[i] = trans.GetChild(i).gameObject.GetInstanceID();
        }

    }
    public GameObj() {

    }

    public override bool CanSerializer() {
        return true;
    }
    public override string Serializer() {
        return LitJsonEx.JsonMapper.ToJson(this);
    }

    public override IObject DeSerializer(string szMsg) {
        return LitJsonEx.JsonMapper.ToObject<GameObj>(szMsg);
    }
}

public class CompObj : IObject {

}

public class PropertyObj : IObject {

}
