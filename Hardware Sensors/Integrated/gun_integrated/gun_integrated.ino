#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Wire.h>
#include "CRC.h"
#include "CRC8.h"
#include <Arduino.h>
#include <IRremote.h>
#include <SPI.h>

#define HELLO_PACKET 'H'
#define ACK_PACKET 'A'
#define GUN_PACKET 'G'
#define SIZE_OF_PACKET 20
#define DURATION 60000000 
#define TIMEOUT 200
#define GUN2 0xCD3239DF
#define GUN1 0xCD3239DE


#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels
#define OLED_RESET     -1 // Reset pin # (or -1 if sharing Arduino reset pin)
#define SCREEN_ADDRESS 0x3C ///< See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

// Parameters for buttons
volatile int buttonState = LOW; // current state of trigger
volatile unsigned long prev_time = 0; // for debouncing
bool release = false; // Flag for trigger press to prevent accidental shot when releasing trigger

// Parameters for ammo
int bullets_count = 6;
bool interrupt = false;
bool noBullets = false;
int reload_counter = 6;

// Parameters for displaying rectangles to show ammo
int rectWidth = 15;
int rectHeight = 60;
int rectSpacing = 5;
int startX = 5;
int startY = 10;

// Pin config
const int IR = 3;
const int triggerPin = 2;
const int reloadPin = 4;
const unsigned long hexVal = GUN2;

bool handshakeDone;
bool packetAck;
CRC8 crc;
byte sendBuffer[SIZE_OF_PACKET];
unsigned long startTime = 0;
unsigned long ackstartTime = 0;
char seq_Num = '0';

void buttonInterrupt() {
  if (!noBullets && !release) {
    if (millis() - prev_time >= 250) {
      prev_time = millis();
      if (bullets_count > 0) {
        IrSender.sendNEC(hexVal, 0x32, 0);
      }
      delay(150);
      interrupt = true;
      release = true;
    }
  }
}

void showReloadScreen() {
  display.clearDisplay();        // Clear the display

  // Draw a filled circle with a large exclamation mark
  drawExclamationMarkWithCircle();

  display.display();    
  delay(10);         // Show the reload screen
}

void drawExclamationMarkWithCircle() {
  int centerX = SCREEN_WIDTH / 2;  // Center the circle horizontally
  int centerY = SCREEN_HEIGHT / 2; // Center the circle vertically
  int radius = 30;                 // Radius of the circle

  // Draw a filled circle
  display.fillCircle(centerX, centerY, radius, SSD1306_WHITE);

  // Set text size to make the '!' large enough (increase size for larger exclamation mark)
  display.setTextSize(6);  // Increase text size
  display.setTextColor(SSD1306_BLACK);  // Set the text color to black (for contrast against white circle)
  
  // Adjust cursor position to center the exclamation mark inside the circle
  display.setCursor(centerX - 14, centerY - 20);  // Adjust this to fit text size
  display.print('!');
}

void updateBulletsOnScreen() {
  display.clearDisplay();  // Clear the display before drawing
  
  for (int i = 0; i < bullets_count; i++) {
    int x = startX + i * (rectWidth + rectSpacing);
    display.fillRect(x, startY, rectWidth, rectHeight, SSD1306_WHITE);
  }
  
  display.display();  // Show the updated display
  return;
}

struct AckPacket {
  byte typeOfPacket = ACK_PACKET;
  byte seqNum = 0;
  byte padding[17] = {0};
  byte checkSum;
};

struct GunPacket {
  byte packetType = GUN_PACKET;
  byte seqNum;
  byte bulletCount;
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
  memset(sendBuffer, 0, sizeof(sendBuffer));  // Reset the buffer

}

void sendGunData() {
  GunPacket gunPkt;
  gunPkt.packetType = GUN_PACKET;
  gunPkt.seqNum = seq_Num;  
  gunPkt.bulletCount = bullets_count;  

  crc.reset();
  crc.add((byte*)&gunPkt, SIZE_OF_PACKET - sizeof(gunPkt.checkSum));
  gunPkt.checkSum = crc.getCRC();
  memcpy(sendBuffer, &gunPkt, sizeof(gunPkt));
  Serial.write(sendBuffer, SIZE_OF_PACKET);
}


void setup(void) {
  Serial.begin(115200);
  handshakeDone = false;
  // SSD1306_SWITCHCAPVCC = generate display voltage from 3.3V internally
  if(!display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS)) {
    Serial.println(F("SSD1306 allocation failed"));
    for(;;); // Don't proceed, loop forever
  }
  // Show initial display buffer contents on the screen --
  // the library initializes this with an Adafruit splash screen.
  display.display();
  delay(1000);
  display.clearDisplay();
  updateBulletsOnScreen();

  IrSender.begin(IR);
  pinMode(triggerPin, INPUT_PULLUP);
  pinMode(reloadPin, INPUT);
  
}

void loop() {
  if (Serial.available()) {
  char incomingResponse = Serial.read();
      if (incomingResponse == HELLO_PACKET) {
          handshakeDone = false;
          sendAckPkt();
      } else if (incomingResponse == ACK_PACKET) {
          handshakeDone = true;
      }
  }

  while (handshakeDone) {
    attachInterrupt(digitalPinToInterrupt(triggerPin), buttonInterrupt, RISING);
    // if (bullets_count <= 0) {
    //   noBullets = true;
    //   bullets_count = 0;
    //   sendGunData();
    //   showReloadScreen();
    // } 
    
    if (Serial.available()) {
      char incomingResponse = Serial.read();
      if (incomingResponse == HELLO_PACKET) {
        sendAckPkt();
        handshakeDone = false;
        break;
      } else if (incomingResponse != HELLO_PACKET and incomingResponse != ACK_PACKET) {
        //get bullet count from laptop, update display
        bullets_count = incomingResponse;
        updateBulletsOnScreen();
        //remove later on
        // if (bullets_count == 0) {
        //   noBullets = true;
        // } else {
        //   noBullets = false;
        // }
      } 
    }
    
    if (digitalRead(triggerPin) == LOW) {
      release = false;
    }

    if (interrupt) { //and !noBullets) {
      bullets_count--;
      if (bullets_count <= 0) {
        //noBullets = true;
        bullets_count = 0;
        showReloadScreen();
      } else {
        updateBulletsOnScreen();
      }
      packetAck = false;
      sendGunData();
      startTime = millis();
    }

    while (!packetAck and interrupt) {
      if ((millis() - startTime) < TIMEOUT) {
        if (Serial.available()) {
          char incomingResponse = Serial.read();
          if (incomingResponse == ACK_PACKET) {
            packetAck = true;
            interrupt = false;
            seq_Num = (seq_Num == '0') ? '1' : '0';
            break;
          }
          if (incomingResponse == HELLO_PACKET) {
            sendAckPkt();
            handshakeDone = false;
            break;
          }
        }
      }
      if ((millis() - startTime) > TIMEOUT) {
        sendGunData();
        startTime = millis();
      }
    }
  }
}

