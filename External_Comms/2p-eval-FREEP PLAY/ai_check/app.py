# from flask import Flask, render_template
# from flask_socketio import SocketIO, emit

# app = Flask(__name__)
# socketio = SocketIO(app)

# # Route to serve the front-end HTML page
# @app.route('/')
# def index():
#     return render_template('index.html')

# @socketio.on('connect')
# def handle_connect():
#     print("Client connected")

# # Handle progress updates from the AI_test_thread
# @socketio.on('progress_update')
# def handle_progress_update(data):
#     """
#     Receives progress data from AI_test_thread and emits it to all connected clients.
#     """
#     device_id = data.get('device_id')
#     progress = data.get('progress')
    
#     # Broadcast the progress update to all connected clients (front-end)
#     emit('progress_update', {'device_id': device_id, 'progress': progress}, broadcast=True)
#     print(f"Progress update for Device {device_id}: {progress}%")

# if __name__ == '__main__':
#     # Start the Flask-SocketIO server
#     socketio.run(app, host='0.0.0.0', port=5000, debug=True)


from flask import Flask, render_template
from flask_socketio import SocketIO, emit

app = Flask(__name__)
socketio = SocketIO(app)

# Route to serve the front-end HTML page
@app.route('/')
def index():
    return render_template('index.html')

@socketio.on('connect')
def handle_connect():
    print("Client connected")

# Handle progress updates from the AI_test_thread
@socketio.on('progress_update')
def handle_progress_update(data):
    """
    Receives progress data from AI_test_thread and emits it to all connected clients.
    """
    device_id = data.get('device_id')
    progress = data.get('progress')
    action = data.get('action')  # Optional action data
    
    # Broadcast the progress and action update to all connected clients (front-end)
    emit('progress_update', {'device_id': device_id, 'progress': progress, 'action': action}, broadcast=True)
    print(f"Progress update for Device {device_id}: {progress}%, Action: {action}")

if __name__ == '__main__':
    # Start the Flask-SocketIO server
    socketio.run(app, host='0.0.0.0', port=5000, debug=True)
