import argparse
from bluepy.btle import DefaultDelegate, Peripheral, BTLEDisconnectError, BTLEException
import crcmod
import csv
import struct
import time
import threading

# UUIDs for Bluno Beetle services and characteristics
SERVICE_UUID = "0000dfb0-0000-1000-8000-00805f9b34fb" 
CHARACTERISTIC_UUID = "0000dfb1-0000-1000-8000-00805f9b34fb" 

glove1_mac = "F4:B8:5E:42:67:08" #glove1
gun1_mac = "F4:B8:5E:42:6D:58" #gun1
vest1_mac = "F4:B8:5E:42:73:36" #vest1

HELLO_PACKET = b'H'
ACK_PACKET = b'A'
DATA_PACKET = b'D'
DATA_PACKET_SIZE = 20
NUM_OF_RETRIES = 5000

RESET_COLOUR = "\033[0m"
COLOUR_ID = {
    1: "\033[0;32m", #glove1
    2: "\033[0;33m", #gun1
    3: "\033[0;31m", #vest1
    4: "\033[35m"
}

DEVICE_ID = {
    "GLOVE_P1": 1,
    "GUN_P1": 2,
    "VEST_P1": 3,
    ""
    "GLOVE_P2": 5,
    "GUN_P2": 4,
    "VEST_P2": 6
}

DEVICE_NAME = {
    1: "GLOVE_P1",
    2: "GUN_P1",
    3: "VEST_P1",
    5: "GLOVE_P2",
    4: "GUN_P2",
    6: "VEST_P2"
}

MAC_ADDRESSES = {
    "GLOVE_P1": "F4:B8:5E:42:73:36", #Glove2 "F4:B8:5E:42:73:35", #Glove1
    "GUN_P1": "F4:B8:5E:42:6D:58", #Gun1
    "VEST_P1": "B4:99:4C:89:1B:BD",  #Vest1
    "GLOVE_P2": "F4:B8:5E:42:73:36", #Glove2
    "VEST_P2": "F4:B8:5E:42:73:36" #Vest2
}

activity = 0
activity_status = False

