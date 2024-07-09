using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MyButton : MonoBehaviour
{

    [SerializeField] string actionName;

    Button but;

    void Start() {
        if (actionName=="") {
            actionName = "emptyAction";
        }

        but = GetComponent<Button>();

        but.onClick.AddListener(UIActions.getActionByName(actionName));
    }
}
