import paho.mqtt.client as mqtt
import json
import threading  # For thread synchronization
from helpers.console_colour import ConsoleColors

class MqttClient:
    def __init__(self, unity_stats_queue):
        self.broker_address = None
        self.port = None
        self.username = None
        self.pw = None
        self.client = None
        self.subscribed_topics = []  # List to store topics to subscribe to
        self.lock = threading.Lock()

        # queues 
        # unity_stats_queue = queue.Queue()
        self.unity_stats_queue = unity_stats_queue
    
    def on_connect(self, client, userdata, flags, rc):
        print(f"MQTT started, MQTT broker connected")
        # Subscribe to all topics once the client connects
        for topic in self.subscribed_topics:
            self.client.subscribe(topic, qos=0)
            print(f"Subscribed to topic: {topic}")

    def on_message(self, client, userdata, message):
        # Process the message (example)
        if message.topic == "gamestats/unity":
            unity_stats = message.payload.decode("utf-8")  # Assuming JSON payload, decode json alrdy, same as json.loads
            print("MQTT - Unity stats message received ")

            # Put the message into the unity_stats_queue
            self.unity_stats_queue.put(unity_stats)
            print("Unity stats message put into unity_stats_queue")
            
            # from unity 
            # {
            #     "player_id":"1",
            #     "action":"shield",
            #     "game_state":{
            #         "p1":{
            #             "hp":100,
            #             "bullets":6,
            #             "bombs":2,
            #             "shield_hp":30,
            #             "deaths":0,
            #             "shields":2
            #         },
            #         "p2":{
            #             "hp":100,
            #             "bullets":6,
            #             "bombs":2,
            #             "shield_hp":0,
            #             "deaths":0,
            #             "shields":3
            #         }
            #     }
            # }


    def add_topic_to_subscribe(self, topic):
        # Add topic to the list of topics to subscribe to
        if topic not in self.subscribed_topics:
            self.subscribed_topics.append(topic)
    
    def publish_to_topic(self, topic, msg):
        self.client.publish(topic, msg, qos=0)

    def subscribe_to_topics(self):
        # Subscribe to all stored topics if not already subscribed
        for topic in self.subscribed_topics:
            self.client.subscribe(topic, qos=0)
            print(f"Subscribed to topic: {topic}")

    def unsubscribe_all(self):
        # Unsubscribe from all topics
        for topic in self.subscribed_topics:
            self.client.unsubscribe(topic)
        self.subscribed_topics.clear()  # Clear the list after unsubscribing

    def start_client(self, on_message_custom=None):
        # Load broker configuration (e.g., from a config file)
        with open('./info.json') as f:
            data = json.load(f)
            self.broker_address = data['mqtt']['hostname']
            self.port = data['mqtt']['port']
            self.username = data['mqtt']['username']
            self.pw = data['mqtt']['password']

        # Connect to the MQTT broker
        self.client = mqtt.Client()
        self.client.on_connect = self.on_connect
        self.client.on_message = on_message_custom if on_message_custom else self.on_message

        # Set authentication and connect to broker
        self.client.username_pw_set(self.username, self.pw)
        self.client.connect(self.broker_address, self.port)
        
        # Start the loop in a separate thread
        self.client.loop_start()

    def close_mqtt_client(self):
        self.unsubscribe_all()  # Unsubscribe from all topics
        self.client.disconnect()
        self.client.loop_stop()
        print(f"{ConsoleColors.RED}MQTT client disconnected and loop stopped.{ConsoleColors.RESET}")
