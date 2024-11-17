import json

class MessageParser:
    def __init__(self):
        pass

    def parse_unity_message(self, message):
        """
        Parses a JSON message from Unity to extract the player_id and action.
        Args:
            message (str): The JSON message from Unity, e.g., '{"player_id": "1", "action": "reload"}'.
        Returns:
            dict: A dictionary containing player_id and action, or None if parsing fails.
        """
        try:
            message_data = json.loads(message)
            player_id = message_data.get("player_id")
            action = message_data.get("action")
            print(f"Parsed Unity message: player_id = {player_id}, action = {action}")
            return {"player_id": player_id, "action": action}
        except json.JSONDecodeError as e:
            print(f"Error parsing Unity message: {str(e)}")
            return None

    def create_eval_server_message(self, player_id, action, players):
        """
        Creates a JSON message to send to the eval server.
        Args:
            player_id (str): The player ID who performed the action.
            action (str): The action performed by the player.
            players (dict): A dictionary containing the game state of all players.
        Returns:
            str: A JSON string ready to be sent to the eval server.
        """
        try:
            # Create the game state from the players dictionary
            game_state = {f'p{i}': player.get_dict() for i, player in players.items()}

            # Create the JSON message
            message = {
                "player_id": player_id,
                "action": action,
                "game_state": game_state
            }

            # Convert the dictionary to a JSON string
            json_message = json.dumps(message)
            print(f"Created eval server message: {json_message}")
            return json_message
        except Exception as e:
            print(f"Error creating eval server message: {str(e)}")
            return None
