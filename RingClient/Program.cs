
using RingClient;

var ringClient = new Ring();
Console.WriteLine(await ringClient.FetchDevices());
