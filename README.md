# Art-Net to WebSocket Server

This application receives Art-Net data on port `1936` and OSC data on port `8000` and transmits it over a WebSocket connection on `ws://localhost:8080`. The Art-Net data is broadcasted to the `/artnet` service, while the OSC data is broadcasted to the `/osc` service.

## Requirements

- .NET Framework 4.5 or later
- WebSocketSharp NuGet package

## Getting Started

1. Clone or download the repository
2. Open the project in Visual Studio
3. Build and run the project

## Usage

- Connect to the WebSocket server using a client such as wscat: `wscat -c ws://localhost:8080/artnet` or `wscat -c ws://localhost:8080/osc'
- Or connect via neos using the websocket component and write the data to a text field or write to string on `ws://localhost:8080/artnet` `ws://localhost:8080/osc`
- Start sending Art-Net or OSC data to the server's IP on the appropriate port
- Observe the received data being broadcasted to all connected clients on the specified service

## Notes

- The Art-Net universe number is used to determine which service to broadcast the data to. Universe `0x1936` is broadcasted to the `/artnet` service, while universe `0x2300` is broadcasted to the `/osc` service.
- The OSC data is transmitted as a string, encoded in UTF-8.
