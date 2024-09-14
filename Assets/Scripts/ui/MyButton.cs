using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
        but.onClick.AddListener(()=>AudioManager.instance.PlaySFX("click"));
        

        EventTrigger evtrig=gameObject.AddComponent<EventTrigger>();

        //Create a new event to add to the trigger
        EventTrigger.TriggerEvent trigev=new EventTrigger.TriggerEvent();
        trigev.AddListener((eventData) => AudioManager.instance.PlaySFX("hover"));

        EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigev, eventID = EventTriggerType.PointerEnter };

        if (evtrig.triggers==null) evtrig.triggers=new List<EventTrigger.Entry>();
        evtrig.triggers.Add(entry);
    }
}
