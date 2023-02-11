using System;
using System.Net;
using System.Net.Sockets;
using WebSocketSharp.Server;
using WebSocketSharp;

class ArtNetServer
{
    // A WebSocket server instance
    static WebSocketServer wssv;

    // An array to store universes
    static int[] universes = new int[512];

    public static void Main()
    {
        // Create a WebSocket server instance
        wssv = new WebSocketServer("ws://localhost:8080");

        // Add the ArtNetService and OSCService WebSocket services to the server instance
        wssv.AddWebSocketService<ArtNetService>("/artnet");
        wssv.AddWebSocketService<OSCService>("/osc");
        wssv.Start();

        Console.WriteLine("Art-Net server is running.");

        // Create a UDP client with the IP endpoint and port 0x1936
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0x1936);
        UdpClient udpClient = new UdpClient(remoteEP);

        // Loop to receive data
        while (true)
        {
            // Receive data from the remote endpoint
            byte[] data = udpClient.Receive(ref remoteEP);

            // Get the universe from the received data
            int universe = data[14] | (data[15] << 8);
            universes[universe] = universe;

            // Handle Art-Net data
            if (universe == 0x1936)
            {
                // Extract the Art-Net data from the received data
                byte[] artnetData = new byte[data.Length - 18];
                Array.Copy(data, 18, artnetData, 0, artnetData.Length);

                // Convert the Art-Net data to hexadecimal string
                string hexData = BitConverter.ToString(artnetData).Replace("-", "");

                // Broadcast the Art-Net data to all clients connected to the /artnet service
                wssv.WebSocketServices["/artnet"].Sessions.Broadcast(
                    $"[{universe}, {hexData}]");
            }
            // Handle OSC data
            else if (universe == 0x2300)
            {
                // Extract the OSC data from the received data
                string oscData = System.Text.Encoding.UTF8.GetString(data, 18, data.Length - 18);

                // Broadcast the OSC data to all clients connected to the /osc service
                wssv.WebSocketServices["/osc"].Sessions.Broadcast(oscData);
            }
        }
    }
}



    class ArtNetService : WebSocketBehavior
{
    protected override void OnOpen()
    {
        Console.WriteLine("A WebSocket client connected.");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Console.WriteLine("A WebSocket client disconnected.");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("A WebSocket client sent a message: " + e.Data);
        Send(e.Data);
    }
}
class OSCService : WebSocketBehavior
{
    protected override void OnOpen()
    {
        Console.WriteLine("A WebSocket client connected to the /osc service.");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Console.WriteLine("A WebSocket client disconnected from the /osc service.");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        Console.WriteLine("A WebSocket client sent a message to the /osc service: " + e.Data);
        Send(e.Data);
    }
}