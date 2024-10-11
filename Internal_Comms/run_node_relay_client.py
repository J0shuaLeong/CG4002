from node_relay_client import NodeRelayClient
from add_to_queue import read_csv_to_queue, read_vest_to_queue, read_bullets_to_queue
import queue
import threading
import time


def main(player_health_queue, player_bullet_queue):
    while True:
        # Set up connection parameters
        server_host = '172.26.191.19'
        server_port = 7777
        client_id = '1' #change the player ID accordingly

        client = None
        connected = False
        # Attempt to establish the connection in a loop
        while not connected:
            try:
                client = NodeRelayClient(server_host, server_port, client_id)
                client.connect()
                print("Connection with U96 successful")
                connected = True
            except Exception as e:
                print(f"Failed to connect to server: {e}")
                print("Retrying connection in 5 seconds...")
                time.sleep(5)  # Wait before retrying the connection

        try:
            # Set up a queues for data tranmission
            sensor_data_queue = queue.Queue()
            vest_data_queue = queue.Queue()
            bullets_data_queue = queue.Queue()

            # Start a thread to read data from the IMU CSV into the queue
            imu_csv_filename = 'player_1_imu.csv'
            imu_csv_thread = threading.Thread(
                target=read_csv_to_queue, args=(imu_csv_filename, sensor_data_queue))
            imu_csv_thread.start()

            # Start a thread to send  IMU data from the queue to U96
            imu_send_thread = threading.Thread(
                target=client.send_data, args=(sensor_data_queue,))
            imu_send_thread.start()

            # Start a thread to read data from the VEST CSV into the queue
            vest_csv_filename = 'player_1_vest.csv'
            vest_csv_thread = threading.Thread(
                target=read_vest_to_queue, args=(vest_csv_filename, vest_data_queue))
            vest_csv_thread.start()

            # Start a thread to send VEST data from the queue to U96
            vest_send_thread = threading.Thread(
                target=client.send_data, args=(vest_data_queue,))
            vest_send_thread.start()

            # Start a thread to read data from the BULLETS CSV into the queue
            bullets_csv_filename = 'player_1_bullets.csv'
            bullets_csv_thread = threading.Thread(
                target=read_bullets_to_queue, args=(bullets_csv_filename, bullets_data_queue))
            bullets_csv_thread.start()

            # Start a thread to send BULLETS data from the queue to U96
            bullets_send_thread = threading.Thread(
                target=client.send_data, args=(bullets_data_queue,))
            bullets_send_thread.start()

            # Start a thread to receive messages from the server
            receive_thread = threading.Thread(
                target=client.handle_server, args=(player_health_queue, player_bullet_queue,))
            receive_thread.start()

        # Start sending data from the queue to U96
            receive_thread.join()  # Wait for the receive thread to finish
            imu_send_thread.join()     # Wait for the send thread to finish
            imu_csv_thread.join()      # Wait for the CSV thread to finish
            vest_send_thread.join()     # Wait for the send thread to finish
            vest_csv_thread.join()      # Wait for the CSV thread to finish
            bullets_send_thread.join()     # Wait for the send thread to finish
            bullets_csv_thread.join()      # Wait for the CSV thread to finish

        except Exception as e:
            print(f"Error occurred: {e}")
            print("Reconnecting...")
            connected = False  # Force reconnection if any error occurs
            time.sleep(5)  # Optional delay before retrying
        except KeyboardInterrupt:
            print("Interrupted by user")
            client.stop()
            break
        finally:
            # receive_thread.join()
            # send_thread.join()
            # csv_thread.join()
            client.close()


#if __name__ == "__main__":
#    main()
