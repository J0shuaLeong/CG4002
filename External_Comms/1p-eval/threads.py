import queue
import threading
import paho.mqtt.client as mqtt
import json
import time
from helpers.console_colour import ConsoleColors

# ======= AI input data Thread =========
def AI_thread(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2, eval_server_queue):
    # Device ID mapped to lists
    p1_glove = []  # id 1
    p1_leg = []    # id 4
    p2_glove = []  # id 5
    p2_leg = []    # id 8
    
    device_data_map = {
        1: p1_glove,
        4: p1_leg,
        5: p2_glove,
        8: p2_leg
    }

    while running_event.is_set():
        try:
            # Get the data from AI_data_queue
            data = json.loads(AI_data_queue.get())
            print(f"AI_thread check: {data}")
            
            is_done = data.get("is_done")
            device_id = int(data.get("device_id"))

            if device_id not in device_data_map:
                print(f"Unknown device_id: {device_id}")
                continue  # Skip unknown device IDs

            # Append data to the correct list based on device_id when is_done is False
            if is_done == "False":
                device_data_map[device_id].append(data)
                continue  # Continue collecting data until is_done is True

            # When is_done is True, send the device's data for inference
            if is_done == "True":
                device_data_map[device_id].append(data)
                device_data_list = device_data_map[device_id]
                print(f"length = {len(device_data_list)}")

                # TODO what happens if true is received and length is not 30? discard all?

                # Perform inference using AI
                ai_action_output = ai.inference(device_data_list)
                print(ai_action_output)
                ai_action_output = json.loads(ai_action_output)

                player_id = int(ai_action_output.get("player_id"))
                ai_action_output["type"] = "action"

                # Send the action output to the correct visualiser queue
                if player_id == 1:
                    vis_1_action = json.dumps(ai_action_output)
                    visualiser_queue_1.put(vis_1_action)
                    print("Put into visualiser_queue_1")
                elif player_id == 2:
                    vis_2_action = json.dumps(ai_action_output)
                    visualiser_queue_2.put(vis_2_action)
                    print("Put into visualiser_queue_2")
                else:
                    print(f"Unknown player_id: {player_id}")

                # Clear the data list after inference
                device_data_list.clear()

            # TODO if there is no addition to any of the list, clear the list so that list will be new when reconnected

        except queue.Empty:
            pass

# ======= Eval Server Thread =========
# this thread will take in the unity stats first and process it accordingly 
# if there is eval_client, then use unity stats to update player 
# if there is eval_client, use response to update player

