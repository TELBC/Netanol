// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using DotNetFlow.Netflow9;

var udpClient = new UdpClient();
var endPoint = new IPEndPoint(IPAddress.Loopback, 22055);

while (true)
{
    var header = new PacketHeader(2, 0, 0);

    var record = new TemplateRecord(256);
    record.Fields.Add(new Field(FieldType.IPv4SourceAddress));
    record.Fields.Add(new Field(FieldType.IPv4DestinationAddress));

    var template = new TemplateFlowSet();
    template.Records.Add(record);

    var data = new DataFlowSet(256);
    data.Records.Add(IPAddress.Parse("192.168.1.12"));
    data.Records.Add(IPAddress.Parse("10.5.12.254"));

    using (var ms = new MemoryStream())
    using (var nw = new NetflowWriter(ms))
    {
        nw.Write(header);
        nw.Write(template);
        nw.Write(data);

        byte[] packet = ms.ToArray();
        await udpClient.SendAsync(packet, packet.Length, endPoint);
    }

    // wait 10 seconds
    await Task.Delay(TimeSpan.FromSeconds(1));
    Console.WriteLine("Sending packet information");
}