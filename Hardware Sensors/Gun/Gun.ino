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

const int IR = 3;
const int buttonPin = 2;
const int reloadPin = 4;
const unsigned long hexVal = 0xE6F839DE;

volatile int buttonState = LOW;
volatile unsigned long prev_time = 0;

int bullets = 10;
bool interrupt = false;
bool noBullets = false;

void buttonInterrupt() {
  if (!noBullets) {
    if (millis() - prev_time >= 250) {
      prev_time = millis();
      digitalWrite(IR, HIGH);
      delay(500);
      digitalWrite(IR, LOW);
      bullets--;
      interrupt = true;
    }
  }
}

void reload() {
  bullets = 10;
  noBullets = false;
  return;
}

void updateBulletsOnScreen() {
  display.clearDisplay();

  display.setTextSize(3);      // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE); // Draw white text
  display.setCursor(0, 0);     // Start at top-left corner
  
  switch(bullets) {
    case 9:
      display.println(F("9/10"));
      display.display();
      break;
    case 8:
      display.println(F("8/10"));
      display.display();
      break;
    case 7:
      display.println(F("7/10"));
      display.display();
      break;
    case 6:
      display.println(F("6/10"));
      display.display();
      break;
    case 5:
      display.println(F("5/10"));
      display.display();
      break;
    case 4:
      display.println(F("4/10"));
      display.display();
      break;
    case 3:
      display.println(F("3/10"));
      display.display();
      break;
    case 2:
      display.println(F("2/10"));
      display.display();
      break;
    case 1:
      display.println(F("1/10"));
      display.display();
      break;
    case 0:
      display.println(F("0/10"));
      display.display();
      break;
    default:
      display.println(F("10/10"));
      display.display();
      delay(200);
      break;
  }
  return;
}

void initialScreen(void) {
  display.clearDisplay();

  display.setTextSize(3);      // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE); // Draw white text
  display.setCursor(0, 0);     // Start at top-left corner
  display.println(F("10/10"));

  display.display();
  delay(200);
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
  initialScreen();

  pinMode(IR, OUTPUT);
  pinMode(buttonPin, INPUT_PULLUP);
  pinMode(reloadPin, INPUT);
  attachInterrupt(digitalPinToInterrupt(buttonPin), buttonInterrupt, RISING);
}

void loop() {
  if (interrupt) {
    updateBulletsOnScreen();
  }

  if (bullets <= 0) {
    noBullets = true;
  }

  buttonState = digitalRead(reloadPin);
  if (buttonState == HIGH) {
    reload();
  }
}