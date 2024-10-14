import csv
import time
from queue import Queue


def read_csv_to_queue(csv_filename, sensor_data_queue):
    """ 
    Reads the CSV and puts new rows in the queue as they are added. 
    Continuously monitors the file for new data.
    """
    try:
        # Ensure the file is initially empty
        open(csv_filename, 'w').close()

        # Open the file in read mode and start reading from the beginning
        with open(csv_filename, 'r') as file:
            csv_reader = csv.reader(file)

            # Move the cursor to the end of the file
            file.seek(0, 2)

            while True:
                # Read new rows if present
                new_line = file.readline()
                if new_line:
                    row = new_line.strip().split(',')
                    if len(row) == 11:  # Ensure there are exactly 8 columns
                        data = {
                            "data_type": "sensor_data",
                            "player_id": row[0],
                            "device_id": row[1],
                            "feature": [row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9]],
                            "is_done": row[10]
                        }
                        sensor_data_queue.put(data)
                else:
                    time.sleep(0.1)  # Check every 0.2 seconds for new data

    except Exception as e:
        print(f"Error reading CSV file: {str(e)}")

def read_vest_to_queue(csv_filename, vest_data_queue):
    """ 
    Reads the CSV and puts new rows in the queue as they are added. 
    Continuously monitors the file for new data.
    """
    try:
        # Ensure the file is initially empty
        open(csv_filename, 'w').close()

        # Open the file in read mode and start reading from the beginning
        with open(csv_filename, 'r') as file:
            csv_reader = csv.reader(file)

            # Move the cursor to the end of the file
            file.seek(0, 2)

            while True:
                # Read new rows if present
                new_line = file.readline()
                if new_line:
                    row = new_line.strip().split(',')
                    if len(row) == 2:  # Ensure there are exact columns
                        data = {
                            "data_type": "is_shot",
                            "player_id": row[0],
                            "is_shot": row[1]
                        }
                        vest_data_queue.put(data)
                        print(f"data put into vest queue: {data}")
                else:
                    time.sleep(0.1)  # Check every 0.2 seconds for new data

    except Exception as e:
        print(f"Error reading CSV file: {str(e)}")

def read_bullets_to_queue(csv_filename, bullets_data_queue):
    """ 
    Reads the CSV and puts new rows in the queue as they are added. 
    Continuously monitors the file for new data.
    """
    try:
        # Ensure the file is initially empty
        open(csv_filename, 'w').close()

        # Open the file in read mode and start reading from the beginning
        with open(csv_filename, 'r') as file:
            csv_reader = csv.reader(file)

            # Move the cursor to the end of the file
            file.seek(0, 2)

            while True:
                # Read new rows if present
                new_line = file.readline()
                if new_line:
                    row = new_line.strip().split(',')
                    if len(row) == 2:  # Ensure there are exact columns
                        data = {
                            "data_type": "gun",
                            "player_id": row[0],
                            "bullets_count": row[1]
                        }
                        bullets_data_queue.put(data)
                        print(f"data put into bullets queue: {data}")
                else:
                    time.sleep(0.1)  # Check every 0.2 seconds for new data

    except Exception as e:
        print(f"Error reading CSV file: {str(e)}")