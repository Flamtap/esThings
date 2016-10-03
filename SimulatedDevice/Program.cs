﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    public class Program
    {
        public static DeviceClient DeviceClient;
        //IoT Hub host name
        public static string IoTHubUri = "esThings.azure-devices.net";
        //Device key generated by CreateDeviceIdentity
        public static string DeviceId = "esDevice";
        public static string DeviceKey = "I15Nn4DW9KAWOqsWh/4+6HxsaWbQn2rQzY8TSBSzYi8=";

        /// <summary>
        /// For the sake of simplicity, this class does not implement any retry policy.
        /// In production code, one should implement a retry policy such as exponential backoff.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/library/hh675232.aspx"/>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting simulated device...\n");

            DeviceClient = DeviceClient.Create(IoTHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey));

            SendDeviceToCloudMessagesAsync();
            SendDeviceToCloudInteractiveMessagesAsync();

            Console.ReadLine();
        }
        
        private static async void SendDeviceToCloudMessagesAsync()
        {
            const double avgWindSpeed = 10d; //milliseconds
            Random rand = new Random();

            while (true)
            {
                double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

                var telemetryDataPoint = new {deviceId = DeviceId, windspeed = currentWindSpeed};

                string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                Message d2CMessage = new Message(Encoding.ASCII.GetBytes(messageString));

                await DeviceClient.SendEventAsync(d2CMessage);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageString}");

                Task.Delay(1000).Wait();
            }
        }

        private static async void SendDeviceToCloudInteractiveMessagesAsync()
        {
            while (true)
            {
                string interactiveMessageString = "Alert message!";

                Message interactiveMessage = new Message(Encoding.ASCII.GetBytes(interactiveMessageString));
                interactiveMessage.Properties["messageType"] = "interactive";
                interactiveMessage.MessageId = Guid.NewGuid().ToString();

                await DeviceClient.SendEventAsync(interactiveMessage);

                Console.WriteLine($"{DateTime.Now} > Sending interactive message: {interactiveMessageString}");

                Task.Delay(10000).Wait();
            }
        }
    }
}
