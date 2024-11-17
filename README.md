# Laser Tag Capstone Project

## Overview

This project is a capstone endeavor focused on creating an advanced laser tag system. The system combines custom-built hardware and software to deliver an immersive and interactive laser tag experience. From real-time scoring to dynamic gameplay, this project demonstrates the seamless integration of cutting-edge technology and fun!

---

## Features

- **Real-Time Scoring System:** Tracks players' scores and updates in real-time.
- **Interactive Gameplay:** Multiple game modes, including Free-For-All, Single Player, and Two-Player modes.
- **User-Friendly Interface:** A mobile interface to display game visuals and player statistics.
- **Custom Laser Tag Hardware:** Laser guns with infrared sensors and motion detection capabilities, enhancing the gameplay experience.

---

## Technologies Used

- **Hardware:** Arduino, Infrared Sensors, Beetle BLE, MPU6050 accelerometer and gyroscope.
- **Hardware AI:** AI Accelerator on FPGA for processing player actions and gameplay data.
- **Internal Communications:** Bluetooth Low Energy connectivity and Relay Node for seamless device communication.
- **External Communications:** TLS/SSL Protocol for secure server-client interaction.
- **Software:** Unity Game Engine for game development and a responsive game UI for visualizer devices.

---

## System Setup

### 1. Reverse SSH Connection

Establish a connection between the Evaluation Client (running locally) and the Evaluation Server (running on Ultra96):

```bash
ssh -R 9999:localhost:<eval_port> xilinx@makerslab-fpga-21.d2.comp.nus.edu.sg
```

Replace `<eval_port>` with the port number provided by the evaluation server. This step ensures secure communication between the two components.

---

### 2. Running the External Communications Script

Open a new terminal and log in to the Ultra96 server:

```bash
ssh xilinx@makerslab-fpga-21.d2.comp.nus.edu.sg
sudo -i
cd /home/xilinx/external_comms/
python3 main.py
```

This command initiates the `main.py` script, which handles external communications between the Ultra96 FPGA and the evaluation server.

---

### 3. Setting Up Hardware Devices

While the external communications script is running:

1. Ensure all hardware peripherals (laser guns, gloves, and leg devices) are powered on and securely connected to their batteries.
2. Verify that the peripherals are ready for use.

---

### 4. Connecting Node Relay and Beetle Peripherals

On the Node Relay laptop:

1. Open 2 terminals, one for each player.
2. cd Internal_Comms/player_{PLAYER_ID}, PLAYER_ID = 1 or 2
3. Ensure that the laptop is connected to the NUS wifi, or use forticlient to connect.
4. Ensure that the laptop bluetooth is on.
5. Run the `main.py` script to establish a connection with the beetle peripherals:
```bash
cd Internal_Comms/player_{PLAYER_ID}, PLAYER_ID = 1 or 2
python3 main.py
```
6. Monitor the terminal for notifications to ensure that all the beetles peripherals are connected.

---

### 5. Visualizer Setup

1. Ensure the visualizer devices are connected to the NUS Wi-Fi. If not, enable FortiClient for network access.
2. Launch the visualizer app on each device.
3. Initialize the player within the app.

---

## Gameplay

Once all components are set up, you are ready to play! The system offers:

- Real-time updates of player scores and actions.
- Visual effects and animations for a rich and immersive environment.

---

## Troubleshooting

- **Connection Issues**: Check network connectivity and verify that all scripts are running without errors.
- **Peripherals Not Responding**: Ensure batteries are fully charged, and devices are switched on.
- **Visualizer Not Loading**: Verify NUS Wi-Fi connection or FortiClient status.

---

## Future Enhancements

- Implementing additional game modes for diverse player preferences.
- Optimizing hardware AI for faster response times.
- Expanding compatibility with various hardware platforms.

---

## Have Fun!

Enjoy the immersive AR Laser Tag experience.
