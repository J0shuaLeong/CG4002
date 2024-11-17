import socket
import threading
import json
from helpers.console_colour import ConsoleColors

class NodeRelayServer:
    def __init__(self, port=7777):
        self.port = port
        self.server_socket = None
        self.clients = []  # List to store connected client sockets
        self.clients_lock = threading.Lock()  # Lock for thread-safe access to clients

    def setup_server(self):
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.server_socket.bind(('0.0.0.0', self.port))
        self.server_socket.listen()
        print(f"Node Relay server is listening on port {self.port}...")

    def broadcast_message(self, server_data_queue):
        try:
            while True:
                message = server_data_queue.get()

                if not self.clients:
                    print("No clients connected")
                    continue

                try:
                    json_data = json.dumps(message)
                except TypeError as json_err:
                    print(f"Error serializing message to JSON: {str(json_err)}")
                    continue

                length_prefix = f"{len(json_data)}_".encode()

                with self.clients_lock:
                    # Create a copy of the clients list to avoid modification during iteration
                    for client_socket in list(self.clients):
                        try:
                            if client_socket.fileno() == -1:
                                raise ConnectionError("Socket is closed, cannot send data.")
                            client_socket.sendall(length_prefix + json_data.encode())
                            print(f"Broadcast message to client {client_socket.getpeername()}")

                        except (ConnectionError, OSError) as e:
                            print(f"Error sending data to client {client_socket.getpeername()}: {str(e)}")
                            self.remove_client(client_socket)
        except Exception as e:
            print(f"Error in broadcasting messages: {type(e).__name__} - {str(e)}")

    def remove_client(self, client_socket):
        try:
            client_address = client_socket.getpeername()  # Get the client's address before closing the socket
        except OSError:
            client_address = "Unknown"  # In case the socket is already invalid, handle gracefully

        with self.clients_lock:
            if client_socket in self.clients:
                self.clients.remove(client_socket)
                client_socket.close()
                print(f"{ConsoleColors.RED}Client {client_address} disconnected and removed{ConsoleColors.RESET}")

    def receive_message(self, client_socket) -> str:
        length_prefix = b""
        try:
            while not length_prefix.endswith(b"_"):
                chunk = client_socket.recv(1)
                if not chunk:
                    raise ConnectionError("Connection closed by the client.")
                length_prefix += chunk

            length = int(length_prefix.decode().strip("_"))

            data = b""
            while len(data) < length:
                chunk = client_socket.recv(length - len(data))
                if not chunk:
                    raise ConnectionError("Connection closed while receiving the message.")
                data += chunk

            return data.decode()

        except Exception as e:
            print(f"Error receiving data: {str(e)}")
            self.remove_client(client_socket)
            return None

    def handle_client(self, client_socket, AI_data_queue, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag):
        client_info = client_socket.getpeername()
        try:
            while True:
                data = self.receive_message(client_socket)
                if not data:
                    break

                message_data = json.loads(data)
                data_type = message_data.get("data_type")
                player_id = message_data.get("player_id")

                if data_type == "sensor_data":
                    AI_data_queue.put(data)
                elif data_type == "gun":
                    json_message = json.dumps({
                        "player_id": player_id,
                        "bullets_count": message_data.get("bullets_count"),
                        "type": "action",
                        "action": "gun"
                    })
                    print(f"bullets_count: {json_message}")
                    if player_id == "1" and not p1_response_flag:
                        visualiser_queue_1.put(json_message)
                    elif player_id == "2" and not p2_response_flag:
                        visualiser_queue_2.put(json_message)

                # ask brian to remove also
                # elif data_type == "is_shot":
                #     json_message = json.dumps({
                #         "player_id": player_id,
                #         "is_shot": message_data.get("is_shot"),
                #         "type": "action",
                #         "action": "is_shot"
                #     })
                #     print(f"is_shot: {json_message}")
                #     if player_id == "1" and not p1_response_flag:
                #         # deen flip for felicia to process it easier on unity
                #         visualiser_queue_2.put(json_message)
                #     elif player_id == "2" and not p2_response_flag:
                #         # deen flip for felicia to process it easier on unity
                #         visualiser_queue_1.put(json_message)

                # uncomment for soccer 
                elif data_type == "soccer":
                    json_message = json.dumps({
                        "player_id": player_id,
                        "type": "action",
                        "action": "soccer"
                    })
                    # print(f"hardware gun: {json_message}")
                    if player_id == "1" and not p1_response_flag:
                        visualiser_queue_1.put(json_message)
                    elif player_id == "2" and not p2_response_flag:
                        visualiser_queue_2.put(json_message)
                

        except ConnectionError as e:
            print(f"Connection error: {str(e)}")
        finally:
            self.remove_client(client_socket)

    def start(self, AI_data_queue, server_data_queue, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag):
        self.setup_server()

        threading.Thread(target=self.broadcast_message, args=(server_data_queue,), daemon=True).start()

        try:
            while True:
                client_socket, addr = self.server_socket.accept()
                print(f"{ConsoleColors.GREEN}Connection established with {addr}{ConsoleColors.RESET}")
                with self.clients_lock:
                    self.clients.append(client_socket)

                threading.Thread(target=self.handle_client, args=(client_socket, AI_data_queue, visualiser_queue_1, visualiser_queue_2, p1_response_flag, p2_response_flag), daemon=True).start()

        except KeyboardInterrupt:
            print("Server shutdown requested by user.")
        finally:
            self.server_socket.close()
            print("Server socket closed.")