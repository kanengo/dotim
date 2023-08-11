using Shared.Models;
using Xunit.Abstractions;

namespace Shared.Tests.models;


public class JwtHmacSha256Should
{   
    private readonly ITestOutputHelper _testOutput;

    private readonly JwtHmacSha256 _jwtHmacSha256 =
        new JwtHmacSha256(
            "7BhHJNInvjolpbMj2GvIBdTOz6X5tT8L2&p2WohELMqCeyBDL6kLUi11kytj#7lCF74&KhIdNo@QD5wWE#9$Zp#1*@M1zQaonX57wFBKptTDCY3i@qwV^IV9!N2aL!!#");

    public JwtHmacSha256Should(ITestOutputHelper testOutput) 
    { 
        _testOutput = testOutput; 
    } 
    [Fact]
    public string CreateJwt()
    {
        var token = _jwtHmacSha256.Create();
        
        _testOutput.WriteLine(token);
        
        return token;
    }
    
    [Fact]
    public void VerifyShould()
    {
        var token = _jwtHmacSha256.Create();
        
        Assert.True( _jwtHmacSha256.Verify(token));
        Assert.True( _jwtHmacSha256.Verify(token));
        
        token += "123";

        Assert.False(_jwtHmacSha256.Verify(token));
    }
}