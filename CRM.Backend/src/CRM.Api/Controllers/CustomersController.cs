// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for Customer management operations.
/// 
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Viewing and searching customers (individuals and organizations)
/// - Creating new customers with category-specific validation
/// - Updating customer information
/// - Linking contacts to organization customers
/// - Soft-deleting customers
/// 
/// TECHNICAL VIEW:
/// - Uses ICustomerService for business logic (dependency injected)
/// - All endpoints require authentication (JWT Bearer token)
/// - Returns standardized JSON responses with appropriate HTTP status codes
/// - Implements proper error handling with logging
/// 
/// API ROUTES:
/// - GET    /api/customers              - Get all customers
/// - GET    /api/customers/{id}         - Get customer by ID
/// - GET    /api/customers/individuals  - Get individual customers only
/// - GET    /api/customers/organizations - Get organization customers only
/// - GET    /api/customers/search/{term} - Search customers
/// - POST   /api/customers              - Create new customer
/// - PUT    /api/customers/{id}         - Update customer
/// - DELETE /api/customers/{id}         - Delete customer
/// - POST   /api/customers/{id}/contacts - Link contact to organization
/// 
/// INDUSTRY STANDARD ALIAS:
/// All routes are also available under /api/accounts for industry-standard naming.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Route("api/accounts")] // Industry-standard alias (Salesforce, HubSpot, Dynamics naming)
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    /// <summary>
    /// Initializes the controller with required services.
    /// </summary>
    /// <param name="customerService">Service for customer business logic</param>
    /// <param name="logger">Logger for error and audit logging</param>
    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers (both individuals and organizations).
    /// 
    /// FUNCTIONAL: Returns list of all active customers for dashboard views.
    /// TECHNICAL: Filters out soft-deleted records, returns 200 OK with array.
    /// </summary>
    /// <returns>Array of CustomerDto objects</returns>
    /// <response code="200">Returns the list of customers</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers");
            return StatusCode(500, new { message = "Error retrieving customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific customer by their unique ID.
    /// 
    /// FUNCTIONAL: Returns detailed customer information for viewing/editing.
    /// TECHNICAL: Returns 404 if customer not found or deleted.
    /// </summary>
    /// <param name="id">The unique customer identifier</param>
    /// <returns>CustomerDto if found</returns>
    /// <response code="200">Returns the customer</response>
    /// <response code="404">If customer not found</response>
    /// <response code="500">If there was an internal error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {Id}", id);
            return StatusCode(500, new { message = "Error retrieving customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Get only individual (non-organization) customers.
    /// 
    /// FUNCTIONAL: Filters to show only person-type customers.
    /// TECHNICAL: Filters by CustomerCategory.Individual.
    /// </summary>
    /// <returns>Array of individual CustomerDto objects</returns>
    /// <response code="200">Returns the list of individual customers</response>
    [HttpGet("individuals")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIndividuals()
    {
        try
        {
            var customers = await _customerService.GetIndividualCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving individual customers");
            return StatusCode(500, new { message = "Error retrieving customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Get only organization (company) customers.
    /// 
    /// FUNCTIONAL: Filters to show only company-type customers.
    /// TECHNICAL: Filters by CustomerCategory.Organization.
    /// </summary>
    /// <returns>Array of organization CustomerDto objects</returns>
    /// <response code="200">Returns the list of organization customers</response>
    [HttpGet("organizations")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations()
    {
        try
        {
            var customers = await _customerService.GetOrganizationCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization customers");
            return StatusCode(500, new { message = "Error retrieving customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Search customers
    /// </summary>
    [HttpGet("search/{searchTerm}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(string searchTerm)
    {
        try
        {
            var customers = await _customerService.SearchCustomersAsync(searchTerm);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers for {SearchTerm}", searchTerm);
            return StatusCode(500, new { message = "Error searching customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Get customers by lifecycle stage
    /// </summary>
    [HttpGet("by-stage/{stage}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLifecycleStage(CustomerLifecycleStage stage)
    {
        try
        {
            var customers = await _customerService.GetCustomersByLifecycleStageAsync(stage);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers by stage {Stage}", stage);
            return StatusCode(500, new { message = "Error retrieving customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Get customers by priority
    /// </summary>
    [HttpGet("by-priority/{priority}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPriority(CustomerPriority priority)
    {
        try
        {
            var customers = await _customerService.GetCustomersByPriorityAsync(priority);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers by priority {Priority}", priority);
            return StatusCode(500, new { message = "Error retrieving customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Get customers assigned to a user
    /// </summary>
    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAssignedUser(int userId)
    {
        try
        {
            var customers = await _customerService.GetCustomersByAssignedUserAsync(userId);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customers for user {UserId}", userId);
            return StatusCode(500, new { message = "Error retrieving customers", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        try
        {
            // Validate based on category
            if (dto.Category == CustomerCategory.Individual)
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
                    return BadRequest(new { message = "FirstName and LastName are required for Individual customers" });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Company))
                    return BadRequest(new { message = "Company name is required for Organization customers" });
            }

            var customer = await _customerService.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, new { message = "Error creating customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a customer
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            var customer = await _customerService.UpdateCustomerAsync(id, dto);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {Id}", id);
            return StatusCode(500, new { message = "Error updating customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (!result)
                return NotFound(new { message = "Customer not found" });
            return Ok(new { message = "Customer deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}", id);
            return StatusCode(500, new { message = "Error deleting customer", error = ex.Message });
        }
    }

    // === Direct Contact Management (One-to-Many) ===

    /// <summary>
    /// Get all contacts that directly belong to a customer (via CustomerId FK)
    /// </summary>
    [HttpGet("{id}/direct-contacts")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDirectContacts(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            var contacts = await _customerService.GetDirectContactsAsync(id);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving direct contacts for customer {Id}", id);
            return StatusCode(500, new { message = "Error retrieving direct contacts", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign a contact to a customer (sets the contact's CustomerId)
    /// </summary>
    [HttpPost("{id}/direct-contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignContactToCustomer(int id, int contactId)
    {
        try
        {
            var result = await _customerService.AssignContactToCustomerAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Customer or Contact not found" });

            return Ok(new { message = "Contact assigned to customer successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning contact {ContactId} to customer {Id}", contactId, id);
            return StatusCode(500, new { message = "Error assigning contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a contact from a customer (clears the contact's CustomerId)
    /// </summary>
    [HttpDelete("{id}/direct-contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignContactFromCustomer(int id, int contactId)
    {
        try
        {
            var result = await _customerService.UnassignContactFromCustomerAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Customer or Contact not found" });

            return Ok(new { message = "Contact removed from customer successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning contact {ContactId} from customer {Id}", contactId, id);
            return StatusCode(500, new { message = "Error unassigning contact", error = ex.Message });
        }
    }

    // === Contact Management for Organization Customers (Many-to-Many via CustomerContact) ===

    /// <summary>
    /// Get all contacts linked to a customer (organization)
    /// </summary>
    [HttpGet("{id}/contacts")]
    [ProducesResponseType(typeof(IEnumerable<CustomerContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerContacts(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            var contacts = await _customerService.GetCustomerContactsAsync(id);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts for customer {Id}", id);
            return StatusCode(500, new { message = "Error retrieving contacts", error = ex.Message });
        }
    }

    /// <summary>
    /// Link a contact to a customer (organization)
    /// </summary>
    [HttpPost("{id}/contacts")]
    [ProducesResponseType(typeof(CustomerContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkContact(int id, [FromBody] LinkContactToCustomerDto dto)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            if (customer.Category != "Organization")
                return BadRequest(new { message = "Can only link contacts to Organization customers" });

            var result = await _customerService.LinkContactToCustomerAsync(id, dto);
            if (result == null)
                return BadRequest(new { message = "Failed to link contact. Contact may not exist or is already linked." });

            _logger.LogInformation("Contact {ContactId} linked to customer {CustomerId}", dto.ContactId, id);
            return CreatedAtAction(nameof(GetCustomerContacts), new { id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking contact to customer {Id}", id);
            return StatusCode(500, new { message = "Error linking contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a customer contact relationship
    /// </summary>
    [HttpPut("{id}/contacts/{contactId}")]
    [ProducesResponseType(typeof(CustomerContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerContact(int id, int contactId, [FromBody] UpdateCustomerContactDto dto)
    {
        try
        {
            var result = await _customerService.UpdateCustomerContactAsync(id, contactId, dto);
            if (result == null)
                return NotFound(new { message = "Customer contact relationship not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId} for customer {Id}", contactId, id);
            return StatusCode(500, new { message = "Error updating contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Unlink a contact from a customer
    /// </summary>
    [HttpDelete("{id}/contacts/{contactId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkContact(int id, int contactId)
    {
        try
        {
            var result = await _customerService.UnlinkContactFromCustomerAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Customer contact relationship not found" });

            _logger.LogInformation("Contact {ContactId} unlinked from customer {CustomerId}", contactId, id);
            return Ok(new { message = "Contact unlinked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking contact {ContactId} from customer {Id}", contactId, id);
            return StatusCode(500, new { message = "Error unlinking contact", error = ex.Message });
        }
    }

    /// <summary>
    /// Set primary contact for a customer
    /// </summary>
    [HttpPost("{id}/contacts/{contactId}/set-primary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryContact(int id, int contactId)
    {
        try
        {
            var result = await _customerService.SetPrimaryContactAsync(id, contactId);
            if (!result)
                return NotFound(new { message = "Customer contact relationship not found" });

            _logger.LogInformation("Contact {ContactId} set as primary for customer {CustomerId}", contactId, id);
            return Ok(new { message = "Primary contact set successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary contact {ContactId} for customer {Id}", contactId, id);
            return StatusCode(500, new { message = "Error setting primary contact", error = ex.Message });
        }
    }
}
