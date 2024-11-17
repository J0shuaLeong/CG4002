from bluepy.btle import DefaultDelegate, Peripheral, BTLEDisconnectError, BTLEException
import crcmod
import csv
import struct
import time
import threading
import queue
#import run_node_relay_client
import random
from mqtt import MqttClient
from queue import Empty
import json

# UUIDs for Bluno Beetle services and characteristics
SERVICE_UUID = "0000dfb0-0000-1000-8000-00805f9b34fb" 
CHARACTERISTIC_UUID = "0000dfb1-0000-1000-8000-00805f9b34fb" 

HELLO_PACKET = b'H'
ACK_PACKET = b'A'
DATA_PACKET = b'D'
DATA_PACKET_SIZE = 20
PLAYER_ID = '2' #change accordingly player 1 or 2

GLOVE_FILE_NAME = ".csv"

#change this file name
SOCCER_FILE_NAME = "deen_null_121124_soccer2.csv"

RESET_COLOUR = "\033[0m"
DISCONNECT_COLOUR = "\033[1;41m"
CONNECTED_COLOUR = "\033[1;42m"
COLOUR_ID = {
    1: "\033[0;32m",
    2: "\033[0;33m",
    3: "\033[0;31m", 
    4: "\033[34m",
    5: "\033[0;32m",
    6: "\033[0;33m",
    7: "\033[0;31m",
    8: "\033[34m",
    9: "\033[0;39m",
}

#DEVICE ID Dict
DEVICE_ID = {
    "GLOVE_P1": 1,
    "GUN_P1": 2,
    "VEST_P1": 3,
    "LEG_P1": 4,
    "GLOVE_P2": 5,
    "GUN_P2": 6,
    "VEST_P2": 7,
    "LEG_P2": 8,
    "TEST": 9
}

DEVICE_NAME = {
    1: "GLOVE_P1",
    2: "GUN_P1",
    3: "VEST_P1",
    4: "LEG_P1",
    5: "GLOVE_P2",
    6: "GUN_P2",
    7: "VEST_P2",
    8: "LEG_P2",
    9: "TEST"
}

MAC_ADDRESSES = {
    "GLOVE_P1":  "F4:B8:5E:42:61:76", #Glove1 
    "GUN_P1": "F4:B8:5E:42:67:16", #Gun1 
    "VEST_P1": "B4:99:4C:89:1B:BD",  #Vest1 
    "LEG_P1": "F4:B8:5E:42:67:08", #Leg1 
    "GLOVE_P2": "F4:B8:5E:42:73:36", #Glove2
    "GUN_P2": "F4:B8:5E:42:6D:58", #Gun2
    "VEST_P2": "34:08:E1:28:16:C3", #VEST2 
    "LEG_P2": "F4:B8:5E:42:73:35", #LEG2
}

# Open IMU CSV file and make sure it is empty
imu_file_name = f'{GLOVE_FILE_NAME}'
open(imu_file_name, mode='a', newline='').close()
#imu_data_fields = ['PlayerID', 'DeviceID', 'AccX', 'AccY', 'AccZ', 'GyrX', 'GyrY', 'GyrZ', 'isDone'] # IMU CSV header

# Open Soccer CSV file and make sure it is empty
soccer_file_name = f'{SOCCER_FILE_NAME}'
open(soccer_file_name, mode='a', newline='').close()
#bullets_data_fields = ['player_id', 'bullets_count'] # vest CSV header

# Open Vest CSV file and make sure it is empty
vest_file_name = f'player_{PLAYER_ID}_vest.csv'
open(vest_file_name, mode='w', newline='').close()
#vest_data_fields = ['player_id', 'is_shot'] # vest CSV header

# Open Bullets CSV file and make sure it is empty
bullets_file_name = f'player_{PLAYER_ID}_bullets.csv'
open(bullets_file_name, mode='w', newline='').close()
#bullets_data_fields = ['player_id', 'bullets_count'] # bullet CSV header

