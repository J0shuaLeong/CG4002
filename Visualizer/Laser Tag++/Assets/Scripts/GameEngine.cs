using System;
using System.Text;
using System.Collections;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameEngine : MonoBehaviour {

    // MQTT Client and Configuration
    private MqttClient client;
    // deen changed the ip address to use digital ocean instead of U96 
    [SerializeField] private string brokerAddress = "172.26.191.19";
    // [SerializeField] private string brokerAddress = "152.42.199.87";
    [SerializeField] private int brokerPort = 1883;
    [SerializeField] private string username = "username";
    [SerializeField] private string password = "bryan12345";
    [SerializeField] private string actionTopic;
    [SerializeField] private string shootTopic;
    [SerializeField] private string gameStatsUnityTopic = "gamestats/unity";
    [SerializeField] private string gameStatsEvalServerTopic = "gamestats/eval_server";
    [SerializeField] private string rainBombCollisionTopic = "visualiser/rainbombcollision";
    
    [SerializeField] private string gunTopic;
    [SerializeField] private string legTopic;
    [SerializeField] private string gloveTopic;


    // Serialized Fields
    [Header("Players")]
    [SerializeField] private Player player;
    [SerializeField] private Player opponent;

    [Header("Game UI")]
    [SerializeField] private GameUI gameUI;

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
    private int playerID;
    private int opponentID;


    // private bool hadAmmo = false; // for free play



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
                client.Subscribe(new string[] { actionTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {actionTopic}");
                client.Subscribe(new string[] { shootTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {shootTopic}");
                client.Subscribe(new string[] { gameStatsUnityTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {gameStatsUnityTopic}");
                client.Subscribe(new string[] { gameStatsEvalServerTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {gameStatsEvalServerTopic}");
                client.Subscribe(new string[] { rainBombCollisionTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {rainBombCollisionTopic}");
                client.Subscribe(new string[] { gunTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {gunTopic}");
                client.Subscribe(new string[] { legTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {legTopic}");
                client.Subscribe(new string[] { gloveTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Debug.Log($"Subscribed to topic: {gloveTopic}");

            }
            else {
                Debug.LogError("Failed to connect to MQTT broker.");
            }
        }
        catch (Exception e) {
            Debug.LogError($"MQTT Connection Error: {e.Message}\nStack Trace: {e.StackTrace}");
        }
    }


    void Start() {
        playerID = PlayerPrefs.GetInt("SelectedPlayerID", 1); // default: player 1
        opponentID = playerID == 1 ? 2 : 1;

        actionTopic = $"visualiser_{playerID}/action";
        shootTopic = $"visualiser_{playerID}/shoot";
        gunTopic = $"visualiser_{playerID}/device/GUN_P{playerID}";
        legTopic = $"visualiser_{playerID}/device/LEG_P{playerID}";
        gloveTopic = $"visualiser_{playerID}/device/GLOVE_P{playerID}";

        SetupMqttClient();
    }



    /// Callback method when a message is received from the MQTT broker
    private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e) {
        // Get the message payload
        string message = Encoding.UTF8.GetString(e.Message);

        // Get the topic the message was published to
        string topic = e.Topic;

        // Log the received message and topic
        Debug.Log($"Received MQTT message on topic {topic}: {message}");

        // Enqueue the message handling, passing both the topic and message
        lock (executionQueue) {
            if (topic == actionTopic || topic == shootTopic || topic == rainBombCollisionTopic) {
                executionQueue.Enqueue(() => HandleMqttMessageAction(message));
            }
            else if (topic == gameStatsEvalServerTopic) {
                executionQueue.Enqueue(() => HandleMqttEvalGroundTruth(message));
            }
            else if (topic == gameStatsUnityTopic) {
                executionQueue.Enqueue(() => HandleMqttUnity(message));
            }
            else if (topic == gunTopic) {
                executionQueue.Enqueue(() => HandleMqttGun(message));
            }
            else if (topic == legTopic) {
                executionQueue.Enqueue(() => HandleMqttLeg(message));
            }
            else if (topic == gloveTopic) {
                executionQueue.Enqueue(() => HandleMqttGlove(message));
            }
        }
    }

    void Update() {
        lock (executionQueue) {
            while (executionQueue.Count > 0) {
                var action = executionQueue.Dequeue();
                action.Invoke();
            }
        }

        // TODO reconnection logic!!!!!!
    }

    /// Ensures the MQTT client disconnects properly when the application quits
    void OnApplicationQuit() {
        if (client != null && client.IsConnected) {
            client.Disconnect();
            Debug.Log("Disconnected from MQTT broker.");
        }
    }


    // -------------------- Handling MQTT Messages --------------------

    // Topics Handled: visualiser_x/shoot, visualiser_x/action, visualiser/rain_bomb_collision
    private void HandleMqttMessageAction(string message) {
        switch (message) {
            case "null":
                StartCoroutine(gameUI.ShowNullActionPopup());
                break;
            // ----- Shoot Topic -----
            case "gun":
                PlayerShoot();
                break;
            case "is_shot":
                // PlayerShootHit(); // for free play
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
            case "logout":
                PlayerLogOut();
                break;
            // ----- RainBombCollisionTopic -----
            default:
                var json = JSON.Parse(message);

                Debug.Log("json player id: " + json["player_id"]);

                if (json["player_id"].AsInt == playerID) {
                    if (json["action"] == "enter") {
                        aREffects.ShowPlayerRainEffect();
                    } else {
                        aREffects.RemovePlayerRainEffect();
                    }
                }
                break;
        }
    }

    // Topics Handled: gamestats/eval_server
    private void HandleMqttEvalGroundTruth(string message) {
        try {
            var json = JSON.Parse(message);

            var playerStats = playerID == 1 ? json["p1"] : json["p2"];
            var opponentStats = opponentID == 1 ? json["p1"] : json["p2"];

            // Process player stats
            player.HP = playerStats["hp"].AsInt;
            player.Ammo = playerStats["bullets"].AsInt;
            player.RainBombCount = playerStats["bombs"].AsInt;
            player.ShieldHP = playerStats["shield_hp"].AsInt;
            player.Score = opponentStats["deaths"].AsInt;
            player.ShieldCount = playerStats["shields"].AsInt;

            // Process opponent stats
            opponent.HP = opponentStats["hp"].AsInt;
            opponent.Ammo = opponentStats["bullets"].AsInt;
            opponent.RainBombCount = opponentStats["bombs"].AsInt;
            opponent.ShieldHP = opponentStats["shield_hp"].AsInt;
            opponent.Score = playerStats["deaths"].AsInt;
            opponent.ShieldCount = opponentStats["shields"].AsInt;

            // Remove shield effects if needed
            RemoveShieldEffects();

            // Update the UI accordingly
            UpdateAllUI();

            Debug.Log("Game stats updated from ground truth message.");
        }
        catch (Exception ex) {
            Debug.LogError($"Error in HandleMqttEvalGroundTruth: {ex.Message}");
        }
    }

    // Topics Handled: gamestats/unity
    private void HandleMqttUnity(string message) {
        try {
            var json = JSON.Parse(message);

            if (json["player_id"].AsInt == opponentID) { // only process if it's the opponent's action
                var gameState = json["game_state"];
                var playerStats = playerID == 1 ? gameState["p1"] : gameState["p2"];
                var opponentStats = opponentID == 1 ? gameState["p1"] : gameState["p2"];

                string action = json["action"];

                switch (action) {
                    case "gun": // for 2 player eval
                    // case "is_shot": // for free play
                    case "basket":
                    case "soccer":
                    case "volley":
                    case "bowl":
                    case "bomb":
                        if (player.HP != playerStats["hp"].AsInt) {
                            OpponentHit();
                        }
                        break;
                    case "shield":
                        OpponentShield();
                        break;
                    default:
                        break;
                }

                if (playerStats["deaths"].AsInt > opponent.Score) {
                    StartCoroutine(gameUI.ShowDeathPopup());
                }

                // Process player stats
                player.HP = playerStats["hp"].AsInt;
                player.Ammo = playerStats["bullets"].AsInt;
                player.RainBombCount = playerStats["bombs"].AsInt;
                player.ShieldHP = playerStats["shield_hp"].AsInt;
                player.Score = opponentStats["deaths"].AsInt;
                player.ShieldCount = playerStats["shields"].AsInt;

                // Process opponent stats
                opponent.HP = opponentStats["hp"].AsInt;
                opponent.Ammo = opponentStats["bullets"].AsInt;
                opponent.RainBombCount = opponentStats["bombs"].AsInt;
                opponent.ShieldHP = opponentStats["shield_hp"].AsInt;
                opponent.Score = playerStats["deaths"].AsInt;
                opponent.ShieldCount = opponentStats["shields"].AsInt;

                // Remove shield effects if needed
                RemoveShieldEffects();

                // Update the UI accordingly
                UpdateAllUI();

                Debug.Log("Game stats updated from opponent's actions.");
            }
        }
        catch (Exception ex) {
            Debug.LogError($"Error in HandleMqttUnity: {ex.Message}");
        }
    }

    private void HandleMqttGun(string message) {
        if (message == "true") {
            gameUI.UpdateGunStatus(true);
        } else {
            gameUI.UpdateGunStatus(false);
        }
    }

    private void HandleMqttLeg(string message) {
        if (message == "true") {
            gameUI.UpdateLegStatus(true);
        } else {
            gameUI.UpdateLegStatus(false);
        }
    }

    private void HandleMqttGlove(string message) {
        if (message == "true") {
            gameUI.UpdateGloveStatus(true);
        } else {
            gameUI.UpdateGloveStatus(false);
        }
    }


    // -------------------- Helper Functions --------------------

    private string GetGameStats(string action) {
        // P1 Stats
        int p1HP = playerID == 1 ? player.HP : opponent.HP;
        int p1Bullets = playerID == 1 ? player.Ammo : opponent.Ammo;
        int p1Bombs = playerID == 1 ? player.RainBombCount : opponent.RainBombCount;
        int p1ShieldHP = playerID == 1 ? player.ShieldHP : opponent.ShieldHP;
        int p1Deaths = playerID == 1 ? opponent.Score : player.Score;
        int p1Shields = playerID == 1 ? player.ShieldCount : opponent.ShieldCount;

        // P2 Stats
        int p2HP = playerID == 2 ? player.HP : opponent.HP;
        int p2Bullets = playerID == 2 ? player.Ammo : opponent.Ammo;
        int p2Bombs = playerID == 2 ? player.RainBombCount : opponent.RainBombCount;
        int p2ShieldHP = playerID == 2 ? player.ShieldHP : opponent.ShieldHP;
        int p2Deaths = playerID == 2 ? opponent.Score : player.Score;
        int p2Shields = playerID == 2 ? player.ShieldCount : opponent.ShieldCount;

        string json = $@"
        {{
            ""player_id"": ""{playerID}"",
            ""action"": ""{action}"",
            ""game_state"": {{
                ""p1"": {{
                    ""hp"": {p1HP},
                    ""bullets"": {p1Bullets},
                    ""bombs"": {p1Bombs},
                    ""shield_hp"": {p1ShieldHP},
                    ""deaths"": {p1Deaths},
                    ""shields"": {p1Shields}
                }},
                ""p2"": {{
                    ""hp"": {p2HP},
                    ""bullets"": {p2Bullets},
                    ""bombs"": {p2Bombs},
                    ""shield_hp"": {p2ShieldHP},
                    ""deaths"": {p2Deaths},
                    ""shields"": {p2Shields}
                }}
            }}
        }}";

        return json;
    }

    private void UpdateAllUI() {
        gameUI.UpdatePlayerHPBar();
        gameUI.UpdatePlayerShieldBar();
        gameUI.UpdateAmmoCount();
        gameUI.UpdateRainBombCount();
        gameUI.UpdatePlayerShieldCount();
        gameUI.UpdatePlayerScore();

        gameUI.UpdateOpponentHPBar();
        gameUI.UpdateOpponentShieldBar();
        gameUI.UpdateOpponentShieldCount();
        gameUI.UpdateOpponentScore();
    }

    private void PublishMqttUnity(string action) {
        string gameStats = GetGameStats(action);
        client.Publish(gameStatsUnityTopic, System.Text.Encoding.UTF8.GetBytes(gameStats), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }


    // -------------------- Existing Game Logic Methods --------------------

    // ---------- Shoot ----------
    public void PlayerShoot() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform(); // for 2 player eval

        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (player.Ammo > 0) {
            // hadAmmo = true; // for free play

            player.Ammo--;

            gameUI.UpdateAmmoCount();

            // for 2 player eval
            if (opponentTransform != null) {
                OpponentTakeDamage(5);
                aREffects.ShowOpponentBulletHitEffect();
            }
        }

        PublishMqttUnity(SHOOT);
    }

    // for free play
    // public void PlayerShootHit() {
    //     if (hadAmmo) {
    //         OpponentTakeDamage(5);
            
    //         aREffects.ShowOpponentBulletHitEffect();

    //         hadAmmo = false;
    //     }
    // }


    // ---------- Sports Actions ----------
    public void PlayerBasketball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (opponentTransform != null) {
            OpponentTakeDamage(10);
        }

        PublishMqttUnity(BASKETBALL);

        aREffects.Throw(basketball, BASKETBALL_TIME);
    }

    public void PlayerSoccer() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (opponentTransform != null) {
            OpponentTakeDamage(10);
        }

        PublishMqttUnity(SOCCER);

        aREffects.Throw(soccerBall, SOCCER_BALL_TIME);
    }

    public void PlayerVolleyball() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (opponentTransform != null) {
            OpponentTakeDamage(10);
        }

        PublishMqttUnity(VOLLEYBALL);

        aREffects.Throw(volleyball, VOLLEYBALL_TIME);
    }

    public void PlayerBowling() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (opponentTransform != null) {
            OpponentTakeDamage(10);
        }

        PublishMqttUnity(BOWLING);

        aREffects.Throw(bowlingBall, BOWLING_BALL_TIME);
    }


    // ---------- Rain Bomb ----------
    public void PlayerThrowRainBomb() {
        Transform opponentTransform = opponentDetection.GetOpponentTransform();

        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (player.RainBombCount > 0) {
            aREffects.Throw(rainBomb, RAIN_BOMB_TIME);

            if (opponentTransform != null) {
                OpponentTakeDamage(5);
                StartCoroutine(aREffects.SpawnOpponentRainCloud(RAIN_BOMB_DELAY, opponentTransform));
            }

            player.RainBombCount--;
            gameUI.UpdateRainBombCount();
        }

        PublishMqttUnity(RAIN_BOMB);
    }

    public void OpponentRainBombCollision() {
        // OpponentTakeDamage(5); // for free play

        // PublishMqttUnity(RAIN_BOMB); // for free play
    }

    public void CheckForOpponentRainBombCollision() {
        RainBombCollision[] rainBombs = FindObjectsOfType<RainBombCollision>();

        foreach (RainBombCollision rainBomb in rainBombs) {
            if (rainBomb.CheckForRainBombCollision() == true) {
                OpponentTakeDamage(5);
            }
        }
    }

    public void PublishOpponentEnteredRainBomb() {
        string json = $@"
        {{
            ""player_id"": ""{opponentID}"",
            ""action"": ""enter""
        }}";

        client.Publish(rainBombCollisionTopic, System.Text.Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }

    public void PublishOpponentExitedRainBomb() {
        string json = $@"
        {{
            ""player_id"": ""{opponentID}"",
            ""action"": ""exit""
        }}";

        client.Publish(rainBombCollisionTopic, System.Text.Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    }


    // ---------- Taking Damage ----------
    public void OpponentTakeDamage(int damage) {
        int excess = 0;

        if (opponent.ShieldHP > 0) {
            opponent.ShieldHP -= damage;

            if (opponent.ShieldHP < 0) {
                excess = -opponent.ShieldHP;
                opponent.ShieldHP = 0;
            }
            
            gameUI.UpdateOpponentShieldBar();
            
            if (opponent.ShieldHP == 0) {
                aREffects.RemoveOpponentShield();
            }
        } else {
            excess = damage;
        }

        opponent.HP -= excess;

        if (opponent.HP < 0) {
            excess = -opponent.HP;
            opponent.HP = 100 - excess;
        }

        gameUI.UpdateOpponentHPBar();

        if (opponent.HP <= 0) {
            player.Score++;
            gameUI.UpdatePlayerScore();

            opponent.HP = 100;
            gameUI.UpdateOpponentHPBar();

            ResetOpponentStatsDuringRespawn();
            
            StartCoroutine(gameUI.ShowKillPopup());
        }
    }

    private void ResetOpponentStatsDuringRespawn() {
        opponent.Ammo = 6;
        opponent.ShieldCount = 3;
        opponent.RainBombCount = 2;
    }


    // ---------- Shield ----------
    public void PlayerShield() {
        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (player.ShieldCount > 0 && player.ShieldHP == 0) {
            player.ShieldHP = 30;
            player.ShieldCount--;
            gameUI.UpdatePlayerShieldBar();
            gameUI.UpdatePlayerShieldCount();

            aREffects.ShowPlayerShield();
        }

        PublishMqttUnity(SHIELD);
    }

    private void RemoveShieldEffects() {
        if (player.ShieldHP <= 0) {
            aREffects.RemovePlayerShield();
        }

        if (opponent.ShieldHP <= 0) {
            aREffects.RemoveOpponentShield();
        }
    }


    // ---------- Reload ----------
    public void PlayerReload() {
        CheckForOpponentRainBombCollision(); // for 2 player eval

        if (player.Ammo == 0) {
            player.Ammo = 6;
            gameUI.UpdateAmmoCount();

            aREffects.ShowReloadAnimation();
        }

        PublishMqttUnity(RELOAD);
    }


    // ---------- Log Out ----------
    public void PlayerLogOut() {
        CheckForOpponentRainBombCollision(); // for 2 player eval
        
        PublishMqttUnity(LOGOUT);
        // SceneManager.LoadScene("Log Out");
    }


    // ---------- Opponent Actions ----------
    public void OpponentHit() {
        aREffects.ShowPlayerHitEffect();
    }

    public void OpponentShield() {
        aREffects.ShowOpponentShield();
    }

}
