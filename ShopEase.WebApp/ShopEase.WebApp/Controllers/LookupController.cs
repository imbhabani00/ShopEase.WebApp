using Microsoft.AspNetCore.Mvc;
using ShopEase.WebApp.Models.lookup;
using ShopEase.WebApp.Services;

namespace ShopEase.WebApp.Controllers
{
    public class LookupController : Controller
    {

        #region Properties
        private readonly ILookupService _lookupService;
        private readonly ILogger<LookupController> _logger;
        #endregion

        #region Constructor
        public LookupController(ILookupService lookupService, ILogger<LookupController> logger)
        {
            _lookupService = lookupService;
            _logger = logger;
        }
        #endregion

        #region GetRoles
        public async Task<IActionResult> GetRoles()
        {
            var result = new LookupList();
            try
            {
                result =  await _lookupService.GetRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while retriving the roles.");
            }
            return new ObjectResult(result);
        }
        #endregion
    }
}
