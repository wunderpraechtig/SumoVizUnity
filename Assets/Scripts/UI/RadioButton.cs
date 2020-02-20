using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadioButton : MonoBehaviour
{
    [System.Serializable]
    public class RadioEvent : UnityEvent { }

    [System.Serializable]
    public class EventConnection
    {
        public Toggle radio;
        public RadioEvent radioEvent;
    }

    [SerializeField] private List<EventConnection> radioButtons = null;

    private void Awake()
    {
        for(int i = 0; i < radioButtons.Count; i++) {
            int curr = i;
            radioButtons[i].radio.onValueChanged.AddListener((bool v) => OnRadioPressed(curr));
        }
    }

    private void OnRadioPressed(int radioNum) {
        for (int i = 0; i < radioButtons.Count; i++)
        {
            if (i == radioNum)
            {
                if(! radioButtons[i].radio.isOn)
                    radioButtons[i].radio.SetIsOnWithoutNotify(true);
                radioButtons[i].radioEvent.Invoke();
            }
            else {
                radioButtons[i].radio.SetIsOnWithoutNotify(false);
            }
        }
    }

}
