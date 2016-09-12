using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJsonEx;

public abstract class IObject {
    //public bool m_bSelect = false;
    public abstract bool CanSerializer();

	public abstract string Serializer();
    public abstract IObject DeSerializer(string szMsg);


	public static string SerialierArray(IObject[] objs) {
		string[] arrayObjs = new string[objs.Length];

		for (int i = 0; i < objs.Length; ++i) {
			arrayObjs[i] = objs[i].Serializer();
		}

		return JsonMapper.ToJson(arrayObjs);
	}

	public static T[] DeserializerArray<T>(string szMsg) where T : IObject {
		string arrayObjs = JsonMapper.ToObject<string[]>(szMsg);

		T[] ret = new T[arrayObjs.Length];
		for (int i = 0; i < arrayObjs.Length; ++i) {
			T obj = new T();
			obj = obj.DeSerializer(arrayObjs[i]);
			ret[i] = obj;
		}

		return ret;
	}
}

/// <summary>
/// Game object.
/// </summary>
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

		this.m_nLayer = gameObject.layer;
		this.m_szTag = gameObject.tag;
		this.m_bActive = gameObject.activeSelf;
		this.m_bStatic = gameObject.isStatic;
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

/// <summary>
/// Comp object.
/// </summary>
public class CompObj : IObject {
	public string m_szName;
	public int m_nInstanceID;

	public bool m_bEnable;
	public bool m_bContainEnable;

	public CompObj() {
		
	}

	public CompObj(Component comp) {
		this.m_szName = comp.GetType().ToString();
		this.m_nInstanceID = comp.GetInstanceID();

		if (comp.ContainProperty("enabled")) {
			m_bContainEnable = true;
			m_bEnable = comp.GetValue<bool>("enabled");
		}
		else {
			m_bContainEnable = m_bEnable = false;
		}
	}

	public override bool CanSerializer () {
		return true;
	}

	public override string Serializer () {
		return LitJsonEx.JsonMapper.ToJson(this);
	}

	public override IObject DeSerializer (string szMsg) {
		return LitJsonEx.JsonMapper.ToObject<CompObj>(szMsg);
	}
}

/// <summary>
/// Property object.
/// </summary>
public class PropertyObj : IObject {
	public override bool CanSerializer () {
		return true;
	}

	public override string Serializer () {
		return LitJsonEx.JsonMapper.ToJson(this);
	}

	public override IObject DeSerializer (string szMsg) {
		PropertyObj obj;
		obj.MemberwiseClone();
		return LitJsonEx.JsonMapper.ToObject<CompObj>(szMsg);
	}
}
