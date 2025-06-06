#include <Arduino.h>
#include <IRremote.hpp>
#include <TM1637Display.h>
#include <ezBuzzer.h>

#define redPin A0
#define CLK 5
#define DIO 4

const int IR_RECEIVE_PIN = 2;
const int buzzerPin = 3;

int health = 100;
int shieldHealth = 30;
bool shieldOn = false;

TM1637Display display(CLK, DIO);
ezBuzzer buzzer(buzzerPin);

void setup()
{
  Serial.begin(9600);
  IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK); // Start the receiver
  pinMode(redPin, OUTPUT);
  pinMode(buzzerPin, OUTPUT);
  display.setBrightness(5);
}

void visualsOnHit() {
  digitalWrite(redPin, HIGH);
  delay(300);
  digitalWrite(redPin, LOW);
  delay(1);
  return;
}

void hit() {
  if (shieldOn) {
    shieldHealth -= 5;
    buzzer.beep(100);
  } else {
    health -= 5; // gunshot or bomb
    visualsOnHit();
    buzzer.beep(100);
  }
  return;
}

void otherHit() {
  if (shieldOn) {
    shieldHealth -= 10;
    buzzer.beep(200);
  } else {
    health -= 10;
    buzzer.beep(200);
    digitalWrite(redPin, HIGH);
    delay(200);
    digitalWrite(redPin, LOW);
    delay(100);
    digitalWrite(redPin, HIGH);
    delay(200);
    digitalWrite(redPin, LOW);
    delay(1);
  }
  return;
}

void shield() {
  shieldOn = true;
}

void shieldHit() {
  shieldHealth -= 5;
}

void loop(){
  buzzer.loop();
  display.showNumberDec(health, false);

  if (health <= 0) {
    // int length = sizeof(noteDurations) / sizeof(int);
    // buzzer.playMelody(melody, noteDurations, length);
    health = 100;
  }

  if (shieldHealth <= 0) {
    shieldOn = false;
  }
  
  if (IrReceiver.decode()) 
  {
    Serial.println(IrReceiver.decodedIRData.decodedRawData, HEX);
    IrReceiver.printIRResultShort(&Serial);

    switch(IrReceiver.decodedIRData.decodedRawData)
    {
      case 0xCD3239DE: // hax val from laser gun
          hit();
        break;
      case 0x54511082: //speed
          hit();
        break;
      case 0xE6F839DE: //osc
          otherHit();
        break;
      case 0x705C5422: //blink
          shield();
        break;
      default:
        break;
    }

    IrReceiver.resume(); // Enable receiving of the next value
  }
}
