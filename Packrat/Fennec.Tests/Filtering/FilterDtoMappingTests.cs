using System.Net;
using AutoMapper;
using Fennec.Database;
using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;

namespace Fennec.Tests.Filtering;

public class FilterDtoMappingTests
{
    private readonly IMapper _mapper = SetupAutoMapper(); // Call the method that sets up AutoMapper

    private static IMapper SetupAutoMapper()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MapperProfile());
        });
        return mappingConfig.CreateMapper();
    }

    [Fact]
    public void DtoToEntityConversion()
    {
        // Arrange
        var dto = new FilterConditionDto(
            "192.168.1.1",
            "255.255.255.0",
            "8080",
            "10.0.0.1",
            "255.255.255.0",
            "80",
            "Tcp",
            true);

        // Act
        var entity = _mapper.Map<FilterCondition>(dto); // Assuming FilterCondition is your domain model

        // Assert
        Assert.Equal(IPAddress.Parse(dto.SourceAddress).GetAddressBytes(), entity.SourceAddress);
        Assert.Equal(IPAddress.Parse(dto.DestinationAddress).GetAddressBytes(), entity.DestinationAddress);
        Assert.Equal(ushort.Parse(dto.SourcePort!), entity.SourcePort);
        Assert.Equal(ushort.Parse(dto.DestinationPort!), entity.DestinationPort);
        Assert.Equal(Enum.Parse<TraceProtocol>(dto.Protocol!), entity.Protocol);
        Assert.Equal(dto.Include, entity.Include);
    }
    
    [Fact]
    public void EntityToDtoConversion()
    {
        // Arrange
        var entity = new FilterCondition(
            sourceAddress: new byte[] { 192, 168, 1, 1 },
            sourceAddressMask: new byte[] { 255, 255, 255, 0 },
            sourcePort: 8080,
            destinationAddress: new byte[] { 10, 0, 0, 1 },
            destinationAddressMask: new byte[] { 255, 255, 255, 0 },
            destinationPort: 80,
            protocol: TraceProtocol.Tcp,
            include: true);

        // Act
        var dto = _mapper.Map<FilterConditionDto>(entity);

        // Assert
        Assert.Equal(new IPAddress(entity.SourceAddress).ToString(), dto.SourceAddress);
        Assert.Equal(new IPAddress(entity.DestinationAddress).ToString(), dto.DestinationAddress);
        Assert.Equal(entity.SourcePort.ToString(), dto.SourcePort);
        Assert.Equal(entity.DestinationPort.ToString(), dto.DestinationPort);
        Assert.Equal(entity.Protocol.ToString(), dto.Protocol);
        Assert.Equal(entity.Include, dto.Include);
    }

    [Fact]
    public void RoundTripConversion()
    {
        // Arrange
        var originalDto = new FilterConditionDto(
            "192.168.1.100",
            "255.255.255.0",
            "8080",
            "10.1.1.1",
            "255.255.255.0",
            "443",
            "Udp",
            false);

        // Act
        var entity = _mapper.Map<FilterCondition>(originalDto);
        var roundTripDto = _mapper.Map<FilterConditionDto>(entity);

        // Assert
        Assert.Equal(originalDto.SourceAddress, roundTripDto.SourceAddress);
        Assert.Equal(originalDto.DestinationAddress, roundTripDto.DestinationAddress);
        Assert.Equal(originalDto.SourcePort, roundTripDto.SourcePort);
        Assert.Equal(originalDto.DestinationPort, roundTripDto.DestinationPort);
        Assert.Equal(originalDto.Protocol, roundTripDto.Protocol);
        Assert.Equal(originalDto.Include, roundTripDto.Include);
    }

}