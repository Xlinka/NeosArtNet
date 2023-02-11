using System;
using System.Net;
using System.Net.Sockets;
using WebSocketSharp.Server;
using WebSocketSharp;

class ArtNetServer
{
    static WebSocketServer wssv;
    static int[] universes = new int[512];

    public static void Main()
    {
        wssv = new WebSocketServer("ws://localhost:8080");
        wssv.AddWebSocketService<ArtNetService>("/artnet");
        wssv.Start();

        Console.WriteLine("Art-Net server is running.");

        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0x1936);
        UdpClient udpClient = new UdpClient(remoteEP);

        while (true)
        {
            byte[] data = udpClient.Receive(ref remoteEP);

            int universe = data[14] | (data[15] << 8);
            universes[universe] = universe;

            // Handle Art-Net data
            if (universe == 0x1936)
            {
                byte[] artnetData = new byte[data.Length - 18];
                Array.Copy(data, 18, artnetData, 0, artnetData.Length);

                string hexData = BitConverter.ToString(artnetData).Replace("-", "");

                wssv.WebSocketServices["/artnet"].Sessions.Broadcast(
                    $"[{universe}, {hexData}]");
            }
            // Handle OSC data
            else if (universe == 0x2300)
            {
                string oscData = System.Text.Encoding.UTF8.GetString(data, 18, data.Length - 18);
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