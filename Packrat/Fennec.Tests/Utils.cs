using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fennec.Tests;

public static class Utils
{
    public static IOptions<T> GetOptions<T>(T obj) where T : class 
    {
        var wrapper = Substitute.For<IOptions<T>>();
        wrapper.Value.Returns(obj);
        return wrapper;
    }
}