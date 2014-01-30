﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using NetworkCommsDotNet;
using DPSBase;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace DebugTests
{
    class BluetoothTest
    {
        static Guid ServiceGUID = new Guid("3a768eea-cbda-4926-a82d-831cb89092aa");
        
        /// <summary>
        /// Run example
        /// </summary>
        public static void RunExample()
        {
            NetworkComms.ConnectionEstablishTimeoutMS = int.MaxValue;

            SendReceiveOptions nullCompressionSRO = new SendReceiveOptions(DPSManager.GetDataSerializer<ProtobufSerializer>(), null, null);
            NetworkComms.DefaultSendReceiveOptions = nullCompressionSRO;

            //We need to define what happens when packets are received.
            //To do this we add an incoming packet handler for a 'Message' packet type. 
            //
            //We will define what we want the handler to do inline by using a lambda expression
            //http://msdn.microsoft.com/en-us/library/bb397687.aspx.
            //We could also just point the AppendGlobalIncomingPacketHandler method 
            //to a standard method (See AdvancedSend example)
            //
            //This handler will convert the incoming raw bytes into a string (this is what 
            //the <string> bit means) and then write that string to the local console window.
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", (packetHeader, connection, incomingString) => { Console.WriteLine("\n  ... Incoming message from " + connection.ToString() + " saying '" + incomingString + "'."); });

            //Start listenning
            BluetoothRadio defaultRadio = BluetoothRadio.PrimaryRadio;
            defaultRadio.Mode = RadioMode.Connectable;
            Connection.StartListening(ConnectionType.Bluetooth, new BluetoothEndPoint(defaultRadio.LocalAddress, ServiceGUID));

            //Print the address we are listening on to make sure everything
            //worked as expected.
            Console.WriteLine("Listening for messages on:");
            foreach (var localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.Bluetooth))
                Console.WriteLine("{0}", localEndPoint);

            //We loop here to allow any number of test messages to be sent and received
            while (true)
            {
                //Request a message to send somewhere
                Console.WriteLine("\nPlease enter your message and press enter (Type 'exit' to quit):");
                string stringToSend = Console.ReadLine();

                //If the user has typed exit then we leave our loop and end the example
                if (stringToSend == "exit") break;
                else
                {
                    //Once we have a message we need to know where to send it
                    //We have created a small wrapper class to help keep things clean here
                    ConnectionInfo targetServerConnectionInfo = new ConnectionInfo(new BluetoothEndPoint(new BluetoothAddress(0x0011B107A235L), ServiceGUID));
                    //GetServerDetails(out targetServerConnectionInfo);

                    //There are loads of ways of sending data (see AdvancedSend example for more)
                    //but the most simple, which we use here, just uses an IP address (string) and port (integer) 
                    //We pull these values out of the ConnectionInfo object we got above and voila!
                    var connection = BluetoothConnection.GetConnection(targetServerConnectionInfo);

                    connection.SendObject("Message", "Hello world");                    
                }
            }

            //We should always call shutdown on comms if we have used it
            NetworkComms.Shutdown();
        }

        private static void GetServerDetails(out ConnectionInfo connectionInfo)
        {
            ConsoleColor initColor = Console.ForegroundColor;

            Console.WriteLine("Please press 'Enter' to scan for bluetooth devices");

            if (Console.ReadKey(false).Key != ConsoleKey.Enter)
            {
                Console.ForegroundColor = initColor;
                GetServerDetails(out connectionInfo);
                return;
            }
            Console.WriteLine();

            BluetoothClient btc = new BluetoothClient();
            var devs = btc.DiscoverDevicesInRange().ToList();

            if (devs == null || devs.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No other listenning devices detected please check that the other device is listenning and try again");
                Console.ForegroundColor = initColor;
                GetServerDetails(out connectionInfo);
                return;
            }
            else
            {
                Console.WriteLine("Please select the device to connect to:");
                Console.ForegroundColor = ConsoleColor.Blue;
                int i = 0;

                foreach (var dev in devs)
                {
                    Console.WriteLine("\t{0}-{1}", i++, dev.DeviceName);
                }

                int selection;
                while (!(int.TryParse(Console.ReadKey(false).Key.ToString(), out selection)) && selection >= i)
                    continue;

                var selectedDev = devs[selection];

                connectionInfo = new ConnectionInfo(new BluetoothEndPoint(selectedDev.DeviceAddress, BluetoothService.SerialPort));
                Console.ForegroundColor = initColor;
                return;
            }
        }
    }
}