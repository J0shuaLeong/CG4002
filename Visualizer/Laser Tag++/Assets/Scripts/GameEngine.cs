using System;
using System.Text;
using System.Collections;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.InputSystem;

public class GameEngine : MonoBehaviour {

    // MQTT Client and Configuration
    private MqttClient client;
    [SerializeField] private string brokerAddress = "172.26.191.19";
    [SerializeField] private int brokerPort = 1883;
    [SerializeField] private string username = "username";
    [SerializeField] private string password = "bryan12345";
    [SerializeField] private string actionTopic = "visualiser_1/action"; // TODO: change depending on if current player is P1 or P2
    [SerializeField] private string shootTopic = "visualiser_1/shoot"; // TODO: change depending on if current player is P1 or P2
    [SerializeField] private string gameStatsUnityTopic = "gamestats/unity";
    [SerializeField] private string gameStatsEvalServerTopic = "gamestats/eval_server";
    [SerializeField] private string rainBombCollisionTopic = "visualiser/rain_bomb_collision";


    // Serialized Fields
    [Header("Players")]
    [SerializeField] private Player player;
    [SerializeField] private Player opponent;

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
    private const string LOGOUT = "logout";


    // Queue to ensure that actions are displayed accordingly, one after another
    private readonly Queue<Action> executionQueue = new Queue<Action>();


    // Variables
    private bool firstRainBombFlag;
    private bool secondRainBombFlag;


