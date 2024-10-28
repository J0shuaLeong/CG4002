#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>
#include "CRC.h"
#include "CRC8.h"


#define HELLO_PACKET 'H'
#define ACK_PACKET 'A'
#define DATA_PACKET 'D' 
#define SIZE_OF_PACKET 20

// Declare float variables to store the sensor readings
float accX, accY, accZ;  // For accelerometer values
float grav_accX, grav_accY, grav_accZ; // With gravity
float gyroX, gyroY, gyroZ;  // For gyroscope values
sensors_event_t a, g, temp;
int16_t dataArray[60];
int arrayIndex = 0;


template <int order> // order is 1 or 2
class HighPass
{
  private:
    float a[order];
    float b[order+1];
    float omega0;
    float dt;
    bool adapt;
    float tn1 = 0;
    float x[order+1]; // Raw values
    float y[order+1]; // Filtered values

  public:  
    HighPass(float f0, float fs, bool adaptive){
      // f0: cutoff frequency (Hz)
      // fs: sample frequency (Hz)
      // adaptive: boolean flag, if set to 1, the code will automatically set
      // the sample frequency based on the time history.
      
      omega0 = 6.28318530718*f0;
      dt = 1.0/fs;
      adapt = adaptive;
      tn1 = -dt;
      for(int k = 0; k < order+1; k++){
        x[k] = 0;
        y[k] = 0;        
      }
      setCoef();
    }

    void setCoef(){
      if(adapt){
        float t = micros()/1.0e6;
        dt = t - tn1;
        tn1 = t;
      }
      
      float alpha = omega0*dt;
      if(order==1){
        float alphaFactor = 1/(1 + alpha/2.0);
        a[0] = -(alpha/2.0 - 1)*alphaFactor;
        b[0] = alphaFactor;
        b[1] = -alphaFactor;      
      }
      if(order==2){
        float alpha = omega0*dt;
        float dtSq = dt*dt;
        float c[] = {omega0*omega0, sqrt(2)*omega0, 1};
        float D = c[0]*dtSq + 2*c[1]*dt + 4*c[2];
        b[0] = 4.0/D;
        b[1] = -8.0/D;
        b[2] = 4.0/D;
        a[0] = -(2*c[0]*dtSq - 8*c[2])/D;
        a[1] = -(c[0]*dtSq - 2*c[1]*dt + 4*c[2])/D;   
      }
    }

    float filt(float xn){
      // Provide me with the current raw value: x
      // I will give you the current filtered value: y
      if(adapt){
        setCoef(); // Update coefficients if necessary      
      }
      y[0] = 0;
      x[0] = xn;
      // Compute the filtered values
      for(int k = 0; k < order; k++){
        y[0] += a[k]*y[k+1] + b[k]*x[k];
      }
      y[0] += b[order]*x[order];

      // Save the historical values
      for(int k = order; k > 0; k--){
        y[k] = y[k-1];
        x[k] = x[k-1];
      }
  
      // Return the filtered value    
      return y[0];
    }
};

// Filter instance
HighPass<2> lpX(1,1e3,true);
HighPass<2> lpY(1,1e3,true);
HighPass<2> lpZ(1,1e3,true);

Adafruit_MPU6050 mpu;
bool handshakeDone;
CRC8 crc;
byte sendBuffer[20];
unsigned long startTime = 0;
int count = -10;

struct AckPacket {
    byte typeOfPacket = ACK_PACKET;
    byte seqNum = 0;
    byte padding[17] = {0};
    byte checkSum;
};

