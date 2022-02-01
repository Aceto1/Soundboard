

![Banner.png](E:\Dateien\Source\Soundboard\img\Banner.png)

An Arduino-based physical soundboard with a Windows Desktop GUI.



# Building the Soundboard

## Materials

- 1 Arduino board of your choice (e.g. ESP8266)

- 1 USB cable

- 1 CD74HC4067 analog 16 channel multiplexer

- 15 push-buttons

- Some sort of case (or like for my prototyp: two pieces of plywood with the appropriate holes and some spacers)

- Wire (I used 20AWG)

- Soldering iron



Begin by inserting the buttons into the case and connecting one pole of each button with wire and solder in series. After that, also connect the last one to a 3V output of your arduino.

Then connect the other side of each button to the appropriate channel of the multiplexer (starting top left with 0 and ending bottom right with 15).

Connect the pins of the multiplexer as follows to the arduino:

- S0: D5

- S1: D6

- S2: D7

- S3: D1

- SIG: A0

- EN: Leave it unconnected

# Installation

## Requirements

- [VB-Audio VoiceMeeter Banana](https://vb-audio.com/Voicemeeter/banana.htm) - for routing the playback into your VoIP Application of choice

- [CH341 Driver](www.wch.cn/download/CH341SER_ZIP.html) - or the equivalent driver for your Arduino board

- The latest binary of this repository



Install VoiceMeeter and the Driver, then reboot your PC. This will probably bork your sound settings in windows. We will fix that soon.

Now copy the Soundboard.exe and all other files that come with it to `C:\Programs Files\Soundboard\`.

Open VoiceMeeter Banana and firstly go to "Menu" at the top right and activate "Run on Windows Startup".

After that Click on "A1" and select your regular output device (speakers, headphones).

Click on "Hardware Input 1" and select your regular input device (microphone). Then only select "B1" below that.

Then, under "Virtual Inputs" just select "A1" below "VoiceMeeter VAIO" and just "B1" below "VoiceMeeter AUX".

Now go into the Windows sound settings and set "VoiceMeeter Input" as your default output device and "VoiceMeeter Output" as yout default input device.



Now connect your physical soundboard and start the application, you're all set.

# Usage

Choose the COM-Port of your Soundboard. This is usually the only or one of very few COM-Ports and can be trial-and-errored.

Click a button to change it's content (title/file). Configured files are kept in the folder they were in. That means should you delete or move a file you also need to change the setting of the button.

Should you wish to start this application with Windows, just activatt he toggle button. This creates a shortcut in your Autostart folder.

Minimizing the application moves it to the system tray. This is also where the application will be on startup. 
