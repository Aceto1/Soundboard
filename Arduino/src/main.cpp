#include <Arduino.h>
#include <CD74HC4067.h>

const int s0 = D5;
const int s1 = D6;
const int s2 = D7;
const int s3 = D1;
const int sig = A0;

CD74HC4067 mux(s0, s1, s2, s3);

void setup()
{
  Serial.begin(9600);
}

void loop()
{
  for (int channel = 0; channel < 16; channel++)
  {
    mux.channel(channel);

    //read the value at the SIG pin
    int val = analogRead(sig);

    //return the value
    float voltage = (val * 5.0) / 1024.0;
    if (voltage > 2.5)
    {
      Serial.print(channel);
      delay(500);
    }
  }
}