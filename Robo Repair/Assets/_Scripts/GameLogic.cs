using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class GameLogic : MonoBehaviour
{
    public static GameLogic instance;

    [SerializeField]
    public GameObject playerPrefab;

    [SerializeField]
    public Transform playerParentTransform;

    [SerializeField]
    public SoundGenerator soundGenerator;

    private Dictionary<int, CharacterController2D> players = new Dictionary<int, CharacterController2D>();

    void Start()
    {
        if (instance != null && instance != this)
            Debug.LogError("Singleton GameLogic should not be instantiated more than once!");
        instance = this;

        AirConsole.instance.onReady += OnReady;
        AirConsole.instance.onMessage += OnMessage;
        AirConsole.instance.onConnect += OnConnect;
    }

    void OnConnect(int device)
    {
        AddNewPlayer(device);
    }

    private void AddNewPlayer(int deviceID)
    {
        if (players.ContainsKey(deviceID))
            return;

        GameObject newPlayer = Instantiate(playerPrefab, playerParentTransform);
        newPlayer.name = "Player" + deviceID;
        players[deviceID] = newPlayer.GetComponent<CharacterController2D>();
        players[deviceID].playerID = deviceID;
        newPlayer.transform.position = new Vector3(Random.Range(-10, 10), Random.Range(-6, 6));
    }

    public CharacterController2D GetPlayerByID(int deviceID)
    {
        if (players.TryGetValue(deviceID, out CharacterController2D player))
        {
            return player;
        }
        return null;
    }

    void OnReady(string code)
    {
        
    }

    void OnMessage(int from, JToken data)
    {
        if (!players.ContainsKey(from))
        {
            Debug.LogWarning($"Recieved message from {from} but that player is no longer connected\n{data}");
            return;
        }

        Debug.Log("message (" + from + "): " + data);
        //AirConsole.instance.Message(from, "Full of pixels!");

        if (data.First is JProperty)
        {
            JProperty prop = data.First as JProperty;
            players[from].ControllerAction(prop.Name, prop.Value);
        }
    }

    void OnDestroy()
    {
        if (AirConsole.instance != null)
        {
            AirConsole.instance.onMessage -= OnMessage;
            AirConsole.instance.onReady -= OnReady;
            AirConsole.instance.onConnect -= OnConnect;
        }
    }
}
