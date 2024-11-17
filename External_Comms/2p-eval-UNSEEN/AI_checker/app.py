from flask import Flask, jsonify, render_template, request

app = Flask(__name__)

# Dictionary to store progress for each device
progress_data = {
    1: 0,  # Device 1
    4: 0,  # Device 4
    5: 0,  # Device 5
    8: 0   # Device 8
}

@app.route('/')
def index():
    # Serve the HTML page with progress bars
    return render_template('index.html')

@app.route('/update_progress', methods=['POST'])
def update_progress():
    data = request.json
    device_id = data.get('device_id')
    progress = data.get('progress')
    
    if device_id in progress_data:
        progress_data[device_id] = progress  # Update the progress for the device
    return jsonify(success=True)

@app.route('/get_progress')
def get_progress():
    # Return the current progress for each device as JSON
    return jsonify(progress_data)

if __name__ == '__main__':
    app.run(debug=True)
