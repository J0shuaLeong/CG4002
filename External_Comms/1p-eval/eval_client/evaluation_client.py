from socket import *
from eval_client.encryption import SecureMessenger  # Ensure the correct path
import struct

class EvaluationClient:
    # default host is localhost
    def __init__(self, port_num: int, host: str = "localhost") -> None:
        self.clientSocket = socket(AF_INET, SOCK_STREAM)
        self.port = port_num
        self.host = host

    def send_message(self, message: str) -> None:
        message = SecureMessenger.encode_message(message)
        self.clientSocket.send(f"{len(message)}_".encode())
        self.clientSocket.send(message)

    def establish_handshake(self) -> None:
        self.clientSocket.connect((self.host, self.port))
        self.send_message("hello")

    def receive_message(self) -> str:
        length_prefix = b""
        while not length_prefix.endswith(b"_"):
            chunk = self.clientSocket.recv(1)
            if not chunk:
                raise ConnectionError("Connection is closed by the server.")
            length_prefix += chunk

        length = int(length_prefix.decode().strip("_"))
        data = b""
        while len(data) < length :
            chunk = self.clientSocket.recv(length - len(data))
            if not chunk:
                raise ConnectionError("Connection closed while receiving the message")
            data += chunk
        
        return data.decode()
    
    


    
