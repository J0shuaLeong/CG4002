using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class PlayerInitialization : MonoBehaviour {

    [SerializeField] private OpponentDetection opponentDetection;

    private Transform opponentTransform;

    private bool playerReady = false;
    private bool opponentReady = false;

    private int playerID;
    private int opponentID;


    // MQTT Client and Configuration
    private MqttClient client;
    // deen changed the ip address to use digital ocean instead of U96 
    [SerializeField] private string brokerAddress = "172.26.191.19";
    // [SerializeField] private string brokerAddress = "152.42.199.87";
    [SerializeField] private int brokerPort = 1883;
    [SerializeField] private string username = "username";
    [SerializeField] private string password = "bryan12345";
    [SerializeField] private string initializationTopic = "visualiser/initialization";
    private readonly Queue<Action> executionQueue = new Queue<Action>();


    private void SetupMqttClient() {
        try {
            // Initialize the MQTT client
            client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);

            // Register to message received event
            client.MqttMsgPublishReceived += OnMqttMessageReceived;

            // Connect to the broker with credentials
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, username, password);

            if (client.IsConnected) {
                Debug.Log("Connected to MQTT broker successfully.");
                // Subscribe to topics with QoS level 1
                client.Subscribe(new string[] { initializationTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {initializationTopic}");

            }
            else {
                Debug.LogError("Failed to connect to MQTT broker.");
            }
        }
        catch (Exception e) {
            Debug.LogError($"MQTT Connection Error: {e.Message}\nStack Trace: {e.StackTrace}");
        }
    }


    private void Start() {
        SetupMqttClient();

        playerID = PlayerPrefs.GetInt("SelectedPlayerID", 1); // default: player 1
        opponentID = playerID == 1 ? 2 : 1;

        opponentTransform = opponentDetection.GetOpponentTransform();
    }


    private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e) {
        // Get the message payload
        string message = Encoding.UTF8.GetString(e.Message);

        // Get the topic the message was published to
        string topic = e.Topic;

        // Log the received message and topic
        Debug.Log($"Received MQTT message on topic {topic}: {message}");

        // Enqueue the message handling, passing both the topic and message
        lock (executionQueue) {
                if (topic == initializationTopic) {
                    executionQueue.Enqueue(() => HandleMqttInitialization(message));
                }
            }
        }


    private void Update() {
        lock (executionQueue) {
            while (executionQueue.Count > 0) {
                var action = executionQueue.Dequeue();
                action.Invoke();
            }
        }

        opponentTransform = opponentDetection.GetOpponentTransform();

        if (opponentTransform != null) {
            playerReady = true;
            string json = $@"
            {{
                ""player_id"": ""{playerID}"",
                ""action"": ""ready""
            }}";

            client.Publish(initializationTopic, System.Text.Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        } else {
            playerReady = false;
            string json = $@"
            {{
                ""player_id"": ""{playerID}"",
                ""action"": ""not ready""
            }}";

            client.Publish(initializationTopic, System.Text.Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        if (playerReady && opponentReady) {
            SceneManager.LoadScene("Main");
        }
    }

    void OnApplicationQuit() {
        if (client != null && client.IsConnected) {
            client.Disconnect();
            Debug.Log("Disconnected from MQTT broker.");
        }
    }

    private void HandleMqttInitialization(string message) {
        var json = JSON.Parse(message);

        if (json["player_id"] == opponentID) {
            if (json["action"] = "ready") {
                opponentReady = true;
            } else {
                opponentReady = false;
            }
        }
    }

}