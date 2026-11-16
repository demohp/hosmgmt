using ClinicManagement.Web.Pages.Patient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace ClinicManagement.UnitTests.Pages.Patient;

/// <summary>Unit tests for AppointmentRequestSentModel page model.</summary>
public class AppointmentRequestSentModelTests
{
    [Fact]
    public void Constructor_CreatesInstance_Successfully()
    {
        // Arrange & Act
        var model = new AppointmentRequestSentModel();

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void OnGet_DoesNotThrow()
    {
        // Arrange
        var model = new AppointmentRequestSentModel();
        var httpContext = new DefaultHttpContext();
        model.PageContext = new PageContext { HttpContext = httpContext };

        // Act
        var exception = Record.Exception(() => model.OnGet());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OnGet_ReturnsVoid_WithoutException()
    {
        // Arrange
        var model = new AppointmentRequestSentModel();
        var httpContext = new DefaultHttpContext();
        model.PageContext = new PageContext { HttpContext = httpContext };

        // Act & Assert - should not throw
        model.OnGet();
        Assert.True(true); // Confirms execution reached here
    }

    [Fact]
    public void Model_IsPageModel()
    {
        // Arrange & Act
        var model = new AppointmentRequestSentModel();

        // Assert
        Assert.IsAssignableFrom<PageModel>(model);
    }
}
