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
        wssv.Start();

        Console.WriteLine("Art-Net server is running.");

        // Create a UDP client with the IP endpoint and port 0x1936
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0x1936);
        UdpClient udpClient = new UdpClient(remoteEP);

        // Loop to receive data
        while (true)
        {
           
            // Get the universe from the received data
            byte[] data = udpClient.Receive(ref remoteEP);

            int universe = data[14] | (data[15] << 8);

            universes[universe] = universe;

            // Extract the Art-Net data from the received data
            byte[] artnetData = new byte[data.Length - 18];
            Array.Copy(data, 18, artnetData, 0, artnetData.Length);

            // Convert the Art-Net data to hexadecimal string
            string hexData = BitConverter.ToString(artnetData).Replace("-", "");

            // Broadcast the Art-Net data to all clients connected to the /artnet service
            wssv.WebSocketServices["/artnet"].Sessions.Broadcast(
                    $"[{universe}, {hexData}]");
            
           
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
