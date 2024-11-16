import threading
import queue
from eval_client.evaluation_client import EvaluationClient
from eval_client.Player import Player
# from eval_2p_thread import start_all_threads
from eval_ai_check_thread import start_AI_test_threads
from helpers.mqtt_client import MqttClient
from node_relay.NodeRelayServer import NodeRelayServer  # Import the class here
from helpers.console_colour import ConsoleColors
from AI_integration import AI


# Main function to start the system
def main():
    eval_server_present = False
    if eval_server_present:
        eval_client = EvaluationClient(9999, "127.0.0.1")
        print("Eval client establishing connection...")
        eval_client.establish_handshake()
        print("Handshake successful, waiting for server response...")
    else:
        eval_client = None
    
    # create queues
    visualiser_queue_1 = queue.Queue()
    visualiser_queue_2 = queue.Queue()
    AI_data_queue = queue.Queue()       # Get sensors data
    hardware_data_queue = queue.Queue()

    # Start mqtt
    unity_stats_queue = queue.Queue()   # for mqtt
    mqtt_client = MqttClient(unity_stats_queue)
    mqtt_client.add_topic_to_subscribe("gamestats/unity")
    # add more topics to subscribe to if needed
    mqtt_client.start_client()

    # Initialise AI
    ai = AI()

    # Instantiate the NodeRelayServer
    node_relay_server = NodeRelayServer(port=7777)
    
    # Initialize players in a set
    players = {i: Player() for i in range(1, 3)}  # Prepare for 2 players

    # create a running event for thread control
    running_event = threading.Event()
    running_event.set()  # Set the event to start threads

    start_AI_test_threads(ai, AI_data_queue, running_event, node_relay_server, hardware_data_queue, visualiser_queue_1, visualiser_queue_2, eval_client, players, unity_stats_queue)


    try:
        while True:
            pass

    except KeyboardInterrupt:
        # running_event.clear()  # Stop the node relay to AI thread
        print(f"{ConsoleColors.RED}Shutting down main...{ConsoleColors.RESET}")
        mqtt_client.close_mqtt_client()

    finally:
        running_event.clear()  # Stop the node relay to AI thread

    # Shutdown the evaluation client connection if it was initialized
    if eval_client:
        eval_client.clientSocket.close()
        print(f"{ConsoleColors.RED}Evaluation client connection closed.{ConsoleColors.RESET}")

    print(f"{ConsoleColors.RED}System shutdown completed.{ConsoleColors.RESET}")

if __name__ == "__main__":
    main()