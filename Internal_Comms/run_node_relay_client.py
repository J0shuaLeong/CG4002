from node_relay_client import NodeRelayClient
from add_to_queue import read_csv_to_queue
import queue
import threading


def main():
    # Set up connection parameters
    server_host = '172.26.191.19'
    server_port = 7777

    # Establish connection
    try:
        client = NodeRelayClient(server_host, server_port)
        client.connect()
        print("Connection with U96 successful")
    except Exception as e:
        print(f"Failed to connect to server: {e}")
        return

    # Set up a queue for sensor data
    sensor_data_queue = queue.Queue()

    # Start a thread to read data from the CSV into the queue
    csv_filename = 'test_inputs.csv'
    csv_thread = threading.Thread(
        target=read_csv_to_queue, args=(csv_filename, sensor_data_queue))
    csv_thread.start()

    # Start sending data from the queue to U96
    try:
        player_id = 1
        # Use send_sensor_data method
        client.send_sensor_data(player_id, sensor_data_queue)

    except KeyboardInterrupt:
        print("Interrupted by user")

    finally:
        # Close the connection when done
        client.close()
        csv_thread.join()  # Ensure the CSV thread is closed properly


if __name__ == "__main__":
    main()
