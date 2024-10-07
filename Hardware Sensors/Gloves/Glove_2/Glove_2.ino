#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include "CRC.h"
#include "CRC8.h"

#define HELLO_PACKET 'H'
#define ACK_PACKET 'A'
#define DATA_PACKET 'D' 
#define SIZE_OF_PACKET 20

Adafruit_MPU6050 mpu;
bool handshakeDone;
CRC8 crc;
byte sendBuffer[20];
unsigned long startTime = 0;

struct AckPacket {
    byte typeOfPacket = ACK_PACKET;
    byte seqNum = 0;
    byte padding[17] = {0};
    byte checkSum;
};

struct DataPacket {
  byte packetType;
  byte deviceID;
  int16_t accX;
  int16_t accY;
  int16_t accZ;
  int16_t gyrX;
  int16_t gyrY;
  int16_t gyrZ;
  byte padding[5] = {0};
  byte checkSum;    
};

void sendAckPkt() {
  AckPacket ackPkt;
  crc.reset();
  crc.add((byte*)&ackPkt, SIZE_OF_PACKET - sizeof(ackPkt.checkSum));
  ackPkt.checkSum = crc.getCRC();
  memcpy(sendBuffer, &ackPkt, sizeof(ackPkt));
  Serial.write(sendBuffer, SIZE_OF_PACKET);
  memset(sendBuffer, 0, sizeof(sendBuffer));  // Set all elements in sendBuffer to 0
}    

void getIMUData(int16_t accX, int16_t accY, int16_t accZ, int16_t gyrX, int16_t gryY, int16_t gryZ ) {
  DataPacket dataPkt;
  dataPkt.packetType = DATA_PACKET;
  dataPkt.deviceID = '1';
  dataPkt.accX = accX;
  dataPkt.accY = accY;
  dataPkt.accZ = accZ;
  dataPkt.gyrX = gyrX;
  dataPkt.gyrY = gryY;
  dataPkt.gyrZ = gryZ;
  crc.reset();
  crc.add((byte*)&dataPkt, SIZE_OF_PACKET - sizeof(dataPkt.checkSum));
  dataPkt.checkSum = crc.getCRC();
  // int randomFailChance = random(0, 10);
  //   if (randomFailChance == 9) {
  //     dataPkt.checkSum ^= 0xFF;
  //   }
  memcpy(sendBuffer, &dataPkt, sizeof(dataPkt));
  Serial.write(sendBuffer, SIZE_OF_PACKET);
  memset(sendBuffer, 0, sizeof(sendBuffer));
  //Serial.println("sending dummy data");
}

void setup(void) {
  Serial.begin(115200);
  handshakeDone = false;
  // Initialize I2C communication with the MPU6050 sensor
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip");
    while (1) {
      delay(10);
    }
  }

  // Maximum measurable acceleration
  // Lowest setting (2G) = least sensitive, highest resolution
  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);

  // Maximum measurable rotation rate
  // lowest maximum rotation rate (250) -> highest sensitivity
  mpu.setGyroRange(MPU6050_RANGE_1000_DEG);

  // Set filter bandwidth to 21 Hz for both accelerometer and gyroscope
  // Noise reduction: higher bandwidth (260) = less filtering, faster response
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  // Give the sensor some time to stabilize
  delay(100);
} 

void loop() {
  // Send the accelerometer and gyroscope data to the Serial Plotter
    // Serial.print("accX:"); Serial.print(accX); Serial.print(",");
    // Serial.print("accY:"); Serial.print(accY); Serial.print(",");
    // Serial.print("accZ:"); Serial.print(accZ); Serial.print(",");

    // Serial.print("gyroX:"); Serial.print(gyroX); Serial.print(",");
    // Serial.print("gyroY:"); Serial.print(gyroY); Serial.print(",");
    // Serial.print("gyroZ:"); Serial.println(gyroZ);

    
  if (Serial.available()) {
    char incomingResponse = Serial.read();
    if (incomingResponse == HELLO_PACKET) {
      handshakeDone = false;
      sendAckPkt();
    } else if (incomingResponse == ACK_PACKET) {
      handshakeDone = true;
      //startTime = millis();
      //Serial.println("handshake done");
    } 
  }
  while (handshakeDone) {
    // Declare float variables to store the sensor readings
    int16_t accX, accY, accZ;  // For accelerometer values
    int16_t gyroX, gyroY, gyroZ;  // For gyroscope values
    // Get new sensor events for accelerometer and gyroscope
    sensors_event_t a, g, temp;
    mpu.getEvent(&a, &g, &temp);

    // Store accelerometer values in float variables
    accX = a.acceleration.x * 100;
    accY = a.acceleration.y * 100;
    accZ = 100 * a.acceleration.z - 0.4;

    // Store gyroscope values in float variables
    gyroX = g.gyro.x * 100;
    gyroY = g.gyro.y * 100;
    gyroZ = g.gyro.z * 100;
    //unsigned long elapsedTime = millis() - startTime;
    char incomingResponse = Serial.read();
    if (incomingResponse == HELLO_PACKET) {
      handshakeDone = false;
      sendAckPkt();
      break;
    }
    
    getIMUData(accX, accY, accZ, gyroX, gyroY, gyroZ);
    delay(100);
  }
}
