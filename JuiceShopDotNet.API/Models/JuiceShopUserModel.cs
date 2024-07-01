using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.API.Models;

//There may be no real security advantage today to having a model with the exact properties as our model object
//Especially if we have one source and they need to sign the request with a digital signature
//We'll keep it separate in this demo partly to demonstrate best practices and partly to avoid mistakes if/when properties are added later
public partial class JuiceShopUserModel
{
    [Required]
    public int JuiceShopUserID { get; set; }

    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string NormalizedUserEmail { get; set; } = null!;
}
