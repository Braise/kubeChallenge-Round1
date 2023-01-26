using System;
// using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

public class ExternalLoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    public ClaimsPrincipal Principal { get; set; } = default!;
}