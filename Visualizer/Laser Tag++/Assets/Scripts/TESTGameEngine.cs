using System;
using System.Text;
using System.Collections;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;

public class GameEngine : MonoBehaviour {

    // MQTT Client and Configuration
    private MqttClient client;
    [SerializeField] private string brokerAddress = "172.26.191.19";
    [SerializeField] private int brokerPort = 1883;
    [SerializeField] private string username = "username";
    [SerializeField] private string password = "bryan12345";
    [SerializeField] private string actionTopic = "visualiser_1/action"; // TODO: change depending on if current player is P1 or P2
    [SerializeField] private string shootTopic = "visualiser_1/shoot"; // TODO: change depending on if current player is P1 or P2
    [SerializeField] private string gameStatsTopic = "gamestats";
    [SerializeField] private string rainBombCollisionTopic = "visualiser/rainbombcollision";


    // Serialized Fields
    [Header("Players")]
    // TODO: rename to player and opponent
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("Game UI")]
    public GameUI gameUI;

    [Header("Visualization")]
    [SerializeField] private AREffects aREffects;
    [SerializeField] private OpponentDetection opponentDetection;

    [SerializeField] private GameObject rainBomb;
    [SerializeField] private GameObject basketball;
    [SerializeField] private GameObject soccerBall;
    [SerializeField] private GameObject volleyball;
    [SerializeField] private GameObject bowlingBall;


    // Constants
    private const float BASKETBALL_TIME = 0.5f;
    private const float SOCCER_BALL_TIME = 0.3f;
    private const float VOLLEYBALL_TIME = 0.8f;
    private const float BOWLING_BALL_TIME = 0.1f;
    private const float RAIN_BOMB_TIME = 0.5f;
    private const float RAIN_BOMB_DELAY = 1.5f;

    private const string SHOOT = "gun";
    private const string BASKETBALL = "basket";
    private const string SOCCER = "soccer";
    private const string VOLLEYBALL = "volley";
    private const string BOWLING = "bowl";
    private const string RAIN_BOMB = "bomb";
    private const string RELOAD = "reload";
    private const string SHIELD = "shield";


    // Queue to ensure that actions are displayed accordingly, one after another
    private readonly Queue<Action> executionQueue = new Queue<Action>();


    void Start()
    {
        SetupMqttClient();
    }


    private void SetupMqttClient()
    {
        try
        {
            // Initialize the MQTT client
            // client = new MqttClient(brokerAddress, brokerPort, false, null);
            client = new MqttClient(brokerAddress, brokerPort, false, null, null, MqttSslProtocols.None);

            // Register to message received event
            client.MqttMsgPublishReceived += OnMqttMessageReceived;

            // Connect to the broker with credentials
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, username, password);

            if (client.IsConnected)
            {
                Debug.Log("Connected to MQTT broker successfully.");
                // Subscribe to topics with QoS level 1
                client.Subscribe(new string[] { actionTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {actionTopic}");
                client.Subscribe(new string[] { shootTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {shootTopic}");
                client.Subscribe(new string[] { gameStatsTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {gameStatsTopic}");
                client.Subscribe(new string[] { rainBombCollisionTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {rainBombCollisionTopic}");
                
            }
            else
            {
                Debug.LogError("Failed to connect to MQTT broker.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"MQTT Connection Error: {e.Message}");
        }
    }

    /// <summary>
    /// Callback method when a message is received from the MQTT broker.
    /// </summary>
    private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Message);
        Debug.Log($"Received MQTT message: {message}");

        lock (executionQueue)
        {
            executionQueue.Enqueue(() => HandleMqttMessage(message));
        }
    }

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                var action = executionQueue.Dequeue();
                action.Invoke();
            }
        }

        // TODO reconnection logic!!!!!!
    }


    /// <summary>
    /// Parses and handles the MQTT message to trigger game actions.
    /// </summary>
    private void HandleMqttMessage(string message)
    {
        switch (message)
        {
            // ----- Shoot Topic -----
            case "gun":
                PlayerShoot();
                break;
            case "is_shot":
                // TODO: shot hit
                break;
            // ----- Action Topic -----
            case "basket":
                PlayerBasketball();
                break;
            case "soccer":
                PlayerSoccer();
                break;
            case "volley":
                PlayerVolleyball();
                break;
            case "bowl":
                PlayerBowling();
                break;
            case "bomb":
                PlayerThrowRainBomb();
                break;
            case "shield":
                PlayerShield();
                break;
            case "reload":
                // TODO: player reload
                break;
            // ----- RainBombCollisionTopic -----
            case "collision":
                // TODO: player collides with rain bomb
                break;
            // ----- GameStats Topic (opponent's action) -----
            default:
                // TODO: opponent's actions
                break;
        }
    }

    /// <summary>
    /// Ensures the MQTT client disconnects properly when the application quits.
    /// </summary>
    void OnApplicationQuit()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
            Debug.Log("Disconnected from MQTT broker.");
        }
    }


    /// <summary>
    /// Forms JSON data of game stats
    /// </summary>
    private string GetGameStats(string action) {
        // TODO: update based on 2 player logic
        string json = $@"
        {{
            ""player_id"": ""1"",
            ""action"": ""{action}"",
            ""game_state"": {{
                ""p1"": {{
                    ""hp"": {player1.HP},
                    ""bullets"": {player1.Ammo},
                    ""bombs"": {player1.RainBombCount},
                    ""shield_hp"": {player1.ShieldHP},
                    ""deaths"": {player2.Score},
                    ""shields"": {player1.ShieldCount}
                }},
                ""p2"": {{
                    ""hp"": {player2.HP},
                    ""bullets"": {player2.Ammo},
                    ""bombs"": {player2.RainBombCount},
                    ""shield_hp"": {player2.ShieldHP},
                    ""deaths"": {player1.Score},
                    ""shields"": {player2.ShieldCount}
                }}
            }}
        }}";

        return json;
    }


    // -------------------- Existing Game Logic Methods --------------------

    // ---------- Shoot ----------
    public void PlayerShoot() {
        // for eval assume all shoots are hits
        if (player1.Ammo > 0) {
            Player2TakeDamage(5);
            player1.Ammo--;

            gameUI.UpdateAmmoCount();

            string gameStats = GetGameStats(SHOOT);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            
            aREffects.SpawnOpponentBulletHitEffect();
        }
    }

    public void PlayerShootHit() {
        // TODO: "isShot" case
    }

    public void OpponentShoot() {
        Player1TakeDamage(5);
        player2.Ammo--;

        aREffects.SpawnPlayerHitEffect();
    }

    // ---------- Actions ----------
    public void PlayerBasketball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            string gameStats = GetGameStats(BASKETBALL);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(basketball, BASKETBALL_TIME);
    }

    public void PlayerSoccer() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            string gameStats = GetGameStats(SOCCER);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(soccerBall, SOCCER_BALL_TIME);
    }

    public void PlayerVolleyball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            string gameStats = GetGameStats(VOLLEYBALL);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(volleyball, VOLLEYBALL_TIME);
    }

    public void PlayerBowling() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            string gameStats = GetGameStats(BOWLING);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(bowlingBall, BOWLING_BALL_TIME);
    }

    public void PlayerThrowRainBomb() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        if (player1.RainBombCount > 0) {
            if (opponentTransform != null) {
                Player2TakeDamage(5);
            }

            player1.RainBombCount--;
            gameUI.UpdateRainBombCount();

            string gameStats = GetGameStats(RAIN_BOMB);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            
            aREffects.Throw(rainBomb, RAIN_BOMB_TIME);
            StartCoroutine(aREffects.SpawnRainCloud(RAIN_BOMB_DELAY));
        }
    }

    public void OpponentSportsAction() {
        Player1TakeDamage(10);

        aREffects.SpawnPlayerHitEffect();
    }

    public void OpponentThrowRainBomb() {
        if (player2.RainBombCount > 0) {
            Player1TakeDamage(5);

            player2.RainBombCount--;
            aREffects.SpawnPlayerHitEffect();
        }
    }

    public void PlayerRainEffect() {
        // TODO
    }

    public void OpponentRainEffect() {
        Player2TakeDamage(5);

        aREffects.SpawnRainEffect();
    }


    // ---------- Taking Damage ----------
    // TODO: update for 2 player logic
    public void Player1TakeDamage(int damage) {
        if (player1.ShieldHP > 0) {
            player1.ShieldHP -= damage;
            gameUI.UpdatePlayer1ShieldBar();
            if (player1.ShieldHP == 0) {
                aREffects.RemovePlayerShield();
            }
        } else {
            player1.HP -= damage;
            gameUI.UpdatePlayer1HPBar();
        }

        if (player1.HP == 0) {
            player2.Score++;
            gameUI.UpdatePlayer2Score();
            player1.HP = 100;
            gameUI.UpdatePlayer1HPBar();
        }
    }

    public void Player2TakeDamage(int damage) {
        if (player2.ShieldHP > 0) {
            player2.ShieldHP -= damage;
            gameUI.UpdatePlayer2ShieldBar();
            if (player2.ShieldHP == 0) {
                aREffects.RemoveOpponentShield();
            }
        } else {
            player2.HP -= damage;
            gameUI.UpdatePlayer2HPBar();
        }

        if (player2.HP == 0) {
            player1.Score++;
            gameUI.UpdatePlayer1Score();
            player2.HP = 100;
            gameUI.UpdatePlayer2HPBar();
        }
    }


    // ---------- Shield ----------
    public void PlayerShield() {
        if (player1.ShieldCount > 0 && player1.ShieldHP == 0) {
            player1.ShieldHP = 30;
            player1.ShieldCount--;
            gameUI.UpdatePlayer1ShieldBar();
            gameUI.UpdatePlayer1ShieldCount();

            aREffects.ShowPlayerShield();

            string gameStats = GetGameStats(SHIELD);
            client.Publish(gameStatsTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }
    }

    public void OpponentShield() {
        if (player2.ShieldCount > 0 && player2.ShieldHP == 0) {
            player2.ShieldHP = 30;
            player2.ShieldCount--;
            gameUI.UpdatePlayer2ShieldBar();
            gameUI.UpdatePlayer2ShieldCount();

            aREffects.ShowOpponentShield();
        }
    }

    // ---------- Reload ----------
    // TODO: move from Player class to here

}
