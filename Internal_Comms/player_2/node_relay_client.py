import socket
import json
import threading
import time

class NodeRelayClient:
    def __init__(self, server_host, server_port, client_id):
        self.server_host = server_host
        self.server_port = server_port
        self.client_socket = None
        self.client_id = client_id
        self.stop_event = threading.Event()  # Add a stop event
        self.reconnect_delay = 3  # Delay in seconds before retrying connection

    def connect(self):
        while True:
            try:
                self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                self.client_socket.connect((self.server_host, self.server_port))
                print(f"Client {self.client_id}, connected to the server at {self.server_host}:{self.server_port}")
                break  # Exit loop on successful connection
            except Exception as e:
                print(f"Error connecting to server: {str(e)}. Retrying in {self.reconnect_delay} seconds...")
                time.sleep(self.reconnect_delay)

    def close(self):
        if self.client_socket:
            self.client_socket.close()
            print("Connection closed.")

    def send_data(self, data_queue):
        while not self.stop_event.is_set():
            try:
                # Get the latest data from the queue
                data = data_queue.get()

                # If None is received, break the loop
                if data is None:
                    break

                # Convert the data to JSON and send it over the socket
                json_data = json.dumps(data)
                length_prefix = f"{len(json_data)}_".encode()
                self.client_socket.sendall(length_prefix + json_data.encode())
                print(f"Sent data to server: {json_data}")

            except (ConnectionError, OSError) as e:
                print(f"Error sending data: {str(e)}. Reconnecting...")
                self.reconnect()
            except Exception as e:
                print(f"Unexpected error: {str(e)}")

    def receive_message(self):
        try:
            length_prefix = b""
            while not length_prefix.endswith(b"_"):
                chunk = self.client_socket.recv(1)
                if not chunk:
                    raise ConnectionError("Connection closed by the server.")
                length_prefix += chunk

            if len(length_prefix) == 0:
                print("Server closed connection")
                return None

            message_length = int(length_prefix.decode().strip("_"))
            data = b""
            while len(data) < message_length:
                chunk = self.client_socket.recv(message_length - len(data))
                if not chunk:
                    raise ConnectionError("Connection closed by the server.")
                data += chunk

            return data.decode()

        except ConnectionError as e:
            print(f"Error receiving data: {str(e)}. Reconnecting...")
            self.reconnect()
            return None

        except Exception as e:
            print(f"Unexpected error: {str(e)}")
            return None

    def handle_server(self, player_health_queue, player_bullets_queue):
        while not self.stop_event.is_set():
            try:
                message = self.receive_message()

                if message is None:
                    print("Connection to server lost, no incoming message from server")
                    break

                try:
                    message_json = json.loads(message)
                    if message_json['player_id'] == self.client_id:
                        player_health_queue.put(message_json)
                        player_bullets_queue.put(message_json)

                        print(f"Message received from server: {message_json}")

                except json.JSONDecodeError as e:
                    print(f"Error decoding JSON: {e}")
                    break

            except (ConnectionError, OSError) as e:
                print(f"Error handling server data: {str(e)}. Reconnecting...")
                self.reconnect()

    def reconnect(self):
        print("Attempting to reconnect...")
        self.close()  # Close current connection
        self.connect()  # Attempt to reconnect

    def stop(self):
        self.stop_event.set()  # Signal to stop threads
