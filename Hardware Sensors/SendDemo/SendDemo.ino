#include <Arduino.h>
#include <IRremote.h>

IRsend irsend;

const unsigned long hexVal = 0xFEA857;

void setup() {
  IrSender.begin(3, true, A0);
}

void loop() {
  irsend.sendNEC(hexVal, 32);
  delay(1000);
}