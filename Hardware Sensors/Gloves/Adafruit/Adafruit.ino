#include <Wire.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>

Adafruit_MPU6050 mpu;

void setup() {
  // Initialize serial communication at 115200 baud
  Serial.begin(115200);
  while (!Serial) delay(10);  // Wait for Serial Monitor to open

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
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);

  // Set filter bandwidth to 21 Hz for both accelerometer and gyroscope
  // Noise reduction: higher bandwidth (260) = less filtering, faster response
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  // Give the sensor some time to stabilize
  delay(100);
}

void loop() {
   // Declare float variables to store the sensor readings
  float accX, accY, accZ;  // For accelerometer values
  float gyroX, gyroY, gyroZ;  // For gyroscope values

  // Get new sensor events for accelerometer and gyroscope
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  // Store accelerometer values in float variables
  accX = a.acceleration.x;
  accY = a.acceleration.y;
  accZ = -1.0 * a.acceleration.z - 0.4;

  // Store gyroscope values in float variables
  gyroX = g.gyro.x;
  gyroY = g.gyro.y;
  gyroZ = g.gyro.z;

  // Send the accelerometer and gyroscope data to the Serial Plotter
  Serial.print("accX:"); Serial.print(accX); Serial.print(",");
  Serial.print("accY:"); Serial.print(accY); Serial.print(",");
  Serial.print("accZ:"); Serial.print(accZ); Serial.print(",");

  Serial.print("gyroX:"); Serial.print(gyroX); Serial.print(",");
  Serial.print("gyroY:"); Serial.print(gyroY); Serial.print(",");
  Serial.print("gyroZ:"); Serial.println(gyroZ);

  // Small delay to control data rate
  delay(100);  // Adjust this delay to control the frequency of data output
}
