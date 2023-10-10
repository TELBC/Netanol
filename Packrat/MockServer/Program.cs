// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;

var udpClient = new UdpClient();
var endPoint = new IPEndPoint(IPAddress.Loopback, 2055);

var addresses = new[]
{
    new[] { "142.251.214.142", "2607:f8b0:4005:80f::200e" }, // youtube
    new[] { "13.107.42.14", "2620:1ec:21::14" }, // linkedin 
    new[] { "142.250.191.37", "2607:f8b0:4005:803::2005" }, // gmail
    new[] { "142.251.46.196", "2607:f8b0:4005:813::2004" }, // google maps
    new[] { "172.217.12.97", "2607:f8b0:4005:803::2001" }, // google drive
    new[] { "216.239.32.29", "2001:4860:4802:32::1d" } // google domains
};

while (true)
{
    var header = new PacketHeader(2, 0, 0);

    var record = new TemplateRecord(256);
    record.Fields.Add(new Field(FieldType.IPv4SourceAddress));
    record.Fields.Add(new Field(FieldType.IPv6SourceAddress));
    record.Fields.Add(new Field(FieldType.IPv4DestinationAddress));
    record.Fields.Add(new Field(FieldType.IPv6DestinationAddress));

    var template = new TemplateFlowSet();
    template.Records.Add(record);

    var data = new DataFlowSet(256);

    var source = new Random().Next(0, addresses.Length);
    var destination = new Random().Next(0, addresses.Length);

    data.Records.Add(IPAddress.Parse(addresses[source][0]));
    data.Records.Add(IPAddress.Parse(addresses[source][1]));
    data.Records.Add(IPAddress.Parse(addresses[destination][0]));
    data.Records.Add(IPAddress.Parse(addresses[destination][1]));

    using (var ms = new MemoryStream())
    using (var nw = new NetflowWriter(ms))
    {
        nw.Write(header);
        nw.Write(template);
        nw.Write(data);

        byte[] packet = ms.ToArray();
        await udpClient.SendAsync(packet, packet.Length, endPoint);
    }

    await Task.Delay(TimeSpan.FromSeconds(1));
    Console.WriteLine("Sending packet information");
}