# ISBM-Publication-Provider

This is one of the three-part series of proof-of-concept projects with the primary objective of constructing an interoperable IoT information cluster. The focus is on utilizing non-proprietary Open Industrial Interoperability Ecosystem (OIIE) open standards. Each project within this series explores key facets of building a cohesive and scalable IoT infrastructure, demonstrating the potential of OIIE standards in promoting interoperability in the interconnect world.

![image](/Documents/Images/IoT-Demo.jpg)
Figure 1. The summary of the IoT demo using OIIE standards. Included in this three-part series are ISBM-2.0-Server-Adapter and ISBM-Publication-Consumer, hosted in their respective repositories.

### Contents
  
   1. [Objectives](#Objectives)
   2. [Project Information](#Project-Information)
   3. [Temperature Sensor Wiring](#MCP9808-Wiring)
   4. [Before Running the Program](#Before-Running-the-Program)
   5. [Useful Links](#Useful-Links)
   6. [Quick Reference](#Quick-Reference)
  
### Objectives

Build a Raspberry Pi IoT device running on Raspbian to measure room temperature using MCP9808. The collected data will be published in CCOM format, embedded in the OAGIS BOD message via an ISBM 2.0 Server adapter, making it accessible for other IoT devices to consume.

![image](/Documents/Images/IoT-Demo-Temperature-Sensor.jpg)
Figure 2. This project focuses on utilizing the temperature sensor as a sample ISBM Publication Provider. The temperature data will be published via the ISBM Server Adapter for other devices to consume in an interoperable manner.

### Project Information

#### Version v0.2

A console program written in C#, targeting .NET 6, adopts a cross-platform approach for diverse devices with different hardware architectures.

The program starts with an Open Publication Session, proceeds to measure temperature, and posts the publication every five seconds. Users can stop the temperature acquisition and publication loop at their discretion. Additionally, the program provides an option to Close the Publication Session before exiting.

A data mode option allows users to choose between sensor-acquired or simulated temperature data. This feature enables the program to function when a physical temperature sensor system is not available.


#### Tools
     1.  Visual Studio 2022 Community
     2.  Raspberry Pi 3b
     3.  Raspbian Version 11 (bullseye)"
     4.  MCP9808 Temperature Sensor

#### Dependencies
     1.  .Net 6
     2.  Microsoft.NetCore.UniversalWindowsPlatform 6.2.9 @
     3.  RapidRedPanda.ISBM.ClientAdapter 2.0.1 @
     4.  Iot.Device.Bindings 3.1.0 @
     5.  NewtonSoft v12.0.2 @
    
     @ NuGet Packages
     
### MCP9808 Wiring

![image](/Documents/Wiring/MCP9808-Wiring.jpg)

### Before Running the Program

    Make sure configurations are set properly in the Configs.json

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

#### Standard Organizations
   1. [OpenO&M](https://openoandm.org/)
   2. [MIMOSA](https://www.mimosa.org/)
   3. [International Society of Automation](https://www.isa.org/)
   4. [OAGi](https://oagi.org/)

#### Development Tools
   1. [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
   2. [Raspberry Pi OS (previously called Raspbian)](https://www.raspberrypi.com/software/)
   3. [RapidRedPanda.ISBM.ClientAdapter](https://www.nuget.org/packages/RapidRedPanda.ISBM.ClientAdapter/#readme-body-tab)
   4. [MCP9808 Temperature Sensor Specifications](https://ww1.microchip.com/downloads/en/DeviceDoc/25095A.pdf)

### Quick Reference

   1. OIIE - [OpenO&M Open Industrial Interoperability Ecosystem](https://www.mimosa.org/open-industrial-interoperability-ecosystem-oiie/)
   2. ISBM - [International Society of Automation ISA-95 Message Service Model](https://openoandm.org/files/standards/ISBM-2.0.pdf)
   3. CCOM - [MIMOSA Common Conceptual Object Model](https://www.mimosa.org/mimosa-ccom/)
   4. BOD - [OAGIS Business Object Document](https://www.oagidocs.org/docs/)
