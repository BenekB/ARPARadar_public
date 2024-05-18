# Radar

author: Benedykt Bela

The app Radar is designed as a part of the author's Master thesis. 

This program is designed as a simulator of the marine ARPA device. It can be vividly separated into two parts. First one (the left side of the GUI) is to define the environment. We can add new ships, delete chosen ones and manipulate their motion and location parameters. The second part (the right side of the GUI) is a simulated radar screen as seen onboard one of the defined earlier ships. It is equiped with some functions that normal radar device has.  

This program also implements a modified Kalman filter to estimate vessels motion parameters basing on noised measurements. Moreover, app has been developed with a module that generates navigational messages in NMEA0183 format.

![Napis](/Radar.png)
