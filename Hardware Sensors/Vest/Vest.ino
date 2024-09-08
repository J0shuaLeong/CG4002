#include <Arduino.h>

#include <IRremote.hpp>

const int IR_RECEIVE_PIN = 2;
#define redPin A0

void setup()
{
  Serial.begin(9600);
  IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK); // Start the receiver
  pinMode(redPin, OUTPUT);
}

void loop(){
  if (IrReceiver.decode()) 
  {
    Serial.println(IrReceiver.decodedIRData.decodedRawData, HEX);
    IrReceiver.printIRResultShort(&Serial);

    switch(IrReceiver.decodedIRData.decodedRawData)
    {
      case 0xE6F839DE: 
        Serial.println("shot");
        digitalWrite(redPin, HIGH);
        delay(2000);
        digitalWrite(redPin, LOW);
        break;
      default:
        break;
    }

    IrReceiver.resume(); // Enable receiving of the next value
  }
}