struct DataPacket {
  byte packetType;
  byte padding = {0};
  int16_t count;
  int16_t accX;
  int16_t accY;
  int16_t accZ;
  int16_t gyrX;
  int16_t gyrY;
  int16_t gyrZ;
  byte paddings[3] = {0};
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

void getIMUData(int16_t accX, int16_t accY, int16_t accZ, int16_t gyrX, int16_t gryY, int16_t gryZ) {
  DataPacket dataPkt;
  dataPkt.packetType = DATA_PACKET;
  dataPkt.count = count;
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
  mpu.setAccelerometerRange(MPU6050_RANGE_4_G);

  // Maximum measurable rotation rate
  // lowest maximum rotation rate (250) -> highest sensitivity
  mpu.setGyroRange(MPU6050_RANGE_1000_DEG);

  // Set filter bandwidth to 21 Hz for both accelerometer and gyroscope
  // Noise reduction: higher bandwidth (260) = less filtering, faster response
  mpu.setFilterBandwidth(MPU6050_BAND_44_HZ);

  // Give the sensor some time to stabilize
  for (int i = 0; i < 80; i++) {
    mpu.getEvent(&a, &g, &temp);

    // Store accelerometer values in float variables
    grav_accX = a.acceleration.x * 100;
    grav_accY = a.acceleration.y * 100;
    grav_accZ = a.acceleration.z * 100;

    // Store gyroscope values in float variables
    gyroX = g.gyro.x * 100;
    gyroY = g.gyro.y * 100;
    gyroZ = g.gyro.z * 100;


    // Compute the filtered signal
    accX = lpX.filt(grav_accX);
    accY = lpY.filt(grav_accY);
    accZ = lpZ.filt(grav_accZ); 
    delay(25);
  }
  delay(100);
} 


bool isAction = false;
float ema_acc = 0;
float ema_gyr = 0;
float smoothing = 0.9;


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
    // Get new sensor events for accelerometer and gyroscope
    mpu.getEvent(&a, &g, &temp);

    // Store accelerometer values in float variables
    grav_accX = a.acceleration.x * 100;
    grav_accY = a.acceleration.y * 100;
    grav_accZ = a.acceleration.z * 100;

    // Store gyroscope values in float variables
    gyroX = g.gyro.x * 100;
    gyroY = g.gyro.y * 100;
    gyroZ = g.gyro.z * 100;


    // Compute the filtered signal
    accX = lpX.filt(grav_accX);
    accY = lpY.filt(grav_accY);
    accZ = lpZ.filt(grav_accZ);


    //unsigned long elapsedTime = millis() - startTime;
    char incomingResponse = Serial.read();
    if (incomingResponse == HELLO_PACKET) {
      handshakeDone = false;
      sendAckPkt();
      break;
    }

    float mag_acc = sqrt(pow(accX, 2) + pow(accY, 2) + pow(accZ, 2));
    float mag_gyr = abs(sqrt(pow(gyroX, 2) + pow(gyroY, 2) + pow(gyroZ, 2)));
    ema_acc = smoothing * ema_acc + (1 - smoothing) * mag_acc;
    ema_gyr = smoothing * ema_gyr + (1 - smoothing) * mag_gyr;
    // reload + shield + bowling
    if (abs(accY) > 2000) {
      isAction = true;
    }
    if (isAction) {
      count += 1;
      if (count == 55) {
        getIMUData(int16_t(dataArray[arrayIndex]), int16_t(dataArray[arrayIndex + 1]), int16_t(dataArray[arrayIndex + 2]), int16_t(dataArray[arrayIndex + 3]), int16_t(dataArray[arrayIndex+ 4]), int16_t(dataArray[arrayIndex + 5]));
        
      } else if (count < 55) {
        getIMUData(int16_t(dataArray[arrayIndex]), int16_t(dataArray[arrayIndex + 1]), int16_t(dataArray[arrayIndex + 2]), int16_t(dataArray[arrayIndex + 3]), int16_t(dataArray[arrayIndex + 4]), int16_t(dataArray[arrayIndex + 5]));
      } else if (count == 110) {
        isAction = false;
        count = -10;
      }
    }
  dataArray[arrayIndex] = grav_accX;
  dataArray[arrayIndex + 1] = grav_accY;
  dataArray[arrayIndex + 2] = grav_accZ;
  dataArray[arrayIndex + 3] = gyroX;
  dataArray[arrayIndex + 4] = gyroY;
  dataArray[arrayIndex + 5] = gyroZ;

  arrayIndex += 6;
  arrayIndex = (arrayIndex >= 60) ? 0 : arrayIndex; 
  delay(25);
  }
}