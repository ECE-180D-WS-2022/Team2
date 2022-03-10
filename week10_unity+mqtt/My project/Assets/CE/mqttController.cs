using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mqttController : MonoBehaviour
{
    public mqttReceiver _eventSender;

    void Start()
    {
        _eventSender.OnMessageArrived += OnMessageArrivedHandler;
    }

    private void OnMessageArrivedHandler(string newMsg)
    {

        Debug.Log("Event Fired. The message is = " + newMsg);
    }

}