activity = 0
activity_status = False

def get_user_input():
    global activity
    while True:
        user_input = input().strip()
        activity = user_input #toggle_value if activity == 0 else 0  # Toggle between 0 and the provided value
        print(f"Activity toggled to: {activity}")

# Create a lock object
lock = threading.Lock()

def save_IMU_Data(data):
    with lock:
        with open(imu_file_name, mode='a', newline='') as file:
            try:
                writer = csv.writer(file)
                writer.writerow([
                    #data["player_id"],  # Accessing the dictionary directly
                    #data["device_id"], 
                    data["count"],  
                    data["accX"], 
                    data["accY"], 
                    data["accZ"], 
                    data["gyrX"], 
                    data["gyrY"], 
                    data["gyrZ"],
                    #data["ema_acc"],
                    #data["ema_gyr"],
                    data["action_label"]
                ])
            except KeyError as e:
                print(f"KeyError encountered: {e}, skipping this data write.")

def save_vest_Data(vest_data):
    with open(vest_file_name, mode='a', newline='') as file:
            try:
                writer = csv.writer(file)
                writer.writerow([
                    vest_data["player_id"],  # Accessing the dictionary directly
                    vest_data["is_shot"]
                ])
            except KeyError as e:
                print(f"KeyError encountered: {e}, skipping this data write.")

def save_bullet_Data(bullet_data):
    with open(bullets_file_name, mode='a', newline='') as file:
            try:
                writer = csv.writer(file)
                writer.writerow([
                    bullet_data["player_id"],  # Accessing the dictionary directly
                    bullet_data["bullets_count"]
                ])
            except KeyError as e:
                print(f"KeyError encountered: {e}, skipping this data write.")

def save_soccer_Data(data):
    with open(soccer_file_name, mode='a', newline='') as file:
            try:
                writer = csv.writer(file)
                writer.writerow([
                    #data["player_id"],  # Accessing the dictionary directly
                    #data["device_id"], 
                    data["count"],
                    data["accX"], 
                    data["accY"], 
                    data["accZ"], 
                    data["gyrX"], 
                    data["gyrY"], 
                    data["gyrZ"],
                    #data["ema_acc"],
                    #data["ema_gyr"],
                    data["action_label"]
                ])
            except KeyError as e:
                print(f"KeyError encountered: {e}, skipping this data write.")

