from node_relay_client import NodeRelayClient
from add_to_queue import read_csv_to_queue, read_vest_to_queue, read_bullets_to_queue, read_soccer_to_queue
import queue
import threading


def main(player_health_queue, player_bullet_queue,):
    # Set up connection parameters
    server_host = '172.26.191.19'
    server_port = 7777
    client_id = '2'  # Change the player ID accordingly

    # Create client and connect (reconnection handled internally in NodeRelayClient)
    client = NodeRelayClient(server_host, server_port, client_id)
    client.connect()  # Initial connection handled, with retry inside NodeRelayClient

    try:
        # Set up queues for data transmission
        sensor_data_queue = queue.Queue()
        vest_data_queue = queue.Queue()
        bullets_data_queue = queue.Queue()
        soccer_data_queue = queue.Queue()

        # Start a thread to read data from the IMU CSV into the queue
        imu_csv_filename = f'player_{client_id}_imu.csv'
        imu_csv_thread = threading.Thread(
            target=read_csv_to_queue, args=(imu_csv_filename, sensor_data_queue))
        imu_csv_thread.start()

        # Start a thread to send IMU data from the queue to U96
        imu_send_thread = threading.Thread(
            target=client.send_data, args=(sensor_data_queue,))
        imu_send_thread.start()

        # Start a thread to read data from the VEST CSV into the queue
        vest_csv_filename = f'player_{client_id}_vest.csv'
        vest_csv_thread = threading.Thread(
            target=read_vest_to_queue, args=(vest_csv_filename, vest_data_queue))
        vest_csv_thread.start()

        # Start a thread to send VEST data from the queue to U96
        vest_send_thread = threading.Thread(
            target=client.send_data, args=(vest_data_queue,))
        vest_send_thread.start()

        # Start a thread to read data from the BULLETS CSV into the queue
        bullets_csv_filename = f'player_{client_id}_bullets.csv'
        bullets_csv_thread = threading.Thread(
            target=read_bullets_to_queue, args=(bullets_csv_filename, bullets_data_queue))
        bullets_csv_thread.start()

        # Start a thread to send BULLETS data from the queue to U96
        bullets_send_thread = threading.Thread(
            target=client.send_data, args=(bullets_data_queue,))
        bullets_send_thread.start()

        # Start a thread to read data from the SOCCER CSV into the queue
        soccer_csv_filename = f'player_{client_id}_soccer.csv'
        soccer_csv_thread = threading.Thread(
            target=read_soccer_to_queue, args=(soccer_csv_filename, soccer_data_queue))
        soccer_csv_thread.start()

        # Start a thread to send SOCCER data from the queue to U96
        soccer_send_thread = threading.Thread(
            target=client.send_data, args=(soccer_data_queue,))
        soccer_send_thread.start()

        # Start a thread to receive messages from the server
        receive_thread = threading.Thread(
            target=client.handle_server, args=(player_health_queue, player_bullet_queue,))
        receive_thread.start()

        # Wait for all threads to complete
        receive_thread.join()
        imu_send_thread.join()
        imu_csv_thread.join()
        vest_send_thread.join()
        vest_csv_thread.join()
        bullets_send_thread.join()
        bullets_csv_thread.join()
        soccer_send_thread.join()
        soccer_csv_thread.join()

    except Exception as e:
        print(f"Error occurred: {e}")
    except KeyboardInterrupt:
        print("Interrupted by user")
        client.stop()
    finally:
        client.close()

