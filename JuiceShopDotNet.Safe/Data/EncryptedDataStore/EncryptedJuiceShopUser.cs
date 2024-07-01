using System;
using System.Collections.Generic;

namespace JuiceShopDotNet.Safe.Data.EncryptedDataStore;

public partial class EncryptedJuiceShopUser
{
    public int JuiceShopUserID { get; set; }

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string NormalizedUserEmail { get; set; } = null!;
}