class BeetleDelegate(DefaultDelegate):
    global activity
    def __init__(self, deviceID, serialChar):
        DefaultDelegate.__init__(self)
        self.deviceID = deviceID
        self.serialChar = serialChar
        self.dataBuffer = b""
        self.packet = b""
        self.handshakeAck = False
        self.prev_seqNum = None
        self.start_time = time.time()
        self.end_time = None            


    def handleNotification(self, cHandle, data):
        self.dataBuffer += data

    def validCheckSum(self):
        crc8 = crcmod.predefined.mkCrcFun('crc-8')
        data = self.packet[:-1]  # Exclude the last byte (which is the checksum byte)
        calculated_crc = crc8(data)
        received_crc = self.packet[-1]
        #print(f"calculate_crc {calculated_crc}")
        return calculated_crc == received_crc

    def duplicatePacket(self):
        return self.prev_seqNum == self.packet[1]            

    def processData(self):
        if len(self.dataBuffer) >= DATA_PACKET_SIZE:
            self.packet = self.dataBuffer[0:DATA_PACKET_SIZE]
            self.dataBuffer = self.dataBuffer[DATA_PACKET_SIZE:]
            #print(f"Received packet: {self.packet.hex()}")
            packetType = chr(self.packet[0])
            if self.validCheckSum():
                self.dataBuffer = self.dataBuffer[DATA_PACKET_SIZE:]
                unpackedPkt = None
                packetFormat = None
                if not self.handshakeAck and packetType == 'A':
                    self.handshakeAck = True
                elif self.handshakeAck:
                    if packetType == 'V' and self.deviceID in (3,7):
                        packetFormat = 'bb?16xb'
                        if not self.duplicatePacket():
                            unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                            self.prev_seqNum = unpackedPkt[1]
                            dataMessage = {
                            "player_id" : PLAYER_ID,
                            "is_shot" : unpackedPkt[2]
                            }
                            save_vest_Data(dataMessage)
                            print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                            self.serialChar.write(ACK_PACKET)
                        else:
                            print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Received Duplicated Packet." + RESET_COLOUR)
                            self.packet = b""
                            self.serialChar.write(ACK_PACKET)                 
                    if packetType == 'G' and self.deviceID in (2,6):   
                        packetFormat = 'bbb16xb'
                        if not self.duplicatePacket():
                            unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                            self.prev_seqNum = unpackedPkt[1]
                            dataMessage = {
                            "player_id" : PLAYER_ID,
                            "bullets_count" : unpackedPkt[2],
                            "seq_num": unpackedPkt[1]
                            }
                            save_bullet_Data(dataMessage)
                            print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                            self.serialChar.write(ACK_PACKET)
                            #print("GUN DATA ACK PACKET IS SENT")
                        else:
                            print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Received Duplicated Packet." + RESET_COLOUR)
                            self.packet = b""
                            self.serialChar.write(ACK_PACKET)  
                    if packetType == 'D' and self.deviceID in (1, 4, 5, 8):
                        packetFormat = 'bb8hxb'
                        unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                        dataMessage = {
                            "player_id": PLAYER_ID,
                            "device_id": self.deviceID,
                            "accX": unpackedPkt[2],
                            "accY": unpackedPkt[3],
                            "accZ": unpackedPkt[4],
                            "gyrX": unpackedPkt[5],
                            "gyrY": unpackedPkt[6],
                            "gyrZ": unpackedPkt[7],
                            "ema_acc": unpackedPkt[8],
                            "ema_gyr": unpackedPkt[9],
                            "count": unpackedPkt[1],
                            "action_label": activity,
                        }
                        #send to game engine
                        if self.deviceID in (1,5):
                            save_IMU_Data(dataMessage)
                        else:
                            save_soccer_Data(dataMessage)
                        print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                    if packetType == 'S' and self.deviceID in (4,8):
                        packetFormat = 'bb17xb'
                        if not self.duplicatePacket():
                            unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                            self.prev_seqNum = unpackedPkt[1]
                            dataMessage = {
                            "player_id": PLAYER_ID,
                            "device_id": self.deviceID,
                            "soccer": "soccer",
                            "seq_num": unpackedPkt[1]
                            }
                            save_soccer_Data(dataMessage)
                            print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                            self.serialChar.write(ACK_PACKET)
                            #print("SOCCER DATA ACK PACKET IS SENT")
                        else:
                            print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Received Duplicated Packet." + RESET_COLOUR)
                            self.packet = b""
                            self.serialChar.write(ACK_PACKET) 
            else:
                ##not valid DATA 
                self.dataBuffer = b""
                print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Data packet is corrupted" + RESET_COLOUR)
                print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Flushing corrupted buffer." + RESET_COLOUR)
        else:
            print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Assembling packet." + RESET_COLOUR)
            #print(f"Number of fragmented packets: {self.fragmentedPktCount}")


    def updatePlayerData(self, player_hp, player_bullet):
        if self.deviceID in (3,7): #send to vest1: 3, vest2: 7 
            self.serialChar.write(bytes([player_hp]))
            print(f"{COLOUR_ID[self.deviceID]}Updating Player {PLAYER_ID} Vest HP: {player_hp} {RESET_COLOUR}")
        if self.deviceID in (2,6): #send to gun1: 2, gun2: 6
            self.serialChar.write(bytes([player_bullet]))
            print(f"{COLOUR_ID[self.deviceID]}Updating Player {PLAYER_ID} Bullets Count:{player_bullet} {RESET_COLOUR}")

