import csv
import random
import time

# Function to generate random row
def generate_random_row():
    field_0 = random.randint(100, 999)  # 3-digit number
    field_1 = random.randint(100, 999)  # 3-digit number
    field_2 = random.randint(-100, -1)  # Negative number
    field_3 = random.randint(-100, -1)  # Negative number
    field_4 = random.randint(-100, -1)  # Negative number
    field_5 = random.randint(0, 9)      # Single digit
    return [field_0, field_1, field_2, field_3, field_4, field_5]

# Function to populate and update CSV in real time
def populate_csv_realtime(filename, interval=0.2, iterations=100):
    with open(filename, mode='a', newline='') as file:  # 'a' for append mode
        writer = csv.writer(file)
        for _ in range(iterations):
            writer.writerow(generate_random_row())
            time.sleep(interval)  # Wait for 0.2 seconds before generating the next row

# Call the function to update 'test_inputs.csv' file in real time
populate_csv_realtime('test_inputs.csv', interval=0.2, iterations=100)
