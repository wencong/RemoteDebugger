using UnityEngine;
using UnityEditor;
using System.Collections;

public class confirmationWindow{

    public static void GameObjHandle(string propertyName) {

        ShowPanelDataSet.ms_gameObjHandleFlag = EditorUtility.DisplayDialogComplex("Change " + propertyName,
            "Do you want to change the " + propertyName + " for all the child",
            "Yes,change children", "No,this object only", "Cancel");

    }

}
