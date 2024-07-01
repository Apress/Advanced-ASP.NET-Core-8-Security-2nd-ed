using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JuiceShopDotNet.Safe.CSP;

[HtmlTargetElement("script", Attributes = "add-nonce")]
public class ScriptTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _contextAccessor;

    [HtmlAttributeName("add-nonce")]
    public bool AddNonce { get; set; }

    public ScriptTagHelper(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (AddNonce)
        {
            var nonceService = _contextAccessor.HttpContext.RequestServices.GetService<NonceContainer>();
            output.Attributes.SetAttribute("nonce", nonceService.ID);
        }
    }
}

