# import csv
# import time


# def read_csv_to_queue(csv_filename, sensor_data_queue):
#     """
#     Reads the CSV and puts new rows in the queue as they are added.
#     Continuously monitors the file for new data.
#     """
#     try:
#         open(csv_filename, 'w').close()
#         with open(csv_filename, 'r') as file:
#             csv_reader = csv.reader(file)

#             # Skip the header (if any)
#             next(csv_reader, None)

#             # Track the current row count
#             current_rows = list(csv_reader)
#             for row in current_rows:
#                 data = {
#                     "type": "sensor_data",
#                     "feature": [row[0], row[1], row[2], row[3], row[4], row[5]]
#                 }
#                 sensor_data_queue.put(data)

#         # Monitor the file for new rows
#         while True:
#             time.sleep(1)  # Poll for new rows every second
#             with open(csv_filename, 'r') as file:
#                 csv_reader = csv.reader(file)
#                 next(csv_reader, None)
#                 # Get only new rows
#                 new_rows = list(csv_reader)[len(current_rows):]

#                 for row in new_rows:
#                     data = {
#                         "type": "sensor_data",
#                         "feature": [row[0], row[1], row[2], row[3], row[4], row[5]]
#                     }
#                     sensor_data_queue.put(data)
#                     current_rows.append(row)  # Update current row count

#     except Exception as e:
#         print(f"Error reading CSV file: {str(e)}")


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
                    if len(row) == 6:  # Ensure there are exactly 6 columns
                        data = {
                            "type": "sensor_data",
                            "feature": [row[0], row[1], row[2], row[3], row[4], row[5]]
                        }
                        sensor_data_queue.put(data)
                else:
                    time.sleep(0.2)  # Check every 0.2 seconds for new data

    except Exception as e:
        print(f"Error reading CSV file: {str(e)}")


# Example usage
# sensor_data_queue = Queue()
# csv_filename = 'test_inputs.csv'

# In a real program, this would run in a separate thread while another thread populates the CSV.
# read_csv_to_queue(csv_filename, sensor_data_queue)

# Check the queue for new data
# while True:
#     if not sensor_data_queue.empty():
#         print(sensor_data_queue.get())
#     time.sleep(0.1)
