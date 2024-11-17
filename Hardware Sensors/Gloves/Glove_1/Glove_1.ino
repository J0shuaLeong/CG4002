#include <Wire.h>
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>

Adafruit_MPU6050 mpu;

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
HighPass<2> lp(0.5,1e3,true);

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
  mpu.setGyroRange(MPU6050_RANGE_1000_DEG);

  // Set filter bandwidth to 21 Hz for both accelerometer and gyroscope
  // Noise reduction: higher bandwidth (260) = less filtering, faster response
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);

  // Give the sensor some time to stabilize
  delay(100);
}

float* minusGravity(float gyroX, float gyroY, float gyroZ, float accX, float accY, float accZ) {
//     Serial.print("gyroX:"); Serial.print(gyroX); Serial.print(",");
//  Serial.print("gyroY:"); Serial.print(gyroY); Serial.print(",");
//  Serial.print("gyroZ:"); Serial.print(gyroZ); Serial.print(",");
   float x = accX, y = accY, z = 0;
   accX -= x * cos(gyroZ) - y * sin(gyroZ) ;
   accY -= x * sin(gyroZ) - y * cos(gyroZ) ;
  
   x = accX; z = accZ;
   accX -=  x * cos(gyroY) + z * sin(gyroY) ;
   accZ -= - x * sin(gyroY) + z * cos(gyroY) ;
  
   y = accY, z = accZ;
   accY -=  y * cos(gyroX) - z * sin(gyroX);
   accZ -=  y * sin(gyroX) + z * cos(gyroX);

  Serial.print("accX:"); Serial.print(accX); Serial.print(",");
  Serial.print("accY:"); Serial.print(accY); Serial.print(",");
  Serial.print("accZ:"); Serial.println(accZ);// Serial.print(",");

  float acc[3] = {accX, accY, accZ};
  return acc;
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
  accZ = a.acceleration.z;

  // Store gyroscope values in float variables
  gyroX = g.gyro.x;
  gyroY = g.gyro.y;
  gyroZ = g.gyro.z;

  float rVec = sqrt(pow(accX, 2) + pow(accY, 2) + pow(accZ, 2));

  // Compute the filtered signal
  float rn = lp.filt(rVec);

  //float* acc = minusGravity(gyroX, gyroY, gyroZ, accX, accY, accZ);
  // Send the accelerometer and gyroscope data to the Serial Plotter
//  Serial.print("accX:"); Serial.print(xn); Serial.print(",");
//  Serial.print("accY:"); Serial.print(yn); Serial.print(",");
  Serial.print("accZ:"); Serial.println(rn);// Serial.print(",");

//  Serial.print("gyroX:"); Serial.print(gyroX); Serial.print(",");
//  Serial.print("gyroY:"); Serial.print(gyroY); Serial.print(",");
//  Serial.print("gyroZ:"); Serial.println(gyroZ);

  // Small delay to control data rate
  delay(50);  // Adjust this delay to control the frequency of data output
}
