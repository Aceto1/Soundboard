#include <Arduino.h>
#include <ezButton.h>
#include <CD74HC4067.h>

const int s0 = 13;
const int s1 = 12;
const int s2 = 14;
const int s3 = 2;
const int sig = 5;
const int en = 16;

ezButton button(sig);
CD74HC4067 mux(s0, s1, s2, s3);

unsigned long debounceDelay = 50; // the debounce time; increase if the output flickers

void setup()
{
  Serial.begin(9600);

    digitalWrite(en, LOW);

  button.setDebounceTime(debounceDelay); // set debounce time to 50 milliseconds
  Serial.println("GOGO");
}

void loop()
{
  for (int channel = 0; channel < 16; channel++)
  {
    mux.channel(channel);

    button.loop();

    if (button.isPressed())
      Serial.println(channel);
  }
}