class Beetle():
    def __init__(self, deviceID, mac_address):
        self.deviceID = deviceID
        self.mac_address = mac_address
        self.peripheral = None
        self.beetleDelegate = None
        self.isConnected = False
        self.isSetup = False
        self.handshaken = False
        self.serialSvc = None
        self.serialChar = None

    def startConnection(self, mqtt_client):
        self.isConnected = False
        self.handshaken = False
        self.isSetup = False

        while not self.isConnected:
            try:
                print(f"{COLOUR_ID[self.deviceID]}Connecting to {DEVICE_NAME[self.deviceID]} at {self.mac_address}. {RESET_COLOUR}")
                self.peripheral = Peripheral(self.mac_address)
                self.isConnected = True
                print(f"{CONNECTED_COLOUR}{DEVICE_NAME[self.deviceID]} is connected. {RESET_COLOUR}")              
            except BTLEException:
                print(f"{DISCONNECT_COLOUR}{DEVICE_NAME[self.deviceID]} Reconnection failed, retrying reconnection...{RESET_COLOUR}")
                time.sleep(1)
            except BTLEDisconnectError:
                print(f"{DISCONNECT_COLOUR}{DEVICE_NAME[self.deviceID]} is disconnected when initiating connection.{RESET_COLOUR}")
                self.setupBeetle(mqtt_client)
                if self.isConnected:
                    self.startHandshake(mqtt_client)
        if self.isConnected == False:
            print(f"{DEVICE_NAME[self.deviceID]} cannot be connected.")
    
    def setupBeetle(self, mqtt_client):
        try:
            self.startConnection(mqtt_client)
            self.handshaken = False

            if self.isConnected == True:
                print(f"Setting up {DEVICE_NAME[self.deviceID]}.")
                self.serialSvc = self.peripheral.getServiceByUUID(SERVICE_UUID)
                self.serialChar = self.serialSvc.getCharacteristics(CHARACTERISTIC_UUID)[0]
                self.beetleDelegate = BeetleDelegate(self.deviceID, self.serialChar)
                self.peripheral.withDelegate(self.beetleDelegate)
                print(f"{COLOUR_ID[self.deviceID]}{DEVICE_NAME[self.deviceID]} setup completed. {RESET_COLOUR}")
                self.isSetup = True
        except BTLEDisconnectError:
                print(f"{DISCONNECT_COLOUR}{DEVICE_NAME[self.deviceID]} is disconnected during setup. {RESET_COLOUR}")
                self.setupBeetle(mqtt_client)
                if self.isConnected:
                    self.startHandshake(mqtt_client)

    def startHandshake(self, mqtt_client):
        self.handshaken = False
        try:
            while self.handshaken == False:
                print(f"Starting Handshake with {DEVICE_NAME[self.deviceID]}....")
                self.beetleDelegate.dataBuffer = b""
                self.serialChar.write(HELLO_PACKET)
                print(f"{COLOUR_ID[self.deviceID]}Send hello packet {DEVICE_NAME[self.deviceID]}")
                if self.peripheral.waitForNotifications(1):
                    self.beetleDelegate.processData()
                    if self.beetleDelegate.handshakeAck == True:
                        print(f"{CONNECTED_COLOUR}Handshake Completed with {DEVICE_NAME[self.deviceID]}{RESET_COLOUR}")
                        self.handshaken = True
                        self.serialChar.write(ACK_PACKET)
                        topic = f"visualiser_{PLAYER_ID}/device/{DEVICE_NAME[self.deviceID]}"
                        device_connection = "true"
                        mqtt_client.publish_to_topic(topic, device_connection)
        except BTLEDisconnectError:
            print(f"{DISCONNECT_COLOUR}{DEVICE_NAME[self.deviceID]} is disconnected during handshake.{RESET_COLOUR}")
            self.setupBeetle(mqtt_client)


    def runBeetle(self, player_data_queue, mqtt_client):
        while True:
            try:
                if not self.handshaken:
                    self.setupBeetle(mqtt_client)
                    if self.isConnected:
                        self.startHandshake(mqtt_client)
                else:
                    if self.peripheral.waitForNotifications(1):
                        self.beetleDelegate.processData()
                    if player_data_queue != "NIL":
                        try:
                            player_data = player_data_queue.get_nowait()
                            player_hp = int(player_data['health'])
                            player_bullet = int(player_data['bullets_count']) 
                            self.beetleDelegate.updatePlayerData(player_hp, player_bullet) 
                        except (ValueError, KeyError):
                            print("Invalid player_data received, skipping this data.")
                            continue
                        except Empty:
                            continue
            except BTLEDisconnectError:
                print(f"{DISCONNECT_COLOUR}{DEVICE_NAME[self.deviceID]} is disconnected.{RESET_COLOUR}")
                topic = f"visualiser_{PLAYER_ID}/device/{DEVICE_NAME[self.deviceID]}"
                device_connection = "false"
                mqtt_client.publish_to_topic(topic, device_connection)
                self.setupBeetle(mqtt_client)
                if self.isConnected:
                    self.startHandshake(mqtt_client)  
            except BTLEException as e:
                print(f"{DEVICE_NAME[self.deviceID]} Error: {str(e)}.")
                self.setupBeetle(mqtt_client)
                if self.isConnected:
                    self.startHandshake(mqtt_client) 
            except Exception as e:
                print(f"{DEVICE_NAME[self.deviceID]} 123 Error: {str(e)}.")
                self.setupBeetle(mqtt_client)
                if self.isConnected:
                    self.startHandshake(mqtt_client)

