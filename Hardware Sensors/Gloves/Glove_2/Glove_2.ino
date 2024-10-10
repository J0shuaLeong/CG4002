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

void getIMUData(int16_t* features_min, int16_t* features_max, int16_t* features_mean, int16_t* features_std) {
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
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);

  // Set filter bandwidth to 21 Hz for both accelerometer and gyroscope
  // Noise reduction: higher bandwidth (260) = less filtering, faster response
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  // Give the sensor some time to stabilize
  delay(100);
} 

int count = 0;
bool isAction = 0;
// 2d array of 6, 30 to store the sensor data
int16_t sensorData[6][30];

int16_t* getmin(int16_t sensorData[6][30]) {
    int16_t min[6];
    for (int i = 0; i < 6; i++) {
        min[i] = sensorData[i][0];
        for (int j = 1; j < 30; j++) {
            if (sensorData[i][j] < min[i]) {
                min[i] = sensorData[i][j];
            }
        }
    }
    return min;
}

int16_t* getmax(int16_t sensorData[6][30]) {
    int16_t max[6];
    for (int i = 0; i < 6; i++) {
        max[i] = sensorData[i][0];
        for (int j = 1; j < 30; j++) {
            if (sensorData[i][j] > max[i]) {
                max[i] = sensorData[i][j];
            }
        }
    }
    return max;
}

int16_t* getmean(int16_t sensorData[6][30]) {
    int16_t mean[6];
    for (int i = 0; i < 6; i++) {
        mean[i] = 0;
        for (int j = 0; j < 30; j++) {
            mean[i] += sensorData[i][j];
        }
        mean[i] = mean[i] / 30;
    }
    return mean;
}

int16_t* getstd(int16_t sensorData[6][30]) {
    int16_t std[6];
    int16_t mean[6];
    mean = getmean(sensorData);
    for (int i = 0; i < 6; i++) {
        std[i] = 0;
        for (int j = 0; j < 30; j++) {
            std[i] += pow(sensorData[i][j] - mean[i], 2);
        }
        std[i] = sqrt(std[i] / 30);
    }
    return std;
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
    accZ = 100 * abs(a.acceleration.z) - 0.4 - 9.81;

    // Store gyroscope values in float variables
    gyroX = g.gyro.x * 100;
    gyroY = g.gyro.y * 100;
    gyroZ = g.gyro.z * 100;

    Serial.print("accX:"); Serial.print(accX); Serial.print(",");
    Serial.print("accY:"); Serial.print(accY); Serial.print(",");
    Serial.print("accZ:"); Serial.print(accZ); Serial.print(",");

    // Serial.print("gyroX:"); Serial.print(gyroX); Serial.print(",");
    // Serial.print("gyroY:"); Serial.print(gyroY); Serial.print(",");
    // Serial.print("gyroZ:"); Serial.println(gyroZ);

    // Apply these thresholds c[(c["AccY"] > 1500) | (c["AccZ"] < -2000) | (c["GyrY"] > 500) | (c["AccX"] > 2500) | (c["GyrZ"] < -500) | (c["AccY"] < -500)]
    if (accY > 1500 || accZ < -2000 || gyroY > 500 || accX > 2500 || gyroZ < -500 || accY < -500) {
        isAction = 1;
    }
    if (isAction == 1) {
        sensorData[0][count] = accX;
        sensorData[1][count] = accY;
        sensorData[2][count] = accZ;
        sensorData[3][count] = gyroX;
        sensorData[4][count] = gyroY;
        sensorData[5][count] = gyroZ;
        count += 1;
    }

    int16_t* features_min;
    int16_t* features_max;
    int16_t* features_mean;
    int16_t* features_std;

    if (count == 30) {
        features_min = getmin(sensorData);
        features_max = getmax(sensorData);
        features_mean = getmean(sensorData);
        features_std = getstd(sensorData);
        isAction = 0;
    }
    //unsigned long elapsedTime = millis() - startTime;
    char incomingResponse = Serial.read();
    if (incomingResponse == HELLO_PACKET) {
      handshakeDone = false;
      sendAckPkt();
      break;
    }
    
    getIMUData(features_min, features_max, features_mean, features_std);
    delay(100);
  }
}
