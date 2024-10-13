using System;
using System.Text;
using System.Collections;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using SimpleJSON;

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
            if (topic == actionTopic){
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

    private void UpdatePlayerStats(PlayerStats stats)
    {
        player.HP = stats.hp;
        player.Ammo = stats.bullets;
        player.RainBombCount = stats.bombs;
        player.ShieldHP = stats.shield_hp;
        player.ShieldCount = stats.shields;
        // player.Score is updated in HandleMqttUnity
    }

    private void UpdateOpponentStats(PlayerStats stats)
    {
        opponent.HP = stats.hp;
        opponent.Ammo = stats.bullets;
        opponent.RainBombCount = stats.bombs;
        opponent.ShieldHP = stats.shield_hp;
        opponent.ShieldCount = stats.shields;
        // opponent.Score is updated in HandleMqttUnity
    }

    private void UpdateUI()
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


    private void HandleMqttUnity(string message)
    {
        try
        {
            // Deserialize the JSON message into a GameStatsMessage object
            var gameStatsMessage = JsonUtility.FromJson<GameStatsMessage>(message);

            // Determine if the current player is p1 or p2 based on player_id
            bool isPlayerP1 = (gameStatsMessage.player_id == "1");

            // Update the stats accordingly
            if (isPlayerP1)
            {
                // Player is p1
                UpdatePlayerStats(gameStatsMessage.game_state.p1);
                UpdateOpponentStats(gameStatsMessage.game_state.p2);

                // Update scores (assuming deaths represent the opponent's score)
                player.Score = gameStatsMessage.game_state.p2.deaths;
                opponent.Score = gameStatsMessage.game_state.p1.deaths;
            }
            else
            {
                // Player is p2
                UpdatePlayerStats(gameStatsMessage.game_state.p2);
                UpdateOpponentStats(gameStatsMessage.game_state.p1);

                // Update scores
                player.Score = gameStatsMessage.game_state.p1.deaths;
                opponent.Score = gameStatsMessage.game_state.p2.deaths;
            }

            // Update the UI accordingly
            UpdateUI();

            Debug.Log("Game stats updated from unity message.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in HandleMqttUnity: {ex.Message}");
        }
    }


    private void HandleMqttEvalGroundTruth(string message)
    {
        try
        {
            var json = JSON.Parse(message);

            // Process p1 stats
            var p1 = json["p1"];
            player.HP = p1["hp"].AsInt;
            player.Ammo = p1["bullets"].AsInt;
            player.RainBombCount = p1["bombs"].AsInt;
            player.ShieldHP = p1["shield_hp"].AsInt;
            player.Score = p1["deaths"].AsInt;
            player.ShieldCount = p1["shields"].AsInt;

            // Process p2 stats
            var p2 = json["p2"];
            opponent.HP = p2["hp"].AsInt;
            opponent.Ammo = p2["bullets"].AsInt;
            opponent.RainBombCount = p2["bombs"].AsInt;
            opponent.ShieldHP = p2["shield_hp"].AsInt;
            opponent.Score = p2["deaths"].AsInt;
            opponent.ShieldCount = p2["shields"].AsInt;

            // Update the UI accordingly
            UpdateUI();

            Debug.Log("Game stats updated from ground truth message.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in HandleMqttEvalGroundTruth: {ex.Message}");
        }
    }



    // private void HandleMqttEvalGroundTruth(string message)
    // {
    //     try
    //     {
    //         // Deserialize the JSON message into a dictionary
    //         var gameStats = JsonUtility.FromJson<GameStatsWrapper>(message);
            

    //         // Update player (p1) stats
    //         // UpdatePlayerStats();
    //         player.HP = gameStats.p1.hp;
    //         player.Ammo = gameStats.p1.bullets;
    //         player.RainBombCount = gameStats.p1.bombs;
    //         player.ShieldHP = gameStats.p1.shield_hp;
    //         player.Score = gameStats.p1.deaths; // Using deaths as the score for now
    //         player.ShieldCount = gameStats.p1.shields;

    //         // Update opponent (p2) stats
    //         // UpdateOpponentStats();
    //         opponent.HP = gameStats.p2.hp;
    //         opponent.Ammo = gameStats.p2.bullets;
    //         opponent.RainBombCount = gameStats.p2.bombs;
    //         opponent.ShieldHP = gameStats.p2.shield_hp;
    //         opponent.Score = gameStats.p2.deaths; // Using deaths as the score for now
    //         opponent.ShieldCount = gameStats.p2.shields;

    //         // Update the UI accordingly
    //         // UpdateUI();
    //         gameUI.UpdatePlayerHPBar();
    //         gameUI.UpdatePlayerShieldBar();
    //         gameUI.UpdateAmmoCount();
    //         gameUI.UpdateRainBombCount();
    //         gameUI.UpdatePlayerShieldCount();

    //         gameUI.UpdateOpponentHPBar();
    //         gameUI.UpdateOpponentShieldBar();
    //         gameUI.UpdateOpponentShieldCount();

    //         Debug.Log("Game stats updated from ground truth message.");
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.LogError($"Error in HandleMqttMessageGroundTruth: {ex.Message}");
    //     }
    // }


    /// <summary>
    /// Parses and handles the MQTT message to trigger game actions.
    /// </summary>
    private void HandleMqttMessageAction(string message)
    {
        switch (message)
        {
            // ----- Shoot Topic -----
            case "null":
                break;
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
                PlayerReload();
                break;
            // ----- RainBombCollisionTopic -----
            case "collision":
                // TODO: player collides with rain bomb
                break;
            // ----- GameStats Topic -----
            default:
                // ----- GameStats/Unity Topic (opponent's actions) -----
                // ----- GameStats/Eval_Server (updated stats from eval server) -----
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


    // -------------------- Existing Game Logic Methods --------------------

    // ---------- Shoot ----------
    public void PlayerShoot() {
        // for eval assume all shoots are hits
        if (player.Ammo > 0) {
            Player2TakeDamage(5);
            player.Ammo--;

            gameUI.UpdateAmmoCount();

            string gameStats = GetGameStats(SHOOT);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            
            aREffects.SpawnOpponentBulletHitEffect();
        }
    }

    public void PlayerShootHit() {
        // TODO: "isShot" case
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

            string gameStats = GetGameStats(BASKETBALL);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(basketball, BASKETBALL_TIME);
    }

    public void PlayerSoccer() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            string gameStats = GetGameStats(SOCCER);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(soccerBall, SOCCER_BALL_TIME);
    }

    public void PlayerVolleyball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

            string gameStats = GetGameStats(VOLLEYBALL);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        aREffects.Throw(volleyball, VOLLEYBALL_TIME);
    }

    public void PlayerBowling() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();
        
        if (opponentTransform != null) {
            Player2TakeDamage(10);

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

            string gameStats = GetGameStats(RAIN_BOMB);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            
            aREffects.Throw(rainBomb, RAIN_BOMB_TIME);
            StartCoroutine(aREffects.SpawnRainCloud(RAIN_BOMB_DELAY));
        }
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
        if (opponent.ShieldHP > 0) {
            opponent.ShieldHP -= damage;
            gameUI.UpdateOpponentShieldBar();
            if (opponent.ShieldHP == 0) {
                aREffects.RemoveOpponentShield();
            }
        } else {
            opponent.HP -= damage;
            gameUI.UpdateOpponentHPBar();
        }

        if (opponent.HP == 0) {
            player.Score++;
            gameUI.UpdatePlayerScore();
            opponent.HP = 100;
            gameUI.UpdateOpponentHPBar();
        }
    }


    // ---------- Shield ----------
    public void PlayerShield() {
        // if (player.ShieldCount > 0 && player.ShieldHP == 0) {
        if (player.ShieldCount > 0 && player.ShieldHP >= 0) {
            player.ShieldHP = 30;
            player.ShieldCount--;
            gameUI.UpdatePlayerShieldBar();
            gameUI.UpdatePlayerShieldCount();

            aREffects.ShowPlayerShield();

            string gameStats = GetGameStats(SHIELD);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }
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
        if (player.Ammo >= 0) {
            player.Ammo = 6;
            gameUI.UpdateAmmoCount();

            aREffects.ShowReloadAnimation();

            string gameStats = GetGameStats(RELOAD);
            client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }
        
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
