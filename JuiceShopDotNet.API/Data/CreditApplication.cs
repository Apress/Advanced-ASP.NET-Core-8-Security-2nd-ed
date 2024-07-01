using System;
using System.Collections.Generic;

namespace JuiceShopDotNet.API.Data;

public partial class CreditApplication
{
    public int CreditApplicationID { get; set; }

    public string SocialSecurityNumber { get; set; } = null!;
}