# this is for 1 player
def eval_server_thread(mqtt_client, eval_server_queue, eval_client, players, eval_ground_truth_queue, unity_stats_queue,hardware_data_queue, visualiser_queue_1, visualiser_queue_2):
    while True:
        try:
            # TODO wait for until unity to publish to me the most updated game state then carry on. topic - "gamestats/eval_server"
            # take from unity_stats_queue
            unity_stats = json.loads(unity_stats_queue.get())   # converts to python dictionary
            
            # print(type(unity_stats))
            print("Message from mqtt_client unity_stats_queue retrieved")

            upid = json.loads(unity_stats["player_id"])

            # # If eval_client is not available, process the unity stats as ground truth
            if not eval_client:
                print("Eval client not present, sending unity stats as ground truth to hardware")

                # Extract the game state for each player
                game_state = unity_stats.get("game_state", {})
                p1_stats = game_state.get("p1", {})
                p2_stats = game_state.get("p2", {})

                # Extract health and bullets count for each player
                p1_hp = str(p1_stats.get("hp"))
                p1_bullets = str(p1_stats.get("bullets"))
                p2_hp = str(p2_stats.get("hp"))
                p2_bullets = str(p2_stats.get("bullets"))

                # Prepare the messages for both players
                message_p1 = {
                    "player_id": "1",
                    "health": p1_hp,
                    "bullets_count": p1_bullets
                }

                message_p2 = {
                    "player_id": "2",
                    "health": p2_hp,
                    "bullets_count": p2_bullets
                }

                # Add the messages to the hardware_data_queue
                hardware_data_queue.put(message_p1)
                hardware_data_queue.put(message_p2)

                print(f"Added to hardware_data_queue: {message_p1}")
                print(f"Added to hardware_data_queue: {message_p2}")

            if not eval_client:
                print("no eval truth response from server")
                continue
            
            
            if eval_client:
                for player_key, player_state in unity_stats["game_state"].items():
                    # print(player_key, player_state)
                    player_id = int(player_key[1])  # Extract player ID from 'p1', 'p2' as 1, 2, etc.
                    
                    if player_id in players:
                        players[player_id].update_state(player_state)

                # # Generate the current game state for all players
                updated_player_stats = {f'p{i}': player.get_dict() for i, player in players.items()}
                # print(updated_player_stats)

                # Prepare JSON to send to evaluation server
                eval_json_message = json.dumps({
                    "player_id": unity_stats["player_id"],
                    "action": unity_stats["action"],
                    "game_state": updated_player_stats
                })

                
                print("message for eval server made")
                eval_client.send_message(eval_json_message)
                # print(json.loads(json_message))

                # Receive the response from the evaluation server (ground truth)
                response = eval_client.receive_message()
    
                # catch empty string "" and None
                if response: 
                    print(f"{ConsoleColors.YELLOW}Response from eval server received by eval client{ConsoleColors.RESET}")
                    print(f"{ConsoleColors.YELLOW}{response}{ConsoleColors.RESET}")
                    
                    # Ensure the response is parsed correctly before placing it in the queue
                    if isinstance(response, str):
                        response = json.loads(response)     # now rsponse is a pyhton dictionary

                    # put into visualiser 1 and 2 queue, publish to topic gamestats/eval_server
                    response_for_vis = {}
                    response_for_vis["type"] = "eval_ground_truth"
                    response_for_vis["eval_ground_truth_stats"] = response
                    print(type(response_for_vis))
                    print(response_for_vis)
                    visualiser_queue_1.put(json.dumps(response_for_vis))
                    visualiser_queue_2.put(json.dumps(response_for_vis))
                    
                    p1_stats = response.get("p1", {})
                    p2_stats = response.get("p2", {})

                    # Extract health and bullets count for each player
                    p1_hp = str(p1_stats.get("hp"))
                    p1_bullets = str(p1_stats.get("bullets"))
                    p2_hp = str(p2_stats.get("hp"))
                    p2_bullets = str(p2_stats.get("bullets"))

                    # Prepare the messages for both players hardware
                    message_p1 = {
                        "player_id": "1",
                        "health": p1_hp,
                        "bullets_count": p1_bullets
                    }

                    message_p2 = {
                        "player_id": "2",
                        "health": p2_hp,
                        "bullets_count": p2_bullets
                    }

                    # Add the messages to the hardware_data_queue
                    hardware_data_queue.put(message_p1)
                    hardware_data_queue.put(message_p2)

                    print(f"Added to hardware_data_queue: {message_p1}")
                    print(f"Added to hardware_data_queue: {message_p2}")

                else:
                    print(f"{ConsoleColors.RED}No response from eval_server{ConsoleColors.RESET}")

            # eval_ground_truth_queue.put(response)

        except queue.Empty:
            pass

        except Exception as e:
            # print(f"Error in eval_server_thread: {str(e)}")
            pass
            
# ======= Visualiser Thread 1 =========
def visualiser_thread_1(visualiser_queue_1, mqtt_client):
    while True:
        try:
            message = visualiser_queue_1.get()
            print(f"vis 1 thread: {message}")

            try:
                message_data = json.loads(message)

                message_type = message_data.get("type")
                # print(f"hi: {message_type}")
                if message_type == "eval_ground_truth": 
                    vis_eval_ground_truth_message = json.dumps(message_data.get("eval_ground_truth_stats"))
                    mqtt_client.publish_to_topic("gamestats/eval_ground_truth", vis_eval_ground_truth_message)
                    print("vis_eval_ground_truth sent to visualiser 1")
                elif message_type == "action":
                    # gun and is_shot is included
                    # pass all actions to felicia, including gun and is_shot
                    handle_action_message(message_data, mqtt_client)

                    # dont do anything if the action is now just gun
                    if message_data.get("action") == "gun":
                        print("health no update, send action to felicia, decrease ammo count")
                        continue

                    # act as taking from unity for hardware
                    time.sleep(1)
                    player_id = message_data.get("player_id")
                    action = message_data.get("action")

                    test_message = {
                        "player_id": player_id,
                        "action": action,
                        "game_state": {
                            "p1": {
                                "hp": 80,
                                "bullets": 6,
                                "bombs": 2,
                                "shield_hp": 20,
                                "deaths": 1,
                                "shields": 2
                            },
                            "p2": {
                                "hp": 40,
                                "bullets": 5,
                                "bombs": 2,
                                "shield_hp": 10,
                                "deaths": 2,
                                "shields": 1
                            }
                        }
                    }

                    test_message_json = json.dumps(test_message)
                    mqtt_client.publish_to_topic("gamestats/unity", test_message_json)

                # elif message_type == "is_shot"                    
                # elif message_type == "health":
                #     handle_health_update(message_data, mqtt_client)
                else:
                    print(f"Unknown message type: {message_type}")

            except json.JSONDecodeError as e:
                print(f"Error parsing JSON: {str(e)}")

        except queue.Empty:
            pass

