#include <Arduino.h>
#include <IRremote.h>
#include <SPI.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels
#define OLED_RESET     -1 // Reset pin # (or -1 if sharing Arduino reset pin)
#define SCREEN_ADDRESS 0x3C ///< See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

// Pin config
const int IR = 3;
const int triggerPin = 2;
const int reloadPin = 4;
const unsigned long hexVal = 0xCD3239DE;//0xE6F839DE;

// Parameters for buttons
volatile int buttonState = LOW; // current state of trigger
volatile unsigned long prev_time = 0; // for debouncing
bool release = false; // Flag for trigger press to prevent accidental shot when releasing trigger

// Parameters for ammo
int bullets = 6;
bool interrupt = false;
bool noBullets = false;

// Parameters for the rectangles
int rectWidth = 15;
int rectHeight = 60;
int rectSpacing = 5;
int startX = 5;
int startY = 10;

void buttonInterrupt() {
  if (!noBullets && !release) {
    if (millis() - prev_time >= 250) {
      prev_time = millis();
      IrSender.sendNEC(hexVal, 0x32, 0);
      delay(150);
      bullets--;
      interrupt = true;
      release = true;
    }
  }
}

void reload() {
  bullets = 6;
  noBullets = false;
  return;
}

void updateBulletsOnScreen() {
  display.clearDisplay();  // Clear the display before drawing
  
  for (int i = 0; i < bullets; i++) {
    int x = startX + i * (rectWidth + rectSpacing);
    display.fillRect(x, startY, rectWidth, rectHeight, SSD1306_WHITE);
  }
  
  display.display();  // Show the updated display
  return;
}

// void reloadScreen() {
//   display.clearDisplay();

//   display.setTextSize(3);      // Normal 1:1 pixel scale
//   display.setTextColor(SSD1306_WHITE); // Draw white text
//   display.setCursor(0, 0);     // Start at top-left corner
//   display.println(F("Reload!"));

//   display.display();
// }

void reloadScreen(int blinkTimes, int delayTime) {
  for (int i = 0; i < blinkTimes; i++) {
    // Turn the screen fully ON (all pixels lit)
    display.clearDisplay();  // Clear the buffer
    display.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, SSD1306_WHITE);  // Fill the screen white
    display.display();  // Send the buffer to the display
    delay(delayTime);  // Wait

    // Turn the screen fully OFF (all pixels off)
    display.clearDisplay();  // Clear the buffer
    display.display();  // Send the blank buffer to the display
    delay(delayTime);  // Wait
  }
}

void setup() {
  Serial.begin(9600);

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
  attachInterrupt(digitalPinToInterrupt(triggerPin), buttonInterrupt, RISING);
}

void loop() {
  if (digitalRead(triggerPin) == LOW) {
    release = false;
  }

  if (interrupt && !noBullets) {
    updateBulletsOnScreen();
  }

  if (bullets <= 0) {
    noBullets = true;
    reloadScreen(5, 500);
  }

  if (noBullets) {
    delay(1000);
    reload();
  }

  // buttonState = digitalRead(reloadPin);
  // if (buttonState == HIGH) {
  //   reload();
  // }
}
