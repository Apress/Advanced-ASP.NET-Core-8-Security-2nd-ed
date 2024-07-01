using System;
using System.Collections.Generic;

namespace JuiceShopDotNet.Safe.Data.EncryptedDataStore;

public class EncryptedCreditApplication
{
    public int CreditApplicationID { get; set; }

    public string SocialSecurityNumber { get; set; } = null!;
}
