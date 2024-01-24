# ISBM-Publication-Provider

This is one of the three-part series of proof-of-concept projects with the primary objective of constructing an interoperable IoT information cluster. The focus is on utilizing non-proprietary Open Industrial Interoperability Ecosystem (OIIE) open standards. Each project within this series explores key facets of building a cohesive and scalable IoT infrastructure, demonstrating the potential of OIIE standards in promoting interoperability in the interconnect world.

![image](/Documents/Images/IoT-Demo.jpg)

Included in this three-part series are ISBM-2.0-Server-Adapter and ISBM-Publication-Consumer, hosted in their respective repositories.

### Objectives

Build a Raspberry Pi IoT device running on Raspbian to measure room temperature using MCP9808. The collected data will be published in CCOM format, embedded in a BOD message via an ISBM 2.0 Server adapter, making it accessible for other IoT devices to consume.

![image](/Documents/Images/IoT-Demo-Temperature-Sensor.jpg)

### Project Information

#### Version v0.2

A console program written in C#, targeting .NET 6, adopts a cross-platform approach for diverse devices with different hardware architectures.

The program starts with an Open Publication Session, proceeds to measure temperature, and posts the publication every five seconds. Users can stop the temperature acquisition and publication loop at their discretion. Additionally, the program provides an option to Close the Publication Session before exiting.

A data mode option allows users to choose between sensor-acquired or simulated temperature data. This feature enables the program to function when a physical temperature sensor system is not available.


#### Tools
     1.  Visual Studio 2022 Community
     2.  Raspberry Pi 3b
     3.  Raspbian Version 11 (bullseye)"
     4.  MCP9808 Temperature sensor

#### Dependencies
     1.  .Net 6
     2.  Microsoft.NetCore.UniversalWindowsPlatform 6.2.9 @
     3.  NewtonSoft v12.0.2 @
    
     @ NuGet Packages
     
### MCP9808 Wiring

![image](/Documents/Wiring/MCP9808-Wiring.jpg)

### Configurations

    Configs.json

    {
	   "hostName": "Your ISBM Server Adapter Host Address",
	   "channelId": "/services/general/publication",
	   "topic": "OIIE:S30:V1.1/CCOM-JSON:SyncMeasurements:V1.0",
	   "authentication": 0,
	   "userName": "",
	   "password": "",
	   "simMode": 1
    }

  1. When using the ISBM Server Adaptor project provided in this OIIE demo, set "authentication" to 0. In case of connecting to secured ISBM servers, change "authentication" to 1 and provide the credential information.
  2. Use "simMode" to toggle between actual and simulated data.   

### Useful Links

http://www.openoandm.org/files/standards/ISBM-2.0.pdf

https://www.mimosa.org/mimosa-ccom/

https://ww1.microchip.com/downloads/en/DeviceDoc/25095A.pdf
