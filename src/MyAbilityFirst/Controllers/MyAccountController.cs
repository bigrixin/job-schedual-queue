using AutoMapper;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Infrastructure.Auth;
using MyAbilityFirst.Models;
using MyAbilityFirst.Services.CareWorkerFunctions;
using MyAbilityFirst.Services.ClientFunctions;
using MyAbilityFirst.Services.Common;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MyAbilityFirst.Controllers
{
	[Authorize]
	public class MyAccountController : Controller
	{

		#region Fields

		private readonly IUserService _userService;
		private readonly ILoginService _loginServices;
		private readonly IClientService _clientServices;
		private readonly IMapper _mapper;
		private readonly ICareWorkerService _careWorkerServices;

		#endregion

		#region Ctor

		public MyAccountController(IUserService userService, ILoginService loginServices, IClientService clientServices, IMapper mapper, ICareWorkerService careWorkerServices)
		{
			this._userService = userService;
			this._loginServices = loginServices;
			this._clientServices = clientServices;
			this._mapper = mapper;
			this._careWorkerServices = careWorkerServices;
		}

		#endregion

		#region Actions

		// GET: MyAccount
		public ActionResult Index()
		{
			string userType = _loginServices.GetUserType(_loginServices.GetCurrentLoginIdentityID());

			// first time login, should redirect to edit profile
			switch (userType)
			{
				case "Admin":
					return RedirectToAction("Info", "Manage", new { usertype = userType });
				case "Client":
				case "CareWorker":
					if (_userService.FindUserByLoginID(_loginServices.GetCurrentLoginIdentityID()).Status == UserStatus.Registered)
						return RedirectToAction("EditProfile", userType, new { usertype = userType });
					else
						return RedirectToAction("MyAccount", userType, new { usertype = userType });
				case "Coordinator":
					return RedirectToAction("MyAccount", userType, new { usertype = userType });
			}

			return View();
		}

		[Authorize(Roles = "Admin")]
		public ActionResult ViewUsers()
		{
			var users = this._userService.GetAllUser();
			return View(users);
		}

		// GET: MyAccount/Details/5
		[Authorize(Roles = "Admin")]
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			User user = this._userService.FindUser(id);
			if (user == null)
			{
				return HttpNotFound();
			}
			return View(user);
		}

		[Authorize]
		[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		public ActionResult _MyAccountSidebar()
		{
			string loginID = _loginServices.GetCurrentLoginIdentityID();
			string userType = _loginServices.GetUserType(loginID);

			MyAccountViewModel vm = new MyAccountViewModel();
			vm.UserType = userType;
			vm.EmailVerified = _loginServices.EmailVerified(loginID);
			vm.UserName = _loginServices.GetCurrentLoginUserName();

			if (userType == "Client")
			{
				var client = this.GetLoggedInUser() as Client;
				vm.Bookings = client.GetBookings();
				vm.PatientList = _clientServices.RetrieveAllPatients(_userService.FindUserByLoginID(loginID).ID);
			}
			else if (userType == "CareWorker")
			{
				var careWorker = this.GetLoggedInUser() as CareWorker;
				vm.Bookings = careWorker.GetNewBookings();
				vm.Jobs = _careWorkerServices.GetJobsListViewModes(careWorker.ID).Count();
			}

			vm.NavSection = string.Format("{0}-{1}", this.ControllerContext.ParentActionViewContext.RouteData.Values["controller"].ToString(), this.ControllerContext.ParentActionViewContext.RouteData.Values["action"].ToString());
			return PartialView("_MyAccountSidebar", vm);
		}

		[AllowAnonymous]
		[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		public ActionResult _MyAccountTopbar()
		{
			MyAccountViewModel vm = new MyAccountViewModel();
			if (Request.IsAuthenticated)
			{
				string loginID = _loginServices.GetCurrentLoginIdentityID();
				string userType = _loginServices.GetUserType(loginID);
				if (userType == "Client" || userType == "CareWorker")
				{
					vm.Shortlists = _userService.RetrieveAllShortlists(this.GetLoggedInUser().ID).Where(s => s.Selected).Select(
					sl =>
					{
						ShortlistViewModel vm1 = new ShortlistViewModel();
						var user = _userService.FindUser(sl.SelectedUserID);
						vm1 = _mapper.Map(user, vm1);
						return vm1;
					}
				).ToList();
				}
			}
			return PartialView(vm);
		}

		#endregion
	}
}
