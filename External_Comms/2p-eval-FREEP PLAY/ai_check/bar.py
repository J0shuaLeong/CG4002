import requests
import time

def update_progress(device_id, progress):
    """Send a progress update to the web app."""
    try:
        response = requests.post('http://localhost:5000/update_progress', json={
            'device_id': device_id,
            'progress': progress
        })
        if response.status_code == 200:
            print(f"Updated progress for Device {device_id} to {progress}%")
        else:
            print(f"Failed to update progress for Device {device_id}. Server responded with status code {response.status_code}.")
    except requests.exceptions.RequestException as e:
        print(f"Failed to update progress for Device {device_id}: {e}")

def simulate_progress():
    device_ids = [1, 4, 5, 8]
    progress_values = {device_id: 0 for device_id in device_ids}

    while True:
        for device_id in device_ids:
            progress_values[device_id] += 10
            if progress_values[device_id] > 100:
                progress_values[device_id] = 0
            
            update_progress(device_id, progress_values[device_id])

        time.sleep(1)

if __name__ == "__main__":
    simulate_progress()
