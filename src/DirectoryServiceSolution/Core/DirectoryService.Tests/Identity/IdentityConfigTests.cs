using DirectoryService.Infrastructure.Identity.Configuration;

namespace DirectoryService.Tests.Identity;

public sealed class IdentityConfigTests
{
    [Fact]
    public void Key_Of_Exactly_Minimum_Length_Is_Accepted()
    {
        string key = new('a', IdentityConfig.MinJwtHashKeyBytes);

        IdentityConfig config = new(key);

        Assert.Equal(key, config.JwtHashkey);
    }

    [Fact]
    public void Key_Longer_Than_Minimum_Is_Accepted()
    {
        string key = new('a', IdentityConfig.MinJwtHashKeyBytes + 16);

        IdentityConfig config = new(key);

        Assert.Equal(key, config.JwtHashkey);
    }

    [Fact]
    public void Key_One_Byte_Shorter_Than_Minimum_Is_Rejected()
    {
        string key = new('a', IdentityConfig.MinJwtHashKeyBytes - 1);

        Assert.Throws<ApplicationException>(() => new IdentityConfig(key));
    }

    [Fact]
    public void Short_Key_Is_Rejected()
    {
        Assert.Throws<ApplicationException>(() => new IdentityConfig("secret"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_Key_Is_Rejected(string key)
    {
        Assert.Throws<ApplicationException>(() => new IdentityConfig(key));
    }

    [Fact]
    public void Key_Length_Is_Measured_In_Bytes_Not_Characters()
    {
        string key = new('я', 20);

        Assert.Equal(20, key.Length);
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(key) >= IdentityConfig.MinJwtHashKeyBytes);

        IdentityConfig config = new(key);

        Assert.Equal(key, config.JwtHashkey);
    }

    [Fact]
    public void Multibyte_Key_Shorter_Than_Minimum_In_Bytes_Is_Rejected()
    {
        string key = new('я', 15);

        Assert.True(System.Text.Encoding.UTF8.GetByteCount(key) < IdentityConfig.MinJwtHashKeyBytes);

        Assert.Throws<ApplicationException>(() => new IdentityConfig(key));
    }
}
