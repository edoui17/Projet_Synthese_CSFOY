using Core.Utils;
using Xunit;

namespace UnitTests.Core.Utils;

public class LoginValidatorTests
{
    [Theory]
    [InlineData("", "password123", false, "L'identifiant ne peut pas être vide.")]
    [InlineData("  ", "password123", false, "L'identifiant ne peut pas être vide.")]
    [InlineData("ab", "password123", false, "L'identifiant doit contenir au moins 3 caractères.")]
    [InlineData("abc", "", false, "Le mot de passe ne peut pas être vide.")]
    [InlineData("abc", "  ", false, "Le mot de passe ne peut pas être vide.")]
    [InlineData("user", "password123", true, null)]
    public void Validate_ShouldReturnExpectedResult(string p_username, string p_password, bool p_expectedValid, string? p_expectedError)
    {
        // Act
        var (isValid, errorMessage) = LoginValidator.Validate(p_username, p_password);

        // Assert
        Assert.Equal(p_expectedValid, isValid);
        Assert.Equal(p_expectedError, errorMessage);
    }
}
