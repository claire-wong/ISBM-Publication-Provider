using Iot.Device.Mcp9808;
using System.Device.I2c;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RapidRedPanda.ISBM.ClientAdapter.ResponseType;
using RapidRedPanda.ISBM.ClientAdapter;

string _hostName = "";
string _channelId = "";
string _topic = "";
string _sessionId = "";
Boolean _authentication;
string _username = "";
string _password = "";
Boolean _simMode;
string _bodTemplate = "";

ProviderPublicationService _myProviderPublicationService = new ProviderPublicationService();

SetConfigurations();
GetBODTemplate();


Console.WriteLine("Welcome to .Net 6 ISBM Temperature Sensor Program!");
Console.WriteLine("A simple example using OIIE to create IoT device quickly.");
Console.WriteLine("This program will serve as a temperature measurement device and the measured ");
Console.WriteLine("temperature data will be published to ISBM server for other applications to consume.");
Console.WriteLine("");
Console.WriteLine("Press Enter to open a Provider Publication Service Session.....");
string key = Console.ReadKey().Key.ToString();

if (key == "Enter")
{
    _myProviderPublicationService.Credential.Username = _username;
    _myProviderPublicationService.Credential.Password = _password;

    //Open an Provider Publication Session
    OpenPublicationSessionResponse myOpenPublicationSessionResponse = _myProviderPublicationService.OpenPublicationSession(_hostName, _channelId);
    //ISBM Adapter Response
    if (myOpenPublicationSessionResponse.StatusCode == 201)
    {
        _sessionId = myOpenPublicationSessionResponse.SessionID;
        Console.WriteLine("Provider Publication Service Session is opened sucessfully!");
        Console.WriteLine("Session Id : " + _sessionId);
        Console.WriteLine(" ");
    }
    else
    {
        Console.WriteLine("Provider Request Session is failed to open! Please check your settings in Configs.json file.");
        Console.WriteLine(" ");
        Console.WriteLine("Press Enter to end program!");
        key = Console.ReadKey().Key.ToString();
        if (key == "Enter")
        {
            Environment.Exit(0);
        }
    }
}

Console.WriteLine("Press Enter to publish measured temperature.....");
Console.WriteLine(" ");
key = Console.ReadKey().Key.ToString();

Boolean stopLoop = false;

if (key == "Enter")
{
    // Start a separate thread for user input
    Thread userInputThread = new Thread(() =>
    {
        Console.WriteLine("Press Enter to stop the publication!");
        Console.ReadLine();
        stopLoop = true;
    });
    userInputThread.Start();

    if (_simMode == false)
    {
        I2cConnectionSettings settings = new I2cConnectionSettings(1, Mcp9808.DefaultI2cAddress);
        I2cDevice device = I2cDevice.Create(settings);
        Mcp9808 sensor = new Mcp9808(device);

        while (!stopLoop)
        {
            double MeasuredTemperature = Math.Round(sensor.Temperature.DegreesCelsius, 2);
            PublishBOD(MeasuredTemperature.ToString());
                        
            Thread.Sleep(5000);
        }

    }
    else
    {
        while (!stopLoop)
        {
            double MeasuredTemperature = Math.Round(SimTemperature(30), 2);
            PublishBOD(SimTemperature(30).ToString());
                        
            Thread.Sleep(5000);
        }
    }
}
   

Console.WriteLine("Temperature publication is stopped, please Press Enter to close Provider Publication Service Session.....");
Console.WriteLine(" ");
key = Console.ReadKey().Key.ToString();

if (key == "Enter")
{
    //Calling ISBM Adaper method
    ClosePublicationSessionResponse myClosePublicationSessionResponse = _myProviderPublicationService.ClosePublicationSession(_hostName, _sessionId);

    //ISBM Adapter Response
    if (myClosePublicationSessionResponse.StatusCode == 204)
    {
        Console.WriteLine("The Provider Request Session is closed sucessfully!");
        Console.WriteLine(" ");
    }
    else
    {
        Console.WriteLine("ISBM HTTP Response : " + myClosePublicationSessionResponse.ISBMHTTPResponse);
        Console.WriteLine(" ");
    }
}

Console.WriteLine("Press Enter to end program!");
key = Console.ReadKey().Key.ToString();
 


void SetConfigurations()
{
    //Read application configurations from Configs.json 
    string filename = "Configs.json";

    string JsonFromFile = System.IO.File.ReadAllText(filename);

    JObject JObjectConfigs = JObject.Parse(JsonFromFile);
    _hostName = JObjectConfigs["hostName"].ToString();
    _channelId = JObjectConfigs["channelId"].ToString();
    _topic = JObjectConfigs["topic"].ToString();

    _authentication = (Boolean)JObjectConfigs["authentication"];
    if (_authentication == true)
    {
        _username = JObjectConfigs["userName"].ToString();
        _password = JObjectConfigs["password"].ToString();
    }

    _simMode = (Boolean)JObjectConfigs["simMode"];
}

void GetBODTemplate()
{
    string filename = "SyncMeasurements.json";

    string JsonFromFile = System.IO.File.ReadAllText(filename);

    _bodTemplate = JsonFromFile;
}

void PublishBOD(string valueToPublish)
{

    //Create new BOD message from SyncMeasurements use case template
    string bodMessage = FillBODFields(_bodTemplate, valueToPublish);

    //Post Publication - BOD message
    PostPublicationResponse myPostPublicationResponse = _myProviderPublicationService.PostPublication(_hostName, _sessionId, _topic, bodMessage);

    string MessageId = "";
    if (myPostPublicationResponse.StatusCode == 201)
    {
        MessageId = myPostPublicationResponse.MessageID;
        Console.WriteLine("Message " + MessageId + " has been pusblished!!  Temperature: " + valueToPublish + "Celsius");
    }
    else
    {
        Console.WriteLine(myPostPublicationResponse.StatusCode + " " + myPostPublicationResponse.ISBMHTTPResponse);
    }

}

string FillBODFields(string bodTemplate, string valueToPublish)
{       
    JObject objBOD = JObject.Parse(_bodTemplate);

    objBOD["syncMeasurements"]["applicationArea"]["bODID"] = System.Guid.NewGuid().ToString();
    objBOD["syncMeasurements"]["applicationArea"]["creationDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurementLocation"]["UUID"] = "6bc95b51-5657-2530-4b86-20e5482d2af8";
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurementLocation"]["shortName"] = "Ambient Temperature";
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurementLocation"]["infoSource"]["UUID"] = "5f73f1b7-522f-2aa7-6c77-57fded70ec7f";

    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["UUID"] = System.Guid.NewGuid().ToString();
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["recorded"]["dateTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["infoSource"]["UUID"] = "5f73f1b7-522f-2aa7-6c77-57fded70ec7f";

    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["data"]["measure"]["value"] = valueToPublish;
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["data"]["measure"]["unitOfMeasure"]["UUID"] = "3912c639-8c27-4b29-868b-a0f01790770f";
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["data"]["measure"]["unitOfMeasure"]["shortName"] = "Degrees Celsius";
    objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["data"]["measure"]["unitOfMeasure"]["infoSource"]["UUID"] = "cf3f3a8a-1e42-4f15-9288-9cf2241e163d";

    return objBOD.ToString(Newtonsoft.Json.Formatting.None);
}

double SimTemperature(int baseTemp)
{
    //Create a sim value
    Random myRandom = new Random();
    double randomValue = (double)myRandom.Next(1, 100);

    double simMeasurement = randomValue / 50 + baseTemp;

    return simMeasurement;  
}
