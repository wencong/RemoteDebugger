using UnityEngine;
using System;
using System.Collections;

public class ComponentToCompNode {
    public CompNode compInfoToCompNode(Component comp) {
        CompNode compNode = new CompNode();
        compNode.instance_id = comp.GetInstanceID();
        compNode.name = comp.GetType().ToString();
        compNode.contain_enable = comp.ContainProperty("enabled");
        if (comp.ContainProperty("enabled")) {
            compNode.enabled = comp.GetValue<bool>("enabled");
        }
        else compNode.enabled = false;
        return compNode;
    }
}