# ======= Visualiser Thread 2 =========
def visualiser_thread_2(visualiser_queue_2, mqtt_client):
    while True:
        try:
            message = visualiser_queue_2.get()
            print(f"vis 2 thread: {message}")

            try:
                message_data = json.loads(message)

                message_type = message_data.get("type")
                if message_type == "eval_ground_truth": 
                    vis_eval_ground_truth_message = json.dumps(message_data.get("eval_ground_truth_stats"))
                    mqtt_client.publish_to_topic("gamestats/eval_ground_truth", vis_eval_ground_truth_message)
                    print("vis_eval_ground_truth sent to visualiser 2")
                elif message_type == "action":
                    # gun and is_shot is included
                    # pass all actions to felicia, including gun and is_shot
                    handle_action_message(message_data, mqtt_client)

                    # dont do anything if the action is now just gun
                    if message_data.get("action") == "gun":
                        print("health no update, send action to felicia, decrease ammo count")
                        continue

                    # Act as taking from unity
                    time.sleep(1)
                    player_id = message_data.get("player_id")
                    action = message_data.get("action")

                    test_message = {
                        "player_id": player_id,
                        "action": action,
                        "game_state": {
                            "p1": {
                                "hp": 80,
                                "bullets": 6,
                                "bombs": 2,
                                "shield_hp": 20,
                                "deaths": 1,
                                "shields": 2
                            },
                            "p2": {
                                "hp": 40,
                                "bullets": 5,
                                "bombs": 2,
                                "shield_hp": 10,
                                "deaths": 2,
                                "shields": 1
                            }
                        }
                    }

                    test_message_json = json.dumps(test_message)
                    mqtt_client.publish_to_topic("gamestats/unity", test_message_json)

                # elif message_type == "is_shot"
                # elif message_type == "health":
                #     handle_health_update(message_data, mqtt_client)
                else:
                    print(f"Unknown message type: {message_type}")

            except json.JSONDecodeError as e:
                print(f"Error parsing JSON: {str(e)}")

        except queue.Empty:
            pass

# ======= Visualiser Methods =========
def handle_action_message(message_data, mqtt_client):
    """
    Publish all actions including gun and is_shot
    """
    player_id = message_data.get("player_id")
    action = message_data.get("action")
    # message = f"{player_id}:{action}"
    # TODO change the action to the actual action
    # message = "Player1Shoot"

    if player_id == "1":
        # mqtt_client.publish_to_topic("visualiser_1/action", message)
        mqtt_client.publish_to_topic("visualiser_1/action", action)
        print(f"{ConsoleColors.BLUE}P1 - Published action to MQTT: {action}{ConsoleColors.RESET}")

    elif player_id == "2":
        mqtt_client.publish_to_topic("visualiser_2/action", action)
        print(f"{ConsoleColors.MAGENTA}P2 - Published action to MQTT: {action}{ConsoleColors.RESET}")

# TODO change this to handle_player_state_update
# def handle_health_update(message_data, mqtt_client):
#     """
#     Handle and publish the health update message.
#     """
#     player_id = message_data.get("player_id")
#     health = message_data.get("health")
#     message = f"{player_id}:{health}"

#     if player_id == "1":
#         mqtt_client.publish_to_topic("visualiser_1/health", health)
#         print(f"{ConsoleColors.BLUE}P1 - Published health to MQTT: {message}{ConsoleColors.RESET}")

#     elif player_id == "2":
#         mqtt_client.publish_to_topic("visualiser_2/health", health)
#         print(f"{ConsoleColors.MAGENTA}P2 - Published health to MQTT: {message}{ConsoleColors.RESET}")


# ======= Hardware Thread =========
# publish health and bullet to internal comms 
# should place into this hardware queue everytime there is a change in the health or bullet count 

# ======= test_action_from_user_thread =========
def test_action_from_user_thread(visualiser_queue_1):
    while True:
        try:
            action = input("Type in your action: ")
            action_message_input = {
                "player_id": "1",
                "action": action, 
                "type": "action"
            }
            visualiser_queue_1.put(json.dumps(action_message_input))

        except Exception as e:
            print(f"Error in getting user input: {str(e)}")

# ======= test_data_from_user_thread =========
# def test_data_from_user_thread(hardware_data_queue):
#     """
#     Continuously get input from the user and put a JSON message in the hardware_data_queue.
#     """
#     while True:
#         # Get input for player_id, health, and bullets_count
#         try:
#             # print()
#             player_id = input("Enter player_id: ")
#             health = input("Enter health: ")
#             bullets_count = input("Enter ammo count: ")

