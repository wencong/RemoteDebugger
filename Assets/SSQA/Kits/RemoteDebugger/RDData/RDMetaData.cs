using System;
using UnityEngine;
using System.Collections;
using System.Reflection;

public enum BatchOption {
    eContainChildren,
    eOnlySelf,
    eCancle
}

/*
public abstract class IMetaObj {
    public string szSelfTypeName;
    public string szValueTypeName;

    public System.Object value;

    public bool bIsSerialized;

    public IMetaObj(string type1, string type2, System.Object value) {
        this.szSelfTypeName = type1;
        this.szValueTypeName = type2;
        this.value = value;
    }

    public virtual Type GetSelfType() {
        return Util.GetTypeByName(szSelfTypeName);
    }

    public virtual Type GetValueType() {
        return Util.GetTypeByName(szValueTypeName);
    }

    public virtual bool IsEnum() {
        return false;
    }

    public virtual bool IsAsset() {
        return false;
    }

    public virtual bool OnClick(params System.Object[] args) {
        return true;
    }
}

public class RDGameObject : IMetaObj {
    public string szName;
    public int nInstanceID;

    public bool bActive;
    public bool bStatic;

    public string szTag;
    public int nLayer;

    public int nParentID;
    public int[] arrChildren;

    public bool bExpand;

    public RDGameObject()
        : base("", "", "") {

    }

    public RDGameObject(GameObject gameobject)
        : base("RDGameObject", "System.String", "") {
        this.szName = gameobject.name;
        this.nInstanceID = gameobject.GetInstanceID();
        this.bActive = gameobject.activeSelf;
        this.bStatic = gameobject.isStatic;
        this.szTag = gameobject.tag;
        this.nLayer = gameobject.layer;

        if (gameobject.transform.parent != null) {
            this.nParentID = gameobject.transform.parent.gameObject.GetInstanceID();
        }
        else {
            this.nParentID = -1;
        }

        arrChildren = new int[gameobject.transform.childCount];

        for (int i = 0; i < arrChildren.Length; ++i) {
            arrChildren[i] = gameobject.transform.GetChild(i).gameObject.GetInstanceID();
        }
    }
}

public class RDComponent : IMetaObj {
    public int nInstanceID;
    public string szName;
    public bool bExpand;

    public bool bContainEnable;
    public bool bEnable;

    public RDComponent()
        : base("RDComponent", typeof(string).ToString(), "") {

    }

    public RDComponent(Component comp)
        : base("RDComponent", typeof(string).ToString(), "") {
        this.nInstanceID = comp.GetInstanceID();
        this.szName = comp.GetType().ToString();

        if (comp.ContainProperty("enabled")) {
            bContainEnable = true;
            bEnable = comp.GetValue<bool>("enabled");
        }
        else {
            bContainEnable = bEnable = false;
        }
    }
}

public class RDProperty : IMetaObj {
    public int nComponentID = 0;

    public int nMemType;

    public string szName;

    public RDProperty()
        : base("RDProperty", "", "") {
    }

    public RDProperty(Component comp, MemberInfo mi)
        : base("RDProperty", "", "") {

        this.nComponentID = comp.GetInstanceID();
        this.nMemType = (int)mi.MemberType;

        this.szName = mi.Name;

        Type typ = null;

        if (mi.MemberType.Equals(MemberTypes.Property)) {
            typ = ((PropertyInfo)mi).PropertyType;
            this.szValueTypeName = typ.ToString();
            this.value = ((PropertyInfo)mi).GetValue(comp, null);
        }
        else if (mi.MemberType.Equals(MemberTypes.Field)) {
            typ = ((FieldInfo)mi).FieldType;
            this.szValueTypeName = typ.ToString();
            this.value = ((FieldInfo)mi).GetValue(comp);
        }

        // deal with Infinity
        if (typ.Equals(typeof(double)) && double.IsInfinity((double)this.value))  {
            this.value = 0.0;
        }

        else if (typ.Equals(typeof(float)) && float.IsInfinity((float)this.value)) {
            this.value = 0.0f;
        }
    }

    public override bool IsEnum() {
        return Util.GetTypeByName(szValueTypeName).IsEnum;
    }

    public override bool IsAsset() {
        return Util.IsAsset(szValueTypeName);
    }
}

*/