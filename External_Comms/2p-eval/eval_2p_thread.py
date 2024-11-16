import queue
import threading
import paho.mqtt.client as mqtt
import json
import time
from helpers.console_colour import ConsoleColors
import random

# ======= AI input data Thread =========
def AI_thread(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag):
    device_data_map = {
        1: [],  # p1_glove
        4: [],  # p1_leg
        5: [],  # p2_glove
        8: []  # p2_leg
    }

    # Initialize counts and inference flags for each device
    device_1_count = 100
    device_5_count = 100
    device_1_inferred = False
    device_5_inferred = False

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

            # Handle device-specific logic for clearing the list when packet count decreases
            if device_id == 1:
                if packet_count < device_1_count:  # Clear if current count is smaller
                    device_data_list.clear()
                    device_1_inferred = False  # Reset the inference flag
                device_1_count = packet_count
            elif device_id == 5:
                if packet_count < device_5_count:
                    device_data_list.clear()
                    device_5_inferred = False  # Reset the inference flag
                device_5_count = packet_count


            if packet_count <= 60:
                device_data_list.append(data)
            else:
                # Perform AI inference only once for packet_count > 60 AND player_response_flag
                if device_id == 1 and not device_1_inferred and not p1_response_flag:
                    print(f"{ConsoleColors.DEVICE1}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    player_id = 1
                    action = str(ai_action_output.get("action"))        # just added
                    ai_action_output["type"] = "action"

                    visualiser_queue_1.put(json.dumps(ai_action_output))  # string
                    print(f"{ConsoleColors.AIOUTPUT}{action} put into visualiser_queue_1{ConsoleColors.RESET}")

                    device_1_inferred = True  # Mark inference as done

                elif device_id == 5 and not device_5_inferred and not p2_response_flag:
                    print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    player_id = 2
                    action = str(ai_action_output.get("action"))
                    ai_action_output["type"] = "action"

                    visualiser_queue_2.put(json.dumps(ai_action_output))  # string
                    print(f"{ConsoleColors.AIOUTPUT}{action} put into visualiser_queue_2{ConsoleColors.RESET}")

                    device_5_inferred = True  # Mark inference as done

        except queue.Empty:
            pass



# ======= Eval Server Thread =========
# this is for 2 players
import threading
import json

def eval_server_thread(eval_client, players, unity_stats_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag):
    # p1_response_flag = False
    # p2_response_flag = False
    timer = None

    def send_random_action(player_id):
        """Send a random action to the eval server for the specified player."""
        random_action = random.choice(["move", "shoot", "defend"])  # Define random actions as needed
        eval_json_message = json.dumps({
            "player_id": player_id,
            "action": random_action,
            "game_state": generate_updated_player_stats(players)
        })
        eval_client.send_message(eval_json_message)
        print(f"Sent random action '{random_action}' to eval server for Player {player_id}")


    def reset_flags():
        """Reset both flags and send a random action for any player whose flag is still False."""
        nonlocal p1_response_flag, p2_response_flag, timer
        print("Timer expired. Resetting flags and checking for players without a response.")

        # Send random action if p1_response_flag is still False
        if not p1_response_flag:
            send_random_action(1)

        # Send random action if p2_response_flag is still False
        if not p2_response_flag:
            send_random_action(2)

        # Reset the flags
        p1_response_flag = False
        p2_response_flag = False

        # Cancel the timer if it's running
        if timer:
            timer.cancel()
            timer = None

    def start_timer_for_other_player(player_id):
        nonlocal timer
        if timer:
            timer.cancel()  # Cancel any previous timer
        timer = threading.Timer(55.0, reset_flags)
        print(f"Starting 55-second timer for Player {player_id}")
        timer.start()

    while True:
        try:
            unity_stats = json.loads(unity_stats_queue.get())   # dictionary
            print("took from unity_stats_queue")
            upid = json.loads(unity_stats["player_id"])
            
            if upid == 1 and p1_response_flag:
                print("P1 trying to send to eval server again")
                continue
            elif upid == 2 and p2_response_flag:
                print("P2 trying to send to eval server again")
                continue

            if not eval_client:
                print("Eval client not present, sending unity stats as ground truth to hardware")
                send_unity_stats_to_hardware(unity_stats, hardware_data_queue)
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
                print(f"Response from eval server received by eval client")
                print(response)
                send_response_to_visualiser_and_hardware(response, visualiser_queue_1, visualiser_queue_2, hardware_data_queue)

                # Set the response flag for the current player
                if upid == 1:
                    p1_response_flag = True
                    if not p2_response_flag:
                        start_timer_for_other_player(2)  # Start timer for Player 2 if their flag is False
                elif upid == 2:
                    p2_response_flag = True
                    if not p1_response_flag:
                        start_timer_for_other_player(1)  # Start timer for Player 1 if their flag is False

                # Check if both flags are set, if so, reset both flags and cancel the timer
                if p1_response_flag and p2_response_flag:
                    if timer:
                        timer.cancel()  # Cancel the timer if both flags are set
                    reset_flags()

            else:
                print("No response from eval_server")

        except queue.Empty:
            pass

        except Exception as e:
            # Handle any other exceptions here
            print(f"Error in eval_server_thread: {str(e)}")
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
        print(f"Response added to hardware_data_queue: {message}")


# ======= Visualiser Threads =========
def visualiser_thread_1(visualiser_queue_1, mqtt_client):
    while True:
        try:
            message = visualiser_queue_1.get()
            print(f"{ConsoleColors.VIS1}vis 1 thread: {message}{ConsoleColors.RESET}")

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
            print(f"{ConsoleColors.VIS2}vis 2 thread: {message}{ConsoleColors.RESET}")

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


def handle_action_message(message_data, mqtt_client, vis_number):    
    player_id = str(vis_number)
    action = message_data.get("action")
    topic = f"visualiser_{player_id}/action"
    mqtt_client.publish_to_topic(topic, action)

    if vis_number == 1:
        print(f"{ConsoleColors.VIS1}P{player_id} - Published action to MQTT topic '{topic}': {action}{ConsoleColors.RESET}")
    else :
        print(f"{ConsoleColors.VIS2}P{player_id} - Published action to MQTT topic '{topic}': {action}{ConsoleColors.RESET}")
      
    # P1 - Published action to MQTT topic 'visualiser_1/action': bomb


# ======= test_action_from_user_thread =========
def test_action_from_user_thread(visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag):
    while True:
        try:
            # Input player_id from user
            player_id = int(input("Input player_id (1 or 2): "))
            
            # Get message type (1 for gun, 2 for is_shot, 3 for action)
            message_type = int(input("Input message type (1: gun, 2: is_shot, 3: action): "))

            action_message_input = {
                "player_id": str(player_id),
                "type": ""
            }

            # Handle different message types
            if message_type == 1:
                # player fires, but bullet does not land on opponent
                action_message_input["type"] = "action"
                action_message_input["action"] = "gun"
                print(f"Player {player_id} fires gun, but bullet does not land on opponent.")

            elif message_type == 2:
                # Gun message for player_id, and is_shot for opponent player_id
                action_message_input["type"] = "action"
                action_message_input["action"] = "gun"
                print(f"Player {player_id} fires gun.")

                # Send gun message to current player
                if player_id == 1:
                    visualiser_queue_1.put(json.dumps(action_message_input))
                elif player_id == 2:
                    visualiser_queue_2.put(json.dumps(action_message_input))

                # Send is_shot message to opponent
                opponent_id = 1 if player_id == 2 else 2
                action_message_input_opponent = {
                    # here do player_id for felicia easier to process
                    "player_id": player_id,
                    "type": "action",
                    "action": "is_shot"
                }
                print(f"Player {opponent_id} is shot.")

                if player_id == 1:
                    visualiser_queue_1.put(json.dumps(action_message_input_opponent))
                elif player_id == 2:
                    visualiser_queue_2.put(json.dumps(action_message_input_opponent))
                continue

            elif message_type == 3:
                # Action message: input and send action for the player
                action = input(f"Input action for player {player_id}: ")
                action_message_input["action"] = action
                action_message_input["type"] = "action"
                print(f"Action '{action}' sent for player {player_id}.")
            
            # Put the message into the appropriate visualizer queue
            if player_id == 1:
                visualiser_queue_1.put(json.dumps(action_message_input))
            elif player_id == 2:
                visualiser_queue_2.put(json.dumps(action_message_input))

        except Exception as e:
            print(f"Error in getting user input: {str(e)}")



# ======= Start All Threads =========
def start_all_threads(ai, AI_data_queue, visualiser_queue_1, visualiser_queue_2, mqtt_client, players, running_event, node_relay_server, eval_server_queue, eval_client, hardware_data_queue, unity_stats_queue, p1_response_flag, p2_response_flag):
    threading.Thread(target=node_relay_server.start, args=(AI_data_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag), daemon=True).start()
    threading.Thread(target=AI_thread, args=(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag), daemon=True).start()
    threading.Thread(target=visualiser_thread_1, args=(visualiser_queue_1, mqtt_client), daemon=True).start()
    threading.Thread(target=visualiser_thread_2, args=(visualiser_queue_2, mqtt_client), daemon=True).start()
    threading.Thread(target=eval_server_thread, args=(eval_client, players, unity_stats_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag), daemon=True).start()

    # testing thread
    threading.Thread(target=test_action_from_user_thread, args=(visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag), daemon=True).start()

    print(f"{ConsoleColors.GREEN}All threads started{ConsoleColors.RESET}")


# ======= test_action_for_eval_thread =========
def test_action_for_eval_thread(unity_stats_queue):
    while True:
        try:
            # Input player_id from user
            player_id = str(input("Input player_id (1 or 2): "))
            action = str(input("Action for eval: "))

            unity_stats = {
                        "player_id": player_id,
                        "action": action,
                        "game_state": {
                            "p1": {
                                "hp": 100,
                                "bullets": 6,
                                "bombs": 2,
                                "shield_hp": 0,
                                "deaths": 0,
                                "shields": 3
                            },
                            "p2": {
                                "hp": 70,
                                "bullets": 6,
                                "bombs": 2,
                                "shield_hp": 0,
                                "deaths": 0,
                                "shields": 3
                            }
                        }
                    }
            unity_stats_queue.put(json.dumps(unity_stats))
            print("put into unity_stats_queue")

        except Exception as e:
            print(f"Error in getting user input: {str(e)}")


# ======= AI Test Threads =========
def AI_test_thread(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2):
    device_data_map = {
        1: [],  # p1_glove
        4: [],  # p1_leg
        5: [],  # p2_glove
        8: []  # p2_leg
    }

    # Initialize counts and inference flags for each device
    device_1_count = 100
    device_4_count = 100
    device_5_count = 100
    device_8_count = 100
    device_1_inferred = False
    device_4_inferred = False
    device_5_inferred = False
    device_8_inferred = False

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

            # Handle device-specific logic for clearing the list when packet count decreases
            if device_id == 1:
                if packet_count < device_1_count:  # Clear if current count is smaller
                    device_data_list.clear()
                    device_1_inferred = False  # Reset the inference flag
                device_1_count = packet_count
            elif device_id == 4:
                if packet_count < device_4_count:
                    device_data_list.clear()
                    device_4_inferred = False  # Reset the inference flag
                device_4_count = packet_count
            elif device_id == 5:
                if packet_count < device_5_count:
                    device_data_list.clear()
                    device_5_inferred = False  # Reset the inference flag
                device_5_count = packet_count
            elif device_id == 8:
                if packet_count < device_8_count:
                    device_data_list.clear()
                    device_8_inferred = False  # Reset the inference flag
                device_8_count = packet_count
            

            # Append data only if packet_count <= 49 cos brian now sending me from -10 to 60?
            if packet_count <= 60:
                device_data_list.append(data)
            else:
                # Perform AI inference only once for packet_count > 60
                if device_id == 1 and not device_1_inferred:
                    print(f"{ConsoleColors.DEVICE1}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    player_id = 1
                    action = str(ai_action_output.get("action"))        # just added
                    ai_action_output["type"] = "action"

                    print(f"{ConsoleColors.AIOUTPUT}P{player_id} Action : {action}{ConsoleColors.RESET}")                    

                    device_1_inferred = True  # Mark inference as done

                elif device_id == 4 and not device_4_inferred:
                    print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    player_id = 1
                    action = str(ai_action_output.get("action"))
                    ai_action_output["type"] = "action"

                    print(f"{ConsoleColors.AIOUTPUT}P{player_id} Action : {action}{ConsoleColors.RESET}")

                    device_4_inferred = True  # Mark inference as done

                elif device_id == 5 and not device_5_inferred:
                    print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    player_id = 2
                    action = str(ai_action_output.get("action"))
                    ai_action_output["type"] = "action"

                    print(f"{ConsoleColors.AIOUTPUT}P{player_id} Action : {action}{ConsoleColors.RESET}")

                    device_5_inferred = True  # Mark inference as done

                elif device_id == 8 and not device_8_inferred:
                    print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
                    ai_action_output = json.loads(ai.inference(device_data_list))
                    player_id = 2
                    action = str(ai_action_output.get("action"))
                    ai_action_output["type"] = "action"

                    print(f"{ConsoleColors.AIOUTPUT}P{player_id} Action : {action}{ConsoleColors.RESET}")

                    device_8_inferred = True  # Mark inference as done

        except queue.Empty:
            pass


# ======= Start AI testing Threads =========
def start_AI_test_threads(ai, AI_data_queue, running_event, node_relay_server, hardware_data_queue, visualiser_queue_1, visualiser_queue_2, eval_client, players, unity_stats_queue):
    threading.Thread(target=node_relay_server.start, args=(AI_data_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2), daemon=True).start()
    threading.Thread(target=AI_test_thread, args=(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2), daemon=True).start()

    print(f"{ConsoleColors.GREEN}All threads started{ConsoleColors.RESET}")







# # ======= AI input data Thread =========
# def AI_thread(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2):
#     device_data_map = {
#         1: [],  # p1_glove
#         4: [],  # p1_leg
#         5: [],  # p2_glove
#         8: []  # p2_leg
#     }

#     # Initialize counts and inference flags for each device
#     device_1_count = 100
#     device_4_count = 100
#     device_5_count = 100
#     device_8_count = 100
#     device_1_inferred = False
#     device_4_inferred = False
#     device_5_inferred = False
#     device_8_inferred = False

#     while running_event.is_set():
#         try:
#             data = json.loads(AI_data_queue.get())  # dict
#             print(f"AI_thread check: {data}")
            
#             device_id = int(data.get("device_id"))
#             packet_count = int(data.get("count"))
            
#             if device_id not in device_data_map:
#                 print(f"{ConsoleColors.RED}Unknown device_id: {device_id}{ConsoleColors.RESET}")
#                 continue

#             device_data_list = device_data_map[device_id]

#             # Handle device-specific logic for clearing the list when packet count decreases
#             if device_id == 1:
#                 if packet_count < device_1_count:  # Clear if current count is smaller
#                     device_data_list.clear()
#                     device_1_inferred = False  # Reset the inference flag
#                 device_1_count = packet_count
#             # elif device_id == 4:
#             #     if packet_count < device_4_count:
#             #         device_data_list.clear()
#             #         device_4_inferred = False  # Reset the inference flag
#             #     device_4_count = packet_count
#             elif device_id == 5:
#                 if packet_count < device_5_count:
#                     device_data_list.clear()
#                     device_5_inferred = False  # Reset the inference flag
#                 device_5_count = packet_count
#             # elif device_id == 8:
#             #     if packet_count < device_8_count:
#             #         device_data_list.clear()
#             #         device_8_inferred = False  # Reset the inference flag
#             #     device_8_count = packet_count
            

#             # Append data only if packet_count <= 49 cos brian now sending me from -10 to 60?
#             if packet_count <= 60:
#                 device_data_list.append(data)
#             else:
#                 # Perform AI inference only once for packet_count > 60
#                 if device_id == 1 and not device_1_inferred:
#                     print(f"{ConsoleColors.DEVICE1}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
#                     ai_action_output = json.loads(ai.inference(device_data_list))
#                     player_id = 1
#                     action = str(ai_action_output.get("action"))        # just added
#                     ai_action_output["type"] = "action"

#                     visualiser_queue_1.put(json.dumps(ai_action_output))  # string
#                     print(f"{ConsoleColors.AIOUTPUT}{action} put into visualiser_queue_1{ConsoleColors.RESET}")

#                     device_1_inferred = True  # Mark inference as done

#                 # elif device_id == 4 and not device_4_inferred:
#                 #     print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
#                 #     ai_action_output = json.loads(ai.inference(device_data_list))
#                 #     player_id = 1
#                 #     action = str(ai_action_output.get("action"))
#                 #     ai_action_output["type"] = "action"

#                 #     visualiser_queue_1.put(json.dumps(ai_action_output))  # string
#                 #     print(f"{ConsoleColors.AIOUTPUT}{action} put into visualiser_queue_1{ConsoleColors.RESET}")

#                 #     device_4_inferred = True  # Mark inference as done

#                 elif device_id == 5 and not device_5_inferred:
#                     print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
#                     ai_action_output = json.loads(ai.inference(device_data_list))
#                     player_id = 2
#                     action = str(ai_action_output.get("action"))
#                     ai_action_output["type"] = "action"

#                     visualiser_queue_2.put(json.dumps(ai_action_output))  # string
#                     print(f"{ConsoleColors.AIOUTPUT}{action} put into visualiser_queue_2{ConsoleColors.RESET}")

#                     device_5_inferred = True  # Mark inference as done

#                 # elif device_id == 8 and not device_8_inferred:
#                 #     print(f"{ConsoleColors.DEVICE5}Device {device_id} length = {len(device_data_list)}{ConsoleColors.RESET}")
                    
#                 #     ai_action_output = json.loads(ai.inference(device_data_list))
#                 #     player_id = 2
#                 #     action = str(ai_action_output.get("action"))
#                 #     ai_action_output["type"] = "action"

#                 #     visualiser_queue_2.put(json.dumps(ai_action_output))  # string
#                 #     print(f"{ConsoleColors.AIOUTPUT}{action} put into visualiser_queue_2{ConsoleColors.RESET}")

#                 #     device_8_inferred = True  # Mark inference as done

#         except queue.Empty:
#             pass
