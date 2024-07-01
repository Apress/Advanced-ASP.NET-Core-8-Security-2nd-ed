using System;
using System.Collections.Generic;

namespace JuiceShopDotNet.API.Data;

public partial class JuiceShopUser
{
    public int JuiceShopUserID { get; set; }

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string NormalizedUserEmail { get; set; } = null!;
}
