using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuiceShopDotNet.Common.PaymentProcessor;

public class PaymentResult
{
    public enum ActualResult
    { 
        Succeeded,
        Failed
    }

    public Guid? PaymentID { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public ActualResult Result { get; set; }
}
