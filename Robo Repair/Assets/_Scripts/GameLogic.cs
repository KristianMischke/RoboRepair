using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameLogic : MonoBehaviour
{
    void Start()
    {
        AirConsole.instance.onReady += OnReady;
        AirConsole.instance.onMessage += OnMessage;
    }


    void OnReady(string code)
    {
        
    }

    void OnMessage(int from, JToken data)
    {
        Debug.Log("message: " + data);
        //AirConsole.instance.Message(from, "Full of pixels!");
    }

    void OnDestroy()
    {
        if (AirConsole.instance != null)
        {
            AirConsole.instance.onMessage -= OnMessage;
            AirConsole.instance.onReady -= OnReady;
            //AirConsole.instance.onConnect -= OnConnect;
        }
    }
}
