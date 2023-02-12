/* Purpose: This is a simple application that acts as an ISBM publication provider.
 *          It demonstrates the idea of using open standards to publish messages
 *          using an ISBM adapter. This .net core UWP app interacts with an ISBM adapter that's
 *          compatible with ISBM 2.0. It should be interoperable with other ISBM adapters
 *          regardless of the actual service bus that delivers the messages.  
 *          
 * Author: Claire Wong
 * Date Created:  2023/01/05
 * 
 * (c) 2023
 * This code is licensed under MIT license
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using ISBM20ClientAdapter;
using ISBM20ClientAdapter.ResponseType;
using ISBM20ClientAdapter.Enums;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ISBM_Temp_Sensor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    
    public sealed partial class MainPage : Page
    {
        string templateBOD = "";

        DispatcherTimer timerTemperature;
        DispatcherTimer timerPublish;
        DispatcherTimer timerInitialize;
       
        I2cDevice MCP9808;

        //ISBM Provider Publication Service
        ProviderPublicationService myProviderPublicationService = new ProviderPublicationService();

        public MainPage()
        {
            this.InitializeComponent();

            textBlockPublishStatus.Text = "";
            buttonPublish.IsEnabled = false;

            // Delay Start for timer initializations.
            // Let all the page initialization complete first.
            timerInitialize = new DispatcherTimer();
            timerInitialize.Interval = TimeSpan.FromMilliseconds(100);
            timerInitialize.Tick += TimerInitialize_Tick;
            timerInitialize.Start();
        }

        private async void TimerInitialize_Tick(object sender, object e)
        {
            timerInitialize.Stop();
            // Initalize temperature reading timer.
            await IntializeMCP9808Async();
            // Initial message publication timer.
            await IntializePublishAsync();
        }

        private void TimerTemperature_Tick(object sender, object e)
        {
            try
            {
                byte[] registerPointer = new byte[1];
                byte[] temperatureData = new byte[2];

                // Register Pointer (Hex) 
                registerPointer[0] = 0x05;

                // Read temperature data from sensor using register pointer 0x05
                MCP9808.WriteRead(registerPointer, temperatureData);

                // MSB - Most Significant Bits
                byte MSB = temperatureData[0];
                // LSB - Least Significant Bits
                byte LSB = temperatureData[1];

                // Keep the lower 5 bits for most significant bits,
                // The higher 3 bits are for status only.
                // Please see MCP9808 data sheet.
                MSB = Convert.ToByte(MSB & 0x1F);

                // Assign MSB byte value to upperByte in floating point.
                float upperByte = MSB;
                // Assign LSB byte value to lowerByte in floating point 
                float lowerByte = LSB;

                // For Temperature >= 0 Celsius,
                //T emperature = UpperByte x 2^4 + LowerByte x 2^-4
                // Please see MCP9808 data sheet for Temperature < 0 Celsius.
                float displayTemp = upperByte * 16 + lowerByte / 16;
                // Round display temperature to 2 decimal places.
                Double roundedDisplayTemp = Math.Round(displayTemp, 2, MidpointRounding.AwayFromZero);
                
                textBlockValue.Text = roundedDisplayTemp.ToString();
            }
            catch (Exception)
            {

            }
        }

        private async void TimerPublish_Tick(object sender, object e)
        {
            // Pause publish timer when publishing message.
            timerPublish.Stop();
            textBlockPublishStatus.Text = "Publishing";
            textBoxStatusCode.Text = "";
            textBoxMessage.Text = "";
            textBoxResponse.Text = "";
            // Publish message.
            await PublishBOD();

            
        }

        private async Task PublishBOD()
        {
            
            await Task.Delay(100);
            // Load BOD template into Newtonsoft JObject.
            JObject objBOD = JObject.Parse(templateBOD);

            // Setup BOD and CCOM values.
            objBOD["syncMeasurements"]["applicationArea"]["bODID"] = System.Guid.NewGuid().ToString();
            objBOD["syncMeasurements"]["applicationArea"]["creationDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["UUID"] = System.Guid.NewGuid().ToString();
            objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["recorded"]["dateTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            objBOD["syncMeasurements"]["dataArea"]["measurements"][0]["measurement"][0]["data"]["measure"]["value"] = textBlockValue.Text;

            try
            {
                                
                PostPublicationResponse myPostPublicationResponse = myProviderPublicationService.PostPublication(textBoxHostName.Text, textBoxSessionId.Text, textBoxTopic.Text, objBOD.ToString(Newtonsoft.Json.Formatting.None));
              
                textBoxResponse.Text = myPostPublicationResponse.ReasonPhrase;
                textBoxStatusCode.Text = myPostPublicationResponse.StatusCode.ToString();

                if (myPostPublicationResponse.StatusCode == 201)
                {
                    textBoxMessage.Text = myPostPublicationResponse.MessageID;
                }
                else
                {
                    textBoxMessage.Text = "";
                }
                
            }
            catch (Exception)
            {
                // Your exception handling code here.
            }

            //Start publish timer again, if the end user didn't push the the button to stop message publication.
            if ((string)buttonPublish.Content != "Publish")
            {
                textBlockPublishStatus.Text = "Idle";
                // Publish completed, start publish timer again.
                timerPublish.Start();
            }
            else
            {
                textBlockPublishStatus.Text = "";
                buttonPublish.IsEnabled = true;
            }
        }

        private async Task IntializeMCP9808Async()
        {
            
            string myIcCDeviceSelector = I2cDevice.GetDeviceSelector();
            // Get all my devices information.
            IReadOnlyList<DeviceInformation> myDevices = await DeviceInformation.FindAllAsync(myIcCDeviceSelector);

            // According to Datasheet, default address of MCP9808 is 0x18 
            I2cConnectionSettings myMCP9808Settings = new I2cConnectionSettings(0x18);
            
            // Get my temperature sensor device. I only have the MCP9808 on the I2c address bus. Device "0" should be my MCP9808.
            MCP9808 = await I2cDevice.FromIdAsync(myDevices[0].Id, myMCP9808Settings);

            // Initialize temperature timer.
            timerTemperature = new DispatcherTimer();
            timerTemperature.Interval = TimeSpan.FromMilliseconds(1000);
            timerTemperature.Tick += TimerTemperature_Tick;
            //Start temperature timer.
            timerTemperature.Start();
        }

        private async Task IntializePublishAsync()
        {
            
            // Get default message template.
            string filename = @"Assets\SyncMeasurements.json";
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(filename);
            string JsonFromFile = await Windows.Storage.FileIO.ReadTextAsync(file);
            templateBOD = JsonFromFile;

            // Initialize Pusblish timer.
            timerPublish = new DispatcherTimer();
            timerPublish.Interval = TimeSpan.FromMilliseconds(10000);
            timerPublish.Tick += TimerPublish_Tick;
        }
               
        private void ButtonPublish_Click(object sender, RoutedEventArgs e)
        {
            // Start Publish timer if button contnent is "Stop"
            if ((string)buttonPublish.Content == "Publish")
            {
                timerPublish.Start();
                buttonPublish.Content = "Stop";
                buttonConnect.IsEnabled = false;
                textBlockPublishStatus.Text = "Idle";
            }
            else
            {
                timerPublish.Stop();
                buttonPublish.Content = "Publish";
                buttonConnect.IsEnabled = true;
                if (textBlockPublishStatus.Text == "Idle")
                {
                    textBlockPublishStatus.Text = "";
                }
                else
                {
                    buttonPublish.IsEnabled = false;
                }
            }
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            // Open a publication session if button content is "Connect".
            if ((string)buttonConnect.Content == "Connect")
            {
                //Open Publication Session
                OpenPublicationSessionResponse myOpenPublicationSessionResponse = myProviderPublicationService.OpenPublicationSession(textBoxHostName.Text, textBoxChannelId.Text, ServerType.Azure);
                
                try
                {
                    textBoxResponse.Text = myOpenPublicationSessionResponse.ReasonPhrase;
                    textBoxStatusCode.Text = myOpenPublicationSessionResponse.StatusCode.ToString();
                    if (myOpenPublicationSessionResponse.StatusCode == 201)
                    {
                        textBoxSessionId.Text = myOpenPublicationSessionResponse.SessionID;

                        buttonConnect.Content = "Disconnect";
                        buttonPublish.IsEnabled = true;
                    }
                }
                catch (Exception)
                {
                    textBoxSessionId.Text = "";
                }
            }
            else
            {
                //Close Publication Session 
                ClosePublicationSessionResponse myClosePublicationSessionResponse = myProviderPublicationService.ClosePublicationSession(textBoxHostName.Text, textBoxSessionId.Text);

                textBoxResponse.Text = myClosePublicationSessionResponse.ReasonPhrase;
                textBoxStatusCode.Text = myClosePublicationSessionResponse.StatusCode.ToString();

                if (myClosePublicationSessionResponse.StatusCode == 204)
                {
                    textBoxSessionId.Text = "";

                    buttonConnect.Content = "Connect";
                    buttonPublish.IsEnabled = false;

                }
            }
        }
    }
}
