using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

public class ITSMCatalogControllerTests
{
    private readonly Mock<IServiceCatalogService> _mockService;
    private readonly CatalogController _controller;

    public ITSMCatalogControllerTests()
    {
        _mockService = new Mock<IServiceCatalogService>();
        _controller = new CatalogController(_mockService.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // GET /items
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCatalogItems_ShouldReturnOk()
    {
        var items = new List<CatalogItemDto>
        {
            new() { CatalogItemId = 1, Name = "Laptop Request" }
        };
        _mockService.Setup(s => s.GetCatalogItemsAsync(null, null)).ReturnsAsync(items);

        var result = await _controller.GetCatalogItems(null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<CatalogItemDto>>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCatalogItems_ShouldPassCategoryFilter()
    {
        var items = new List<CatalogItemDto>();
        _mockService.Setup(s => s.GetCatalogItemsAsync(5, null)).ReturnsAsync(items);

        var result = await _controller.GetCatalogItems(5);

        _mockService.Verify(s => s.GetCatalogItemsAsync(5, null), Times.Once);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /items/{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCatalogItem_ShouldReturnOk_WhenItemExists()
    {
        var item = new CatalogItemDto { CatalogItemId = 1, Name = "VPN Access" };
        _mockService.Setup(s => s.GetCatalogItemByIdAsync(1)).ReturnsAsync(item);

        var result = await _controller.GetCatalogItem(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(item);
    }

    [Fact]
    public async Task GetCatalogItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        _mockService.Setup(s => s.GetCatalogItemByIdAsync(999)).ReturnsAsync((CatalogItemDto?)null);

        var result = await _controller.GetCatalogItem(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCatalogRequest_ShouldReturnOk_WithRequestId()
    {
        var dto = new CreateCatalogRequestDto { CatalogItemId = 1, RequestedForId = 1 };
        _mockService.Setup(s => s.CreateCatalogRequestAsync(dto, 1)).ReturnsAsync(42);

        var result = await _controller.CreateCatalogRequest(dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        // Controller returns anonymous { requestId = 42 }
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/my
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyRequests_ShouldReturnOk_WithCurrentUserRequests()
    {
        var requests = new List<CatalogRequest>
        {
            new() { RequestId = 1, CatalogItemId = 1, RequestedById = 1 }
        };
        _mockService.Setup(s => s.GetMyRequestsAsync(1)).ReturnsAsync(requests);

        var result = await _controller.GetMyRequests();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<CatalogRequest>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /search
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchCatalog_ShouldReturnOk()
    {
        var items = new List<CatalogItemDto>
        {
            new() { CatalogItemId = 1, Name = "Monitor" }
        };
        _mockService.Setup(s => s.SearchCatalogAsync("monitor")).ReturnsAsync(items);

        var result = await _controller.SearchCatalog("monitor");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /featured
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedItems_ShouldReturnOk_WithFeaturedOnly()
    {
        var items = new List<CatalogItemDto> { new() { CatalogItemId = 1, Name = "Featured" } };
        _mockService.Setup(s => s.GetCatalogItemsAsync(null, true)).ReturnsAsync(items);

        var result = await _controller.GetFeaturedItems();

        _mockService.Verify(s => s.GetCatalogItemsAsync(null, true), Times.Once);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /categories
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_ShouldReturnOk()
    {
        var categories = new List<CatalogCategoryInfo>
        {
            new() { CategoryId = 1, Name = "Hardware", ItemCount = 5 }
        };
        _mockService.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _controller.GetCategories();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests/for-others
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRequestForOthers_ShouldReturnOk_WithRequestId()
    {
        _mockService
            .Setup(s => s.CreateCatalogRequestForOthersAsync(
                It.IsAny<CRM.Core.Interfaces.ITSM.CreateCatalogRequestForOthersDto>(), 1))
            .ReturnsAsync(99);

        var dto = new CreateCatalogRequestForOthersRequest
        {
            CatalogItemId = 1,
            RequestedForUserId = 42,
            Notes = "For new hire"
        };
        var result = await _controller.CreateRequestForOthers(dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/{requestId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequestById_ShouldReturnOk_WhenExists()
    {
        var request = new CatalogRequest { RequestId = 1, CatalogItemId = 5 };
        _mockService.Setup(s => s.GetRequestByIdAsync(1)).ReturnsAsync(request);

        var result = await _controller.GetRequestById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(request);
    }

    [Fact]
    public async Task GetRequestById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        _mockService.Setup(s => s.GetRequestByIdAsync(999)).ReturnsAsync((CatalogRequest?)null);

        var result = await _controller.GetRequestById(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // DELETE /requests/{requestId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelRequest_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.CancelRequestAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.CancelRequest(1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelRequest_ShouldReturnBadRequest_WhenFails()
    {
        _mockService.Setup(s => s.CancelRequestAsync(1, 1)).ReturnsAsync(false);

        var result = await _controller.CancelRequest(1);

        var badReq = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badReq.Value.Should().Be("Unable to cancel request");
    }
}
