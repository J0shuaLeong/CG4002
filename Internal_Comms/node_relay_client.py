import socket
import json


class NodeRelayClient:
    def __init__(self, server_host, server_port):
        self.server_host = server_host
        self.server_port = server_port
        self.client_socket = None

    def connect(self):
        try:
            self.client_socket = socket.socket(
                socket.AF_INET, socket.SOCK_STREAM)
            self.client_socket.connect((self.server_host, self.server_port))
            print(
                f"Connected to the server at {self.server_host}:{self.server_port}")
        except Exception as e:
            print(f"Error connecting to server, exiting program: {str(e)}")
            exit()

    def close(self):
        if self.client_socket:
            self.client_socket.close()
            print("Connection closed.")

    def send_sensor_data(self, player_id, sensor_data_queue):
        try:
            while True:
                # Get the latest data from the queue
                data = sensor_data_queue.get()

                # If None is received, break the loop
                if data is None:
                    break

                # Add player_id to the data
                data['player_id'] = player_id

                # Convert the data to JSON and send it over the socket
                json_data = json.dumps(data)
                length_prefix = f"{len(json_data)}_".encode()
                self.client_socket.sendall(length_prefix + json_data.encode())
                print(f"Sent data: {json_data}")

        except Exception as e:
            print(f"Error sending data: {str(e)}")
