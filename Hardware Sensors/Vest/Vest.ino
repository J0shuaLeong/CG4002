#include <Arduino.h>
#include <IRremote.hpp>

#define redPin A0

const int IR_RECEIVE_PIN = 2;
const int buzzerPin = 5;

int health = 100;
int shieldHealth = 30;
bool shieldOn = false;

void setup()
{
  Serial.begin(9600);
  IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK); // Start the receiver
  pinMode(redPin, OUTPUT);
  pinMode(buzzerPin, OUTPUT);
}

void audioOnHit() {
  tone(buzzerPin, 3000); // Send 1KHz sound signal
  delay(300);
  noTone(buzzerPin); // Stop sound...
  delay(1);
  return;
}

void visualsOnHit() {
  digitalWrite(redPin, HIGH);
  delay(300);
  digitalWrite(redPin, LOW);
  delay(1);
  return;
}

void hit() {
  //health -= 5; // gunshot or bomb
  visualsOnHit();
  //audioOnHit();
  return;
}

void otherHit() {
  health -= 10;
}

void shield() {
  shieldOn = true;
}

void shieldHit() {
  shieldHealth -= 5;
}

void loop(){
  if (health <= 0) {
    health = 100;
  }
  
  if (IrReceiver.decode()) 
  {
    Serial.println(IrReceiver.decodedIRData.decodedRawData, HEX);
    IrReceiver.printIRResultShort(&Serial);

    switch(IrReceiver.decodedIRData.decodedRawData)
    {
      case 0xE6F839DE:
          hit();
        break;
      default:
        break;
    }

    IrReceiver.resume(); // Enable receiving of the next value
  }
}
