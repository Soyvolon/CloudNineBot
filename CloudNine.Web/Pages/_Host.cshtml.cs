using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudNine.Web
{
    public class HostModel : PageModel
    {
        public bool HasConsent { get; private set; }

        public HostModel()
        {

        }

        public void OnGet()
        {
            var consentFeature = HttpContext.Features.Get<ITrackingConsentFeature>();
            HasConsent = !consentFeature?.HasConsent ?? false;
        }
    }
}