    void Start()
    {
        SetupMqttClient();

        firstRainBombFlag = false;
        secondRainBombFlag = false;
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
                client.Subscribe(new string[] { gameStatsUnityTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {gameStatsUnityTopic}");
                client.Subscribe(new string[] { gameStatsEvalServerTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                Debug.Log($"Subscribed to topic: {gameStatsEvalServerTopic}");
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
        // Get the message payload
        string message = Encoding.UTF8.GetString(e.Message);
        
        // Get the topic the message was published to
        string topic = e.Topic;
        
        // Log the received message and topic
        Debug.Log($"Received MQTT message on topic {topic}: {message}");
        
        // Enqueue the message handling, passing both the topic and message
        lock (executionQueue)
        {
            if (topic == actionTopic || topic == shootTopic || topic == rainBombCollisionTopic){
                executionQueue.Enqueue(() => HandleMqttMessageAction(message));
            }
            else if (topic == gameStatsEvalServerTopic){
                executionQueue.Enqueue(() => HandleMqttEvalGroundTruth(message));
            }     
            else if (topic == gameStatsUnityTopic){
                executionQueue.Enqueue(() => HandleMqttUnity(message));
            }
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


    // -------------------- Handling MQTT Messages --------------------

    // Topics Handled: gamestats/unity
    private void HandleMqttUnity(string message) {
        // TODO: handle opponent action
    }

    // Topics Handled: visualiser_1/shoot, visualiser_1/action, visualiser/rain_bomb_collision
    private void HandleMqttMessageAction(string message)
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
                    // for evaluation
                    if (firstRainBombFlag == false) {
                        firstRainBombFlag = true;
                    } else if (secondRainBombFlag == false) {
                        secondRainBombFlag = true;
                    }
                    break;
                case "shield":
                    PlayerShield();
                    break;
                case "reload":
                    PlayerReload();
                    break;
                case "logout":
                    PlayerLogOut();
                    break;
                // ----- RainBombCollisionTopic -----
                case "collision":
                    // TODO: player collides with rain bomb
                    break;
                default:
                    break;
            }
    }

    // Topics Handled: gamestats/eval_server
    private void HandleMqttEvalGroundTruth(string message)
    // TODO: change depending on if current player is P1 or P2
    {
        try
        {
            var json = JSON.Parse(message);

            // Process p1 stats
            var p1 = json["p1"];
            var p2 = json["p2"];

            player.HP = p1["hp"].AsInt;
            player.Ammo = p1["bullets"].AsInt;
            player.RainBombCount = p1["bombs"].AsInt;
            player.ShieldHP = p1["shield_hp"].AsInt;
            player.Score = p2["deaths"].AsInt;
            player.ShieldCount = p1["shields"].AsInt;

            // Process p2 stats
            opponent.HP = p2["hp"].AsInt;
            opponent.Ammo = p2["bullets"].AsInt;
            opponent.RainBombCount = p2["bombs"].AsInt;
            opponent.ShieldHP = p2["shield_hp"].AsInt;
            opponent.Score = p1["deaths"].AsInt;
            opponent.ShieldCount = p2["shields"].AsInt;

            // Update the UI accordingly
            UpdateAllUI();

            Debug.Log("Game stats updated from ground truth message.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in HandleMqttEvalGroundTruth: {ex.Message}");
        }
    }


    // -------------------- Helper Functions --------------------

    private string GetGameStats(string action) {
        // TODO: change depending on if current player is P1 or P2
        string json = $@"
        {{
            ""player_id"": ""1"",
            ""action"": ""{action}"",
            ""game_state"": {{
                ""p1"": {{
                    ""hp"": {player.HP},
                    ""bullets"": {player.Ammo},
                    ""bombs"": {player.RainBombCount},
                    ""shield_hp"": {player.ShieldHP},
                    ""deaths"": {opponent.Score},
                    ""shields"": {player.ShieldCount}
                }},
                ""p2"": {{
                    ""hp"": {opponent.HP},
                    ""bullets"": {opponent.Ammo},
                    ""bombs"": {opponent.RainBombCount},
                    ""shield_hp"": {opponent.ShieldHP},
                    ""deaths"": {player.Score},
                    ""shields"": {opponent.ShieldCount}
                }}
            }}
        }}";

        return json;
    }

    private void UpdateAllPlayerStats(PlayerStats stats)
    {
        player.HP = stats.hp;
        player.Ammo = stats.bullets;
        player.RainBombCount = stats.bombs;
        player.ShieldHP = stats.shield_hp;
        player.ShieldCount = stats.shields;
    }

    private void UpdateAllOpponentStats(PlayerStats stats)
    {
        opponent.HP = stats.hp;
        opponent.Ammo = stats.bullets;
        opponent.RainBombCount = stats.bombs;
        opponent.ShieldHP = stats.shield_hp;
        opponent.ShieldCount = stats.shields;
    }

    private void UpdateAllUI()
    {                   
        // Update Player UI
        gameUI.UpdatePlayerHPBar();
        gameUI.UpdatePlayerShieldBar();
        gameUI.UpdateAmmoCount();
        gameUI.UpdateRainBombCount();
        gameUI.UpdatePlayerShieldCount();
        gameUI.UpdatePlayerScore();

        // Update Opponent UI
        gameUI.UpdateOpponentHPBar();
        gameUI.UpdateOpponentShieldBar();
        gameUI.UpdateOpponentShieldCount();
        gameUI.UpdateOpponentScore();
    }


    // -------------------- Existing Game Logic Methods --------------------

    // ---------- Shoot ----------
    public void PlayerShoot() {
        // TODO: after 1 player eval, edit to -bullets only, damage taken only if "is_shot" received
        if (player.Ammo > 0) {
            Player2TakeDamage(5);
            player.Ammo--;

            gameUI.UpdateAmmoCount();
            
            aREffects.SpawnOpponentBulletHitEffect();
        }

        if (secondRainBombFlag == true) {
            Player2TakeDamage(10);
        } else if (firstRainBombFlag == true) {
            Player2TakeDamage(5);
        }

        string gameStats = GetGameStats(SHOOT);
        client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

    public void PlayerShootHit() {
        // TODO: "is_shot" case
    }

    public void OpponentShoot() {
        Player1TakeDamage(5);
        opponent.Ammo--;

        aREffects.SpawnPlayerHitEffect();
    }


    // ---------- Sports Actions ----------
    public void PlayerBasketball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            if (secondRainBombFlag == true) {
                Player2TakeDamage(10);
            } else if (firstRainBombFlag == true) {
                Player2TakeDamage(5);
            }

            string gameStats = GetGameStats(BASKETBALL);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(basketball, BASKETBALL_TIME);
    }

    public void PlayerSoccer() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            if (secondRainBombFlag == true) {
                Player2TakeDamage(10);
            } else if (firstRainBombFlag == true) {
                Player2TakeDamage(5);
            }

            string gameStats = GetGameStats(SOCCER);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(soccerBall, SOCCER_BALL_TIME);
    }

    public void PlayerVolleyball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            if (secondRainBombFlag == true) {
                Player2TakeDamage(10);
            } else if (firstRainBombFlag == true) {
                Player2TakeDamage(5);
            }

            string gameStats = GetGameStats(VOLLEYBALL);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(volleyball, VOLLEYBALL_TIME);
    }

    public void PlayerBowling() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            if (secondRainBombFlag == true) {
                Player2TakeDamage(10);
            } else if (firstRainBombFlag == true) {
                Player2TakeDamage(5);
            }

            string gameStats = GetGameStats(BOWLING);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(bowlingBall, BOWLING_BALL_TIME);
    }

    public void OpponentSportsAction() {
        Player1TakeDamage(10);

        aREffects.SpawnPlayerHitEffect();
    }


    // ---------- Rain Bomb ----------
    public void PlayerThrowRainBomb() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        if (player.RainBombCount > 0) {
            if (opponentTransform != null) {
                Player2TakeDamage(5);
            }

            player.RainBombCount--;
            gameUI.UpdateRainBombCount();
            
            aREffects.Throw(rainBomb, RAIN_BOMB_TIME);
            StartCoroutine(aREffects.SpawnRainCloud(RAIN_BOMB_DELAY));
        }

        if (secondRainBombFlag == true) {
            Player2TakeDamage(10);
        } else if (firstRainBombFlag == true) {
            Player2TakeDamage(5);
        }

        string gameStats = GetGameStats(RAIN_BOMB);
        client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

    public void OpponentThrowRainBomb() {
        if (opponent.RainBombCount > 0) {
            Player1TakeDamage(5);

            opponent.RainBombCount--;
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
        if (player.ShieldHP > 0) {
            player.ShieldHP -= damage;
            gameUI.UpdatePlayerShieldBar();
            if (player.ShieldHP == 0) {
                aREffects.RemovePlayerShield();
            }
        } else {
            player.HP -= damage;
            gameUI.UpdatePlayerHPBar();
        }

        if (player.HP == 0) {
            opponent.Score++;
            gameUI.UpdateOpponentScore();
            player.HP = 100;
            gameUI.UpdatePlayerHPBar();
        }
    }

    public void Player2TakeDamage(int damage) {
        int excess = 0;

        if (opponent.ShieldHP > 0) {
            opponent.ShieldHP -= damage;
            gameUI.UpdateOpponentShieldBar();
            if (opponent.ShieldHP == 0) {
                aREffects.RemoveOpponentShield();
            }
        } else {
            if (damage > opponent.HP) {
                excess = damage - opponent.HP;
            }
            opponent.HP -= damage;
            gameUI.UpdateOpponentHPBar();
        }

        if (opponent.HP <= 0) {
            player.Score++;
            gameUI.UpdatePlayerScore();
            opponent.HP = 100 - excess;
            gameUI.UpdateOpponentHPBar();
        }
    }


    // ---------- Shield ----------
    public void PlayerShield() {
        if (player.ShieldCount > 0 && player.ShieldHP == 0) {
            player.ShieldHP = 30;
            player.ShieldCount--;
            gameUI.UpdatePlayerShieldBar();
            gameUI.UpdatePlayerShieldCount();

            aREffects.ShowPlayerShield();
        }

        if (secondRainBombFlag == true) {
            Player2TakeDamage(10);
        } else if (firstRainBombFlag == true) {
            Player2TakeDamage(5);
        }

        string gameStats = GetGameStats(SHIELD);
        client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

    public void OpponentShield() {
        if (opponent.ShieldCount > 0 && opponent.ShieldHP == 0) {
            opponent.ShieldHP = 30;
            opponent.ShieldCount--;
            gameUI.UpdateOpponentShieldBar();
            gameUI.UpdateOpponentShieldCount();

            aREffects.ShowOpponentShield();
        }
    }


    // ---------- Reload ----------
    public void PlayerReload() {
        if (player.Ammo == 0) {
            player.Ammo = 6;
            gameUI.UpdateAmmoCount();

            aREffects.ShowReloadAnimation();
        }

        if (secondRainBombFlag == true) {
            Player2TakeDamage(10);
        } else if (firstRainBombFlag == true) {
            Player2TakeDamage(5);
        }

        string gameStats = GetGameStats(RELOAD);
        client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }


    // ---------- Log Out ----------
    public void PlayerLogOut() {
        // TODO: show quit game page

        if (secondRainBombFlag == true) {
            Player2TakeDamage(10);
        } else if (firstRainBombFlag == true) {
            Player2TakeDamage(5);
        }

        string gameStats = GetGameStats(LOGOUT);
        client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

}


// Classes for deserializing JSON
[Serializable]
public class PlayerStats
{
    public int hp;
    public int bullets;
    public int bombs;
    public int shield_hp;
    public int deaths;
    public int shields;
}

[Serializable]
public class GameStatsWrapper
{
    public PlayerStats p1;
    public PlayerStats p2;
}



[Serializable]
public class GameStatsMessage
{
    public string player_id;
    public string action;
    public GameState game_state;
}

[Serializable]
public class GameState
{
    public PlayerStats p1;
    public PlayerStats p2;
}