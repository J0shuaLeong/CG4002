#include <Wire.h>
#include "CRC.h"
#include "CRC8.h"
#include <IRremote.hpp>
#include <TM1637Display.h>
#include <ezBuzzer.h>

#define HELLO_PACKET 'H'
#define ACK_PACKET 'A'
#define VEST_PACKET 'V'
#define BULLET_HIT_PACKET 'B'
#define SIZE_OF_PACKET 20
#define DURATION 60000000
#define TIMEOUT 100

#define redPin A0
#define CLK 5
#define DIO 4
#define GUN2 0xCD3239DF
#define GUN1 0xCD3239DE
#define SPEED 0x54511082
#define OSC 0xE6F839DE
#define BLINK 0x705C5422


bool handshakeDone;
bool packetAck;
bool isShot;
CRC8 crc;
byte sendBuffer[SIZE_OF_PACKET];
unsigned long startTime = 0;
unsigned long ackstartTime = 0;
char seq_Num = '0'; 

const int IR_RECEIVE_PIN = 2;
const int buzzerPin = 3;
int health;
int prevHealth;

TM1637Display display(CLK, DIO);
ezBuzzer buzzer(buzzerPin);

void playTone(int frequency, int duration) {
  long delayPeriod = 1000000 / frequency;  // Calculate the delay period for the desired frequency (in microseconds)
  long cycles = frequency * duration / 1000;  // Calculate the number of cycles for the duration
  for (long i = 0; i < cycles; i++) { 
    digitalWrite(buzzerPin, HIGH);  // Set buzzer pin HIGH (buzz)
    delayMicroseconds(delayPeriod / 2);  // Delay for half the period (HIGH phase)
    digitalWrite(buzzerPin, LOW);   // Set buzzer pin LOW (silent)
    delayMicroseconds(delayPeriod / 2);  // Delay for the remaining half period (LOW phase)
  }
}
// Function for successful connection (low, mid, and high tones)
void playConnectionTone() {
  playTone(400, 2000);  // Low tone (400Hz) for 200ms
  delay(200);          // Brief pause between tones
  playTone(1000, 2000);  // Mid tone (700Hz) for 200ms 
}
// Function for disconnection (high, mid, and low tones)
void playDisconnectionTone() {
  playTone(700, 2000);  // Mid tone (700Hz) for 200ms
  delay(200);          // Brief pause between tones
  playTone(400, 2000);  // Low tone (400Hz) for 200ms
}


struct AckPacket {
    byte typeOfPacket = ACK_PACKET;
    byte seqNum = 0;
    byte padding[17] = {0};
    byte checkSum;
};

struct VestPacket {
    byte packetType;
    byte seqNum;
    bool shotReceived;
    byte padding[16] = {0};
    byte checkSum;
};

void sendAckPkt() {
    AckPacket ackPkt;
    crc.reset();
    crc.add((byte*)&ackPkt, SIZE_OF_PACKET - sizeof(ackPkt.checkSum));
    ackPkt.checkSum = crc.getCRC();
    memcpy(sendBuffer, &ackPkt, sizeof(ackPkt));
    Serial.write(sendBuffer, SIZE_OF_PACKET);
    memset(sendBuffer, 0, sizeof(sendBuffer));
}

void sendVestData() {
    VestPacket vestPkt;
    vestPkt.packetType = VEST_PACKET;
    vestPkt.seqNum = seq_Num;
    vestPkt.shotReceived = true; 
    crc.reset();
    crc.add((byte*)&vestPkt, SIZE_OF_PACKET - sizeof(vestPkt.checkSum));
    vestPkt.checkSum = crc.getCRC();
    memcpy(sendBuffer, &vestPkt, sizeof(vestPkt));
    Serial.write(sendBuffer, SIZE_OF_PACKET);
}

void visualsOnHit() {
  digitalWrite(redPin, HIGH);
  delay(300);
  digitalWrite(redPin, LOW);
  delay(1);
  return;
}

void bulletHit() {
  visualsOnHit();
  buzzer.beep(100);
  return;
}

void actionHit() {
  buzzer.beep(200);
  digitalWrite(redPin, HIGH);
  delay(200);
  digitalWrite(redPin, LOW);
  delay(100);
  digitalWrite(redPin, HIGH);
  delay(200);
  digitalWrite(redPin, LOW);
  delay(1);
  return;
}

void setup(void) {
    Serial.begin(115200);
    health = 100;
    prevHealth = 100;
    handshakeDone = false;
    IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK); // Start the receiver
    pinMode(redPin, OUTPUT);
    pinMode(buzzerPin, OUTPUT);
    display.setBrightness(5);
    display.showNumberDec(health, false);
    buzzer.loop();
}

void loop() {  
  if (Serial.available()) {
    char incomingResponse = Serial.read();
    if (incomingResponse == HELLO_PACKET) {
        sendAckPkt();
    } else if (incomingResponse == ACK_PACKET) {
        handshakeDone = true;
        //playTone(400, 4000);
        playConnectionTone();
    }
  }

  while (handshakeDone) {
    display.showNumberDec(health, false);
    buzzer.loop();
    if (Serial.available()) {
      char incomingResponse = Serial.read();
      if (incomingResponse == HELLO_PACKET) {
        sendAckPkt();
        handshakeDone = false;
        playDisconnectionTone();
        break;
      } else if (incomingResponse != HELLO_PACKET and incomingResponse != ACK_PACKET) {
          if (prevHealth != incomingResponse) {
            health = incomingResponse;
            bulletHit();
            // int healthdiff = prevHealth - health;
            // if (healthdiff == 5) {
            //   bulletHit();
            // } else if (healthdiff == 10) {
            //   actionHit();
            // }
            prevHealth = health;
          }
          else {
            continue;
          }
      }
    }

    if (IrReceiver.decode()) {
      //Serial.println(IrReceiver.decodedIRData.decodedRawData, HEX);
      //IrReceiver.printIRResultShort(&Serial);
      if (IrReceiver.decodedIRData.decodedRawData == GUN1) { //gun
        //bulletHit();
        packetAck = false;
        isShot = true;
        sendVestData();
        startTime = millis();
      }
      // } else if (IrReceiver.decodedIRData.decodedRawData == SPEED) {
      //   bulletHit();
      //   packetAck = false;
      //   isShot = true;
      //   sendVestData();
      //   startTime = millis();
      // }
      // switch(IrReceiver.decodedIRData.decodedRawData)
      // {
      //   case HIT: // hax val from laser gun
      //     bulletHit();
      //     packetAck = false;
      //     isShot = true;
      //     break;
      //   // case SPEED: //speed from remote controller
      //   //   bulletHit();
      //   //   packetAck = false;
      //   //   isShot = true;
      //   //   break;
      // }
      IrReceiver.resume(); // Enable receiving of the next value
    }

    // if (isShot and !packetAck) {
    //   sendVestData();
    //   startTime = millis();
    // }
    
    while (!packetAck and isShot) {
      if ((millis() - startTime) < TIMEOUT) {
          char incomingResponse = Serial.read();
        if (incomingResponse == ACK_PACKET) {
          packetAck = true;
          seq_Num = (seq_Num == '0') ? '1' : '0';
          isShot = false;
          break;
        }
        if (incomingResponse == HELLO_PACKET) {
          sendAckPkt();
          handshakeDone = false;
          playDisconnectionTone();
          break;
        }
      }
      if ((millis() - startTime) > TIMEOUT) {
        sendVestData();
        startTime = millis();
      }
    }
  }
}
