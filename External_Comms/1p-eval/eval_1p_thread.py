import queue
import threading
import paho.mqtt.client as mqtt
import json
import time
from helpers.console_colour import ConsoleColors

# ======= AI input data Thread =========
def AI_thread(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2):
    device_data_map = {
        1: [],  # p1_glove
    }

    # Initialize counts and inference flags for each device
    device_1_count = 100
    device_1_inferred = False

    while running_event.is_set():
        try:
            data = json.loads(AI_data_queue.get())  # dict
            print(f"AI_thread check: {data}")
            
            device_id = int(data.get("device_id"))
            packet_count = int(data.get("count"))
            
            if device_id not in device_data_map:
                print(f"{ConsoleColors.RED}Unknown device_id: {device_id}{ConsoleColors.RESET}")
                continue

            device_data_list = device_data_map[device_id]

            if packet_count < device_1_count:  # Clear if current count is smaller
                device_data_list.clear()
                device_1_inferred = False  # Reset the inference flag
                
            device_1_count = packet_count

            # Append data only if packet_count <= 60
            if packet_count <= 60:
                device_data_list.append(data)
                continue
            else:
                # Perform AI inference only once for packet_count > 60
                if device_id == 1 and not device_1_inferred:
                    print(f"{ConsoleColors.DEVICE1}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    action = str(ai_action_output.get("action"))        # just added
                    player_id = int(ai_action_output.get("player_id"))
                    ai_action_output["type"] = "action"

                    visualiser_queue_1.put(json.dumps(ai_action_output))  # string
                    print(f"{ConsoleColors.AIOUTPUT}AI action output: {action}{ConsoleColors.RESET}")

                    device_1_inferred = True  # Mark inference as done

        except queue.Empty:
            pass



# ======= Eval Server Thread =========
def eval_server_thread(eval_client, players, unity_stats_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2):

    while True:
        try:
            unity_stats = json.loads(unity_stats_queue.get())       # dict
            # print("Message from mqtt_client gamestats/unity retrieved")

            if not eval_client:
                print("Eval client not present, sending unity stats as ground truth to hardware")

                # send to hardware
                send_unity_stats_to_hardware(unity_stats, hardware_data_queue)

                # update stats for u96 players

                # dont need to send to visualiser because there is no update of ground truth on visualiser side
                continue
            
            # if eval_client is present
            process_unity_stats_for_eval_server(unity_stats, players)

            eval_json_message = json.dumps({
                "player_id": unity_stats["player_id"],
                "action": unity_stats["action"],
                "game_state": generate_updated_player_stats(players)
            })

            eval_client.send_message(eval_json_message) # string
            response = eval_client.receive_message()    


            if response:
                response = json.loads(response)     # dict
                print(f"{ConsoleColors.YELLOW}Response from eval server received by eval client{ConsoleColors.RESET}")
                # {'p1': {'hp': 100, 'bullets': 6, 'bombs': 2, 'shield_hp': 30, 'deaths': 0, 'shields': 2}, 'p2': {'hp': 100, 'bullets': 6, 'bombs': 2, 'shield_hp': 0, 'deaths': 0, 'shields': 3}}
                print(f"{ConsoleColors.YELLOW}{response}{ConsoleColors.RESET}")
                send_response_to_visualiser_and_hardware(response, visualiser_queue_1, visualiser_queue_2, hardware_data_queue)
            else:
                print(f"{ConsoleColors.RED}No response from eval_server{ConsoleColors.RESET}")

        except queue.Empty:
            pass
        except Exception as e:
            pass

def send_unity_stats_to_hardware(unity_stats, hardware_data_queue):
    game_state = unity_stats.get("game_state", {})
    for player_id, stats in [("1", game_state.get("p1", {})), ("2", game_state.get("p2", {}))]:
        message = {
            "player_id": player_id,
            "health": str(stats.get("hp")),
            "bullets_count": str(stats.get("bullets"))
        }
        hardware_data_queue.put(message)
        print(f"Added to hardware_data_queue: {message}")

def process_unity_stats_for_eval_server(unity_stats, players):
    for player_key, player_state in unity_stats["game_state"].items():
        player_id = int(player_key[1])
        if player_id in players:
            players[player_id].update_state(player_state)

def generate_updated_player_stats(players):
    return {f'p{i}': player.get_dict() for i, player in players.items()}

def send_response_to_visualiser_and_hardware(response, visualiser_queue_1, visualiser_queue_2, hardware_data_queue):
    response_for_vis = {
        "type": "eval_ground_truth",
        "eval_ground_truth_stats": response
    }

    visualiser_queue_1.put(json.dumps(response_for_vis))        # string
    visualiser_queue_2.put(json.dumps(response_for_vis))        # string
    print(f"Eval response added to visualiser 1 and 2 queues")

    for player_id, stats in [("1", response.get("p1", {})), ("2", response.get("p2", {}))]:
        message = {
            "player_id": player_id,             # "1", "2" - string
            "health": str(stats.get("hp")),     
            "bullets_count": str(stats.get("bullets"))  
        }
        hardware_data_queue.put(message)
        # print(f"Response added to hardware_data_queue: {message}")

# ======= Visualiser Threads =========
def visualiser_thread_1(visualiser_queue_1, mqtt_client):
    while True:
        try:
            message = visualiser_queue_1.get()
            print(f"vis 1 thread: {message}")

            try:
                message_data = json.loads(message)
                handle_visualiser_message(message_data, mqtt_client, 1)
            except json.JSONDecodeError as e:
                print(f"{ConsoleColors.RED}Vis 1 - Error parsing JSON: {str(e)}{ConsoleColors.RESET}")

        except queue.Empty:
            pass

def visualiser_thread_2(visualiser_queue_2, mqtt_client):
    while True:
        try:
            message = visualiser_queue_2.get()
            print(f"vis 2 thread: {message}")

            try:
                message_data = json.loads(message)      # dict
                handle_visualiser_message(message_data, mqtt_client, 2)
            except json.JSONDecodeError as e:
                print(f"{ConsoleColors.RED}Vis 2 - Error parsing JSON: {str(e)}{ConsoleColors.RESET}")

        except queue.Empty:
            pass

def handle_visualiser_message(message_data, mqtt_client, vis_number):
    # send out the eval_ground_truth to both visualisers
    message_type = message_data.get("type")
    if message_type == "eval_ground_truth":
        handle_eval_ground_truth(message_data, mqtt_client, vis_number)

    elif message_type == "action":
        handle_action_message(message_data, mqtt_client, vis_number)
    
    else:
        print(f"{ConsoleColors.RED}Unkown message type, visualiser unable to process{ConsoleColors.RESET}")

def handle_eval_ground_truth(message_data, mqtt_client, vis_number):
    eval_ground_truth = json.dumps(message_data.get("eval_ground_truth_stats"))     #string
    mqtt_client.publish_to_topic(f"gamestats/eval_server", eval_ground_truth)

    if vis_number == 1:
        print(f"{ConsoleColors.VIS1}eval_ground_truth sent to visualiser {vis_number}{ConsoleColors.RESET}")
    else:
        print(f"{ConsoleColors.VIS2}eval_ground_truth sent to visualiser {vis_number}{ConsoleColors.RESET}")


def handle_action_message(message_data, mqtt_client, vis_number):
    player_id = message_data.get("player_id")
    action = message_data.get("action")

    topic = f"visualiser_{player_id}/action"
    mqtt_client.publish_to_topic(topic, action)

    if vis_number == 1:
        print(f"{ConsoleColors.VIS1}P{player_id} - Published action to MQTT topic '{topic}': {action}{ConsoleColors.RESET}")
    else :
        print(f"{ConsoleColors.VIS2}P{player_id} - Published action to MQTT topic '{topic}': {action}{ConsoleColors.RESET}")
        
    # P1 - Published action to MQTT topic 'visualiser_1/action': bomb
    


# ======= test_action_from_user_thread =========
# def test_action_from_user_thread(visualiser_queue_1, visualiser_queue_2):
# def test_action_from_user_thread(visualiser_queue_1):
#     while True:
#         try:
#             # player_id = input("Input player_id: ")
#             action = input("Input action: ")
#             action_message_input = {
#                 "player_id": 1,
#                 "action": action, 
#                 "type": "action"
#             }
            
#             # if player_id == 1:                
#             visualiser_queue_1.put(json.dumps(action_message_input))
#             print(f"Put into visualiser_queue_1")
#             # elif player_id == 2:
#             #     visualiser_queue_2.put(json.dumps(action_message_input))
#             #     print(f"Put into visualiser_queue_2")
            

#         except Exception as e:
#             print(f"Error in getting user input: {str(e)}")


def test_action_from_user_thread(visualiser_queue_1):
    while True:
        try:
            action = input("Type in your action: ")
            action_message_input = {
                "player_id": "1",
                "action": str(action), 
                "type": "action"
            }
            visualiser_queue_1.put(json.dumps(action_message_input))        # string

        except Exception as e:
            print(f"Error in getting user input: {str(e)}")

# ======= Start All Threads =========
def start_all_threads(ai, AI_data_queue, visualiser_queue_1, visualiser_queue_2, mqtt_client, players, running_event, node_relay_server, eval_server_queue, eval_client, hardware_data_queue, unity_stats_queue):
    threading.Thread(target=node_relay_server.start, args=(AI_data_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2), daemon=True).start()
    threading.Thread(target=AI_thread, args=(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2), daemon=True).start()
    threading.Thread(target=visualiser_thread_1, args=(visualiser_queue_1, mqtt_client), daemon=True).start()
    threading.Thread(target=visualiser_thread_2, args=(visualiser_queue_2, mqtt_client), daemon=True).start()
    threading.Thread(target=eval_server_thread, args=(eval_client, players, unity_stats_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2), daemon=True).start()

    # testing thread
    threading.Thread(target=test_action_from_user_thread, args=(visualiser_queue_1,), daemon=True).start()

    print(f"{ConsoleColors.GREEN}All threads started{ConsoleColors.RESET}")
