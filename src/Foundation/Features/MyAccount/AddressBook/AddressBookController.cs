using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Customer.ViewModels;
using Foundation.Commerce.Models.Pages;
using System.Web.Mvc;

namespace Foundation.Features.MyAccount.AddressBook
{
    [Authorize]
    public class AddressBookController : PageController<AddressBookPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAddressBookService _addressBookService;
        private readonly LocalizationService _localizationService;


        public AddressBookController(
            IContentLoader contentLoader,
            IAddressBookService addressBookService,
            LocalizationService localizationService)
        {
            _contentLoader = contentLoader;
            _addressBookService = addressBookService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public ActionResult Index(AddressBookPage currentPage)
        {
            return View(_addressBookService.GetAddressBookViewModel(currentPage));
        }

        [HttpGet]
        public ActionResult EditForm(AddressBookPage currentPage, string addressId)
        {
            var viewModel = new AddressViewModel(currentPage)
            {
                Address = new AddressModel
                {
                    AddressId = addressId,
                },
                CurrentContent = currentPage
            };

            _addressBookService.LoadAddress(viewModel.Address);

            return AddressEditView(viewModel);
        }

        [ChildActionOnly]
        public PartialViewResult AddNewAddress(string multiShipmentUrl)
        {
            var startPage = _contentLoader.Get<PageData>(ContentReference.StartPage) as CommerceHomePage;
            var addressBookPage = _contentLoader.Get<PageData>(startPage.AddressBookPage) as AddressBookPage;
            var model = new AddressViewModel(addressBookPage)
            {
                Address = new AddressModel()
            };
            _addressBookService.LoadAddress(model.Address);
            ViewData["IsInMultiShipment"] = true;
            ViewData["MultiShipmentUrl"] = multiShipmentUrl;

            return PartialView("EditAddress", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRegionsForCountry(string countryCode, string region, string htmlPrefix)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = htmlPrefix;
            var countryRegion = new CountryRegionViewModel
            {
                RegionOptions = _addressBookService.GetRegionsByCountryCode(countryCode),
                Region = region
            };

            return PartialView("_AddressRegion", countryRegion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(AddressViewModel viewModel, string returnUrl = "")
        {
            if (string.IsNullOrEmpty(viewModel.Address.Name))
            {
                ModelState.AddModelError("Address.Name", _localizationService.GetString("/Shared/Address/Form/Empty/Name", "Name is required"));
            }

            if (!_addressBookService.CanSave(viewModel.Address))
            {
                ModelState.AddModelError("Address.Name", _localizationService.GetString("/AddressBook/Form/Error/ExistingAddress", "An address with the same name already exists"));
            }

            if (!ModelState.IsValid)
            {
                _addressBookService.LoadAddress(viewModel.Address);

                return AddressEditView(viewModel);
            }

            _addressBookService.Save(viewModel.Address);

            if (Request.IsAjaxRequest())
            {
                return Json(viewModel.Address);
            }

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });

            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(string addressId)
        {
            _addressBookService.Delete(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPreferredShippingAddress(string addressId)
        {
            _addressBookService.SetPreferredShippingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPreferredBillingAddress(string addressId)
        {
            _addressBookService.SetPreferredBillingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        public ActionResult OnSaveException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<AddressBookPage>();

            var viewModel = new AddressViewModel
            {
                Address = new AddressModel
                {
                    AddressId = filterContext.HttpContext.Request.Form["addressId"],
                    ErrorMessage = filterContext.Exception.Message,
                },
                CurrentContent = currentPage
            };

            _addressBookService.LoadAddress(viewModel.Address);

            return AddressEditView(viewModel);
        }

        private ActionResult AddressEditView(AddressViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView("~/Features/MyAccount/AddressBook/ModalAddressDialog.cshtml", viewModel);
            }

            return View("~/Features/MyAccount/AddressBook/EditForm.cshtml", viewModel);
        }

        private CommerceHomePage GetStartPage() => _contentLoader.Get<PageData>(ContentReference.StartPage) as CommerceHomePage;

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetRegions(string countryCode, string region, string inputName)
        {
            var countryRegion = new CountryRegionViewModel
            {
                RegionOptions = _addressBookService.GetRegionsByCountryCode(countryCode),
                Region = region
            };
            ViewData["Name"] = inputName;
            return PartialView("~/Features/Shared/Foundation/DisplayTemplates/CountryRegionViewModel.cshtml", countryRegion);
        }
    }
}