def get_user_input(toggle_value):
    global activity
    while True:
        input()  # Wait for the user to press Enter
        activity = toggle_value if activity == 0 else 0  # Toggle between 0 and the provided value
        print(f"Activity toggled to: {activity}")

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
        self.packetCount = 0
        self.corruptPktCount = 0
        self.numofcrptedpkt = 0
        self.fragmentedPktCount = 0
        self.pktdropCount = 0
        self.start_time = time.time()
        self.end_time = None
        self.total_bytes_received = 0
        self.last_speed_time = self.start_time

        # Open CSV file to store IMU data
        if self.deviceID in (1,5):  # Only for Glove 1 and Glove 2
            self.csv_file = open('thang_reload_221024.csv', mode='w', newline='')
            self.csv_writer = csv.writer(self.csv_file)
            self.csv_writer.writerow(['Count', 'AccX', 'AccY', 'AccZ', 'GyrX', 'GyrY', 'GyrZ', 'Ema_Acc', 'Ema_Gyr', 'Activity'])  # CSV header

    def closeCSV(self):
        if self.deviceID == DEVICE_ID["GLOVE_P1"]:
            self.csv_file.close()

    def handleNotification(self, cHandle, data):
        self.dataBuffer += data
        self.total_bytes_received += len(data)

    def validCheckSum(self):
        crc8 = crcmod.predefined.mkCrcFun('crc-8')
        data = self.packet[:-1]  # Exclude the last byte (which is the checksum byte)
        calculated_crc = crc8(data)
        received_crc = self.packet[-1]
        return calculated_crc == received_crc

    def processData(self):
        try:
            if len(self.dataBuffer) >= DATA_PACKET_SIZE:
                self.packet = self.dataBuffer[0:DATA_PACKET_SIZE]
                self.dataBuffer = self.dataBuffer[DATA_PACKET_SIZE:]
                #print(f"Received packet: {self.packet.hex()}")
                packetType = chr(self.packet[0])
                if self.validCheckSum():
                    self.dataBuffer = self.dataBuffer[DATA_PACKET_SIZE:]
                    unpackedPkt = None
                    packetFormat = None
                    self.packetCount += 1 
                    if not self.handshakeAck and packetType == 'A':
                        self.handshakeAck = True
                    elif self.handshakeAck:
                        #print(f"{DEVICE_NAME[self.deviceID]}: Packet Count = {self.packetCount}")
                        if packetType == 'V' and self.deviceID in (3,6):
                            packetFormat = 'bbbi12xb'
                            if not self.duplicatePacket():
                                unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                                self.prev_seqNum = unpackedPkt[2]
                                dataMessage = {
                                "deviceID" : self.deviceID,
                                "seqNum" : unpackedPkt[2],
                                "HP" : unpackedPkt[3]
                                }
                                print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                                self.serialChar.write(ACK_PACKET)
                            else:
                                print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Received Duplicated Packet." + RESET_COLOUR)
                                self.pa#writing to CSV make sure all the features in IMU is insidecket = b""
                                self.pktdropCount += 1
                                self.serialChar.write(ACK_PACKET)                 
                        if packetType == 'G' and self.deviceID in (2,5):   
                            packetFormat = 'bbbb15xb'
                            if not self.duplicatePacket():
                                unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                                self.prev_seqNum = unpackedPkt[2]
                                dataMessage = {
                                "deviceID" : self.deviceID,
                                "seqNum" : unpackedPkt[2],
                                "BulletCount" : unpackedPkt[3]
                                }
                                #print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                                self.serialChar.write(ACK_PACKET)
                                #print("GUN DATA ACK PACKET IS SENT")
                            else:
                                print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Received Duplicated Packet." + RESET_COLOUR)
                                self.serialChar.write(ACK_PACKET)
                                self.packet = b""
                                self.pktdropCount += 1
                                self.serialChar.write(ACK_PACKET)  
                        if packetType == 'D' and self.deviceID in (1,5):
                            packetFormat = 'b1x7h3xb'
                            unpackedPkt = struct.unpack_from(packetFormat, self.packet, 0)
                            dataMessage = {
                                "deviceID" : self.deviceID,
                                "count": unpackedPkt[2],
                                #writing to CSV make sure all the features in IMU is inside CSV writerow
                                "imuData": {
                                    "accX": unpackedPkt[3],
                                    "accY": unpackedPkt[4],
                                    "accZ": unpackedPkt[5],
                                    "gyrX": unpackedPkt[6],
                                    "gyrY": unpackedPkt[7],
                                    "gyrZ": unpackedPkt[8],
                                    "activity": activity                            
                                }
                            }
                            #send to game engine
                            #if activity_status:
                            
                            self.csv_writer.writerow([unpackedPkt[1], unpackedPkt[2], unpackedPkt[3], unpackedPkt[4], unpackedPkt[5], unpackedPkt[6], unpackedPkt[7], unpackedPkt[8], unpackedPkt[9], activity])
                            print(f"{COLOUR_ID[self.deviceID]}" + str(dataMessage) + RESET_COLOUR)
                else:
                    ##not valid DATA
                    self.corruptPktCount += 1
                    self.numofcrptedpkt +=1
                    self.packetCount += 1  
                    self.dataBuffer = b""
                    self.pktdropCount += 1
                    print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Data packet is corrupted" + RESET_COLOUR)
                    print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Flushing corrupted buffer." + RESET_COLOUR)
                    #if self.corruptPktCount >= 3:
                    #    self.corruptPktCount = 0
                    #    self.dataBuffer = b""
                    #    self.pktdropCount += 1time.sleep(5)
                    #    print(f"{DEVICE_NAME[self.deviceID]}: Flushing corrupted buffer.")
            else:
                self.fragmentedPktCount += 1
                print(f"{COLOUR_ID[self.deviceID]}" + f"{DEVICE_NAME[self.deviceID]}: Assembling packet." + RESET_COLOUR)
                #print(f"Number of fragmented packets: {self.fragmentedPktCount}")
        except BTLEDisconnectError:
                print(f"{DEVICE_NAME[self.deviceID]} is disconnected.")
                self.setupBeetle()
                if self.isConnected:
                    self.startHandshake()

    # Destructor method to call closeCSV when the object is destroyed
    def __del__(self):
        self.closeCSV()


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
        self.connectionRetries = None

    def startConnection(self):
        self.isConnected = False
        self.handshaken = False
        self.isSetup = False
        self.connectionRetries = NUM_OF_RETRIES
        retry_delay = 1

        while self.connectionRetries > 0 and not self.isConnected:
            try:
                print(f"Connecting to {DEVICE_NAME[self.deviceID]} at {self.mac_address} with {self.connectionRetries} reconnection attempts left.")
                self.peripheral = Peripheral(self.mac_address)
                self.isConnected = True
                print(f"{DEVICE_NAME[self.deviceID]} is connected.")              
            except BTLEException as e:
                self.connectionRetries -= 1  
                print(f"{DEVICE_NAME[self.deviceID]} Reconnection failed, retrying in {retry_delay} seconds... Error: {str(e)}")
                time.sleep(1)

        if self.isConnected == False:
            print(f"{DEVICE_NAME[self.deviceID]} cannot be connected.")
    
    def setupBeetle(self):
        self.startConnection()

        if self.isConnected == True:
            print(f"Setting up {DEVICE_NAME[self.deviceID]}.")
            self.serialSvc = self.peripheral.getServiceByUUID(SERVICE_UUID)
            self.serialChar = self.serialSvc.getCharacteristics(CHARACTERISTIC_UUID)[0]
            self.beetleDelegate = BeetleDelegate(self.deviceID, self.serialChar)
            self.peripheral.withDelegate(self.beetleDelegate)
            print(f"{DEVICE_NAME[self.deviceID]} setup completed.")
            self.isSetup = True

    def startHandshake(self):
        self.handshaken = False
        try:
            while self.handshaken == False:
                print(f"Starting Handshake with {DEVICE_NAME[self.deviceID]}....")
                self.beetleDelegate.dataBuffer = b""
                self.serialChar.write(HELLO_PACKET)
                if self.peripheral.waitForNotifications(10):
                    self.beetleDelegate.processData()
                    if self.beetleDelegate.handshakeAck == True:
                        print(f"Handshake Completed with {DEVICE_NAME[self.deviceID]}")
                        self.handshaken = True
                        self.serialChar.write(ACK_PACKET)
        except BTLEDisconnectError:
            print(f"{DEVICE_NAME[self.deviceID]} is disconnected during handshake.")
            self.setupBeetle()

    def runBeetle(self):
        while True:
            try:
                if not self.handshaken:
                    self.setupBeetle()
                    if self.isConnected:
                        self.startHandshake()
                else:
                    if self.peripheral.waitForNotifications(1000):
                        self.beetleDelegate.processData()

            except BTLEDisconnectError:
                print(f"{DEVICE_NAME[self.deviceID]} is disconnected.")
                self.setupBeetle()
                if self.isConnected:
                    self.startHandshake() 
            except Exception as e:
                print(f"{DEVICE_NAME[self.deviceID]} Error: {str(e)}.")
                self.setupBeetle()
                if self.isConnected:
                    self.startHandshake()


if __name__ == "__main__":
    # Parse the command-line arguments
    parser = argparse.ArgumentParser(description="Activity switcher script")
    parser.add_argument('toggle_value', type=int, help='Value to toggle activity between 0 and this value')

    args = parser.parse_args()

    try:
        # Start the user input thread to capture activity toggle
        user_input_thread = threading.Thread(target=get_user_input, args=(args.toggle_value,), daemon=True)
        user_input_thread.start()

        gloveP1_Beetle = Beetle(DEVICE_ID["GLOVE_P1"], MAC_ADDRESSES["GLOVE_P1"])
        gloveP1_Thread = threading.Thread(target=gloveP1_Beetle.runBeetle, args=())

        gloveP1_Thread.start()
        gloveP1_Thread.join()

    except (KeyboardInterrupt, SystemExit):
        print("END INTERNAL COMMUNICATIONS")
