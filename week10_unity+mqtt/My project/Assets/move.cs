using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    public mqttReceiver _eventSender;

    void Start()
    {
        _eventSender.OnMessageArrived += OnMessageArrivedHandler;
    }


    private void OnMessageArrivedHandler(string newMsg)
    {

        Debug.Log("Event Fired. The message is = " + newMsg);
        if(newMsg=="L")
        {
            gameObject.transform.position = new Vector2 (transform.position.x - 2, transform.position.y);
        }
        else if(newMsg=="R")
        {
            gameObject.transform.position = new Vector2 (transform.position.x + 2, transform.position.y);
        }
        else if(newMsg=="U")
        {
            gameObject.transform.position = new Vector2 (transform.position.x, transform.position.y + 2);
        }
        else if(newMsg=="D")
        {
            gameObject.transform.position = new Vector2 (transform.position.x, transform.position.y - 2);
        }


        
    } 


}