if __name__ == "__main__":
    try:
        #player_health_queue = queue.Queue()
        #player_bullets_queue = queue.Queue()
        
        mqtt_client = MqttClient()
        mqtt_client.start_client()

         # Start the user input thread to capture activity toggle
        user_input_thread = threading.Thread(target=get_user_input,  daemon=True)
        user_input_thread.start()

        #set up ecomm
        #ecommThread = threading.Thread(target=run_node_relay_client.main, args=(player_health_queue, player_bullets_queue,))
        #ecommThread.start()

        #gloveP2_Beetle = Beetle(DEVICE_ID["GLOVE_P2"], MAC_ADDRESSES["GLOVE_P2"])
        #gloveP2_Thread = threading.Thread(target= gloveP2_Beetle.runBeetle, args=("NIL", mqtt_client,))
        #gloveP2_Thread.start()

        legP2_Beetle = Beetle(DEVICE_ID["LEG_P2"], MAC_ADDRESSES['LEG_P2'])
        legP2_Thread = threading.Thread(target=legP2_Beetle.runBeetle, args=("NIL", mqtt_client,))
        legP2_Thread.start()

        #vestP2_Beetle = Beetle(DEVICE_ID["VEST_P2"], MAC_ADDRESSES["VEST_P2"])
        #vestP2_Thread = threading.Thread(target=vestP2_Beetle.runBeetle, args=(player_health_queue, mqtt_client,))

        #gunP2_Beetle = Beetle(DEVICE_ID["GUN_P2"], MAC_ADDRESSES["GUN_P2"])
        #gunP2_Thread = threading.Thread(target=gunP2_Beetle.runBeetle, args=(player_bullets_queue, mqtt_client,))

        
        #vestP2_Thread.start()
        #gunP2_Thread.start()
        
        
    except (KeyboardInterrupt):
        print("END INTERNAL COMMUNICATIONS")
        #ecommThread.join()
        #gloveP2_Thread.join()
        #vestP2_Thread.join()
        #gunP2_Thread.join()
        legP2_Thread.join()
        user_input_thread.join()