#             # Create a dictionary with the 3 fields
#             message = {
#                 "player_id": player_id,
#                 "health": health,
#                 "bullets_count": bullets_count
#             }

#             # Put the message into the hardware_data_queue -> testing hardware thread
#             hardware_data_queue.put(message)

#             print(f"Message for client {player_id} added to queue")
#         except Exception as e:
#             print(f"Error in getting user input: {str(e)}")



# ======= Start All Threads =========
def start_all_threads(ai, AI_data_queue, visualiser_queue_1, visualiser_queue_2, mqtt_client, players, running_event, node_relay_server, eval_server_queue, eval_client, eval_ground_truth_queue, hardware_data_queue, unity_stats_queue):

    threading.Thread(target=node_relay_server.start, args=(AI_data_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2), daemon=True).start()

    threading.Thread(target=AI_thread, args=(ai, AI_data_queue, running_event, visualiser_queue_1, visualiser_queue_2, eval_server_queue), daemon=True).start()
    
    # threading.Thread(target=AI_output_action_thread, args=(AI_data_queue, visualiser_queue_1, visualiser_queue_2, eval_server_queue, running_event), daemon=True).start()

    threading.Thread(target=visualiser_thread_1, args=(visualiser_queue_1, mqtt_client), daemon=True).start()

    threading.Thread(target=visualiser_thread_2, args=(visualiser_queue_2, mqtt_client), daemon=True).start()
    
    threading.Thread(target=eval_server_thread, args=(mqtt_client, eval_server_queue, eval_client, players, eval_ground_truth_queue, unity_stats_queue, hardware_data_queue, visualiser_queue_1, visualiser_queue_2), daemon=True).start()
    
    # threading.Thread(target=game_engine_thread, args=(eval_ground_truth_queue, players, visualiser_queue_1, visualiser_queue_2), daemon=True).start()

    # Start the test data from user thread (this will handle user inputs)
    # threading.Thread(target=test_data_from_user_thread, args=(hardware_data_queue,), daemon=True).start()

    threading.Thread(target=test_action_from_user_thread, args=(visualiser_queue_1,), daemon=True).start()

    print(f"{ConsoleColors.GREEN}All threads started{ConsoleColors.RESET}")






# ======= game engine thread - DONT NEED CAN DELETE =========
# def game_engine_thread(eval_ground_truth_queue, players, visualiser_queue_1, visualiser_queue_2):
    # while True:
    #     try:
    #         # Get ground truth from eval_ground_truth_queue
    #         players_ground_truth = eval_ground_truth_queue.get(timeout=0.1)
            
    #         # Update player states from ground truth
    #         update_player_states(players, players_ground_truth)


    #     except queue.Empty:
    #         pass

# def update_player_states(players, players_ground_truth):
#     """
#     Update the current player states with the values from the ground truth.
#     """
#     if 'p1' in players_ground_truth and 'p2' in players_ground_truth:
#         # Update Player 1's state
#         update_single_player(players[1], players_ground_truth['p1'])

#         # Update Player 2's state
#         update_single_player(players[2], players_ground_truth['p2'])

#         print(f"{ConsoleColors.YELLOW}Updated game state with ground truth{ConsoleColors.RESET}")

#         # TODO send latest bullets to individual players

#         # print(f"{ConsoleColors.YELLOW}Updated game state with ground truth: {players_ground_truth}{ConsoleColors.RESET}")


# def update_single_player(player, ground_truth):
#     """
#     Update an individual player's state with the values from the ground truth.
#     """
#     player.hp = ground_truth['hp']
#     player.num_bullets = ground_truth['bullets']
#     player.num_bombs = ground_truth['bombs']
#     player.hp_shield = ground_truth['shield_hp']
#     player.num_deaths = ground_truth['deaths']
#     player.num_shield = ground_truth['shields']


# def send_health_to_visualizers(players, visualiser_queue_1, visualiser_queue_2):
#     """
#     Send updated health values to the visualizer queues for each player.
#     """
#     # Create the health update message for Player 1
#     player_1_health_message = json.dumps({
#         "type": "health",
#         "player_id": "1",
#         "health": players[1].hp
#     })
#     visualiser_queue_1.put(player_1_health_message)
#     # print(f"{ConsoleColors.BLUE}Sent Player 1's health: {players[1].hp} to visualizer 1{ConsoleColors.RESET}")

#     # Create the health update message for Player 2
#     player_2_health_message = json.dumps({
#         "type": "health",
#         "player_id": "2",
#         "health": players[2].hp
#     })
#     visualiser_queue_2.put(player_2_health_message)
#     # print(f"{ConsoleColors.MAGENTA}Sent Player 2's health: {players[2].hp} to visualizer 2{ConsoleColors.RESET}")
