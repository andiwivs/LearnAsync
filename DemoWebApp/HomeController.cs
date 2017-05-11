using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DemoWebApp
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                // Tip: use ConfigureAwait(false) to allow any IIS thread to complete the action
                var httpMessage = await client.GetAsync("http://www.google.co.uk").ConfigureAwait(false);

                var result = await httpMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                return Content(result);
            }
        }
    }
}