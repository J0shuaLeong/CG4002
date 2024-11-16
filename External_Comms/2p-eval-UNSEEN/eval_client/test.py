import os
import json
import paho.mqtt.client as mqtt
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
from evaluation_client import EvaluationClient
from Player import Player
import threading
import queue

# Queue for handling messages
for_visualiser_queue = queue.Queue()
for_eval_server_queue = queue.Queue()

# Define a File Modified Handler
class FileModifiedHandler(FileSystemEventHandler):
    def __init__(self, eval_client, mqtt_client, players):
        self.eval_client = eval_client
        self.mqtt_client = mqtt_client
        self.players = players
        self.file_positions = {}

    def on_modified(self, event):
        if event.src_path.endswith("FOR_FPGA.txt"):
            self.process_file(event.src_path)

    def process_file(self, file_path):
        """Process the file, extracting the last message and forwarding it."""
        try:
            file_size = os.path.getsize(file_path)
            last_pos = self.file_positions.get(file_path, 0)

            # Read the file from the last position
            if last_pos < file_size:
                with open(file_path, 'r') as file:
                    file.seek(last_pos)
                    for line in file:
                        if not line.strip():
                            continue

                        # Extract action data from the line
                        _, json_str = line.split('_', 1)
                        action_data = json.loads(json_str.strip())
                        game_state = {f'p{i}': player.get_dict() for i, player in self.players.items()}

                        pid = int(action_data.get('player_id'))
                        action = action_data.get('action')

                        # Hand-off the message to other functions to process
                        self.forward_message(pid, action, game_state)

                self.file_positions[file_path] = file_size

        except Exception as e:
            print(f"An error occurred while processing the file: {e}")

    def forward_message(self, pid, action, game_state):
        """Craft and send messages to queues."""
        # Create the unity and json messages
        unity_message = self.create_unity_message(pid, action)
        json_message = self.create_json_message(pid, action, game_state)

        # Send messages to their respective queues for threads to process
        for_visualiser_queue.put(unity_message)
        for_eval_server_queue.put(json_message)
        print(f"Messages forwarded: Unity -> {unity_message}, JSON -> {json_message}")

    def create_unity_message(self, pid, action):
        """Create a Unity message."""
        return f"{pid}:{action}"

    def create_json_message(self, pid, action, game_state):
        """Create a JSON message."""
        return json.dumps({
            "player_id": pid,
            "action": action,
            "game_state": game_state
        })

# MQTT Event Handlers
def on_connect(client, userdata, flags, rc):
    print(f"Connected to MQTT broker with result code {rc}")

def on_publish(client, userdata, mid):
    print(f"Message {mid} has been published.")

# Define worker threads for processing messages
def visualiser_thread(mqtt_client):
    while True:
        try:
            message = for_visualiser_queue.get(block=True)
            # Process the Unity visualizer message here
            mqtt_client.publish("action/status/deen", message)
            print(f"Processing Unity visualiser message: {message}")
        except Exception as e:
            print(f"Error in visualiser_thread: {str(e)}")

def eval_server_thread():
    while True:
        try:
            message = for_eval_server_queue.get(block=True)
            # Process the message for evaluation server here
            print(f"Processing eval server message: {message}")
        except Exception as e:
            print(f"Error in eval_server_thread: {str(e)}")

# Main function
def main():
    # MQTT Broker settings
    broker_address = "broker.hivemq.com"
    broker_port = 1883
    mqtt_client = mqtt.Client("Ultra96_MQTT_Client")
    mqtt_client.on_connect = on_connect
    mqtt_client.on_publish = on_publish
    mqtt_client.connect(broker_address, broker_port, 60)
    mqtt_client.loop_start()

    # Evaluation client
    eval_client = EvaluationClient(8888, "127.0.0.1")
    players = {i: Player() for i in range(1, 3)}  # Prepare for 2 players

    print("Eval client establishing connection...")
    eval_client.establish_handshake()
    print("Handshake successful, waiting for server response...")

    # Start the watchdog observer
    event_handler = FileModifiedHandler(eval_client, mqtt_client, players)
    observer = Observer()
    observer.schedule(event_handler, path='/home/xilinx/external_comms/node_relay/', recursive=False)
    observer.start()

    # Start the worker threads2
    threading.Thread(target=visualiser_thread, args=(mqtt_client,), daemon=True).start()
    threading.Thread(target=eval_server_thread, daemon=True).start()

    try:
        while True:
            pass
    except KeyboardInterrupt:
        observer.stop()
        print("Stopped by the user.")
    observer.join()

    # Stop MQTT and clean up
    mqtt_client.loop_stop()
    mqtt_client.disconnect()
    eval_client.clientSocket.close()
    print("Connection closed.")

if __name__ == "__main__":
    main()
