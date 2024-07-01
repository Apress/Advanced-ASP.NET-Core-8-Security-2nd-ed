using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuiceShopDotNet.Common.PaymentProcessor;

public class PaymentSimulator
{
    public static PaymentResult Pay(PaymentInfo info)
    {
        return new PaymentResult() { PaymentID = Guid.NewGuid(), Result = PaymentResult.ActualResult.Succeeded };
    }
}
