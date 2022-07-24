using AutoMapper;
using Microsoft.AspNet.Identity;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Infrastructure;
using MyAbilityFirst.Infrastructure.Auth;
using MyAbilityFirst.Models;
using MyAbilityFirst.Services.ClientFunctions;
using MyAbilityFirst.Services.Common;
using NWebsec.Mvc.HttpHeaders.Csp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MyAbilityFirst.Controllers
{
	[Authorize(Roles = "Client")]
	[Csp(Enabled = false)]
	public class JobController : Controller
	{

		#region Fields

		private readonly IWriteEntities _entities;
		private readonly ILoginService _loginServices;
		private readonly IClientService _clientServices;
		private readonly IUploadService _uploadServices;
		private readonly IPresentationService _viewModelServices;
		private readonly IMapper _mapper;

		#endregion

		#region Ctor

		public JobController(IWriteEntities entities, ILoginService loginServices, IClientService clientServices, IUploadService uploadServices, IPresentationService viewModelServices, IMapper mapper)
		{
			this._entities = entities;
			this._loginServices = loginServices;
			this._clientServices = clientServices;
			this._uploadServices = uploadServices;
			this._viewModelServices = viewModelServices;
			this._mapper = mapper;
		}

		#endregion

		#region Actions

		[HttpGet, Route("jobs")]
		public ActionResult Jobs(string sortOrder, int? page)
		{
			var client = this.GetLoggedInUser() as Client;
			var jobsVM = this._clientServices.GetJobsListVM(client.ID);
			ViewBag.Total = jobsVM.ToList().Count();
			int pageIndex = 1;
			int pageSize = 5;
			int pageNumber = (page ?? 1);
			pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

			jobsVM = selectSorting(jobsVM, sortOrder);
			return View(jobsVM.ToPagedList(pageNumber, pageSize));
		}

		[HttpGet, Route("job/create")]
		public ActionResult Create()
		{
			JobViewModel model = new JobViewModel();
			var client = this.GetLoggedInUser() as Client;
			model.GenderDropDownList = _viewModelServices.GetSubCategorySelectList("Gender");
			model.ServiceDropDownList = _viewModelServices.GetSubCategorySelectList("JobService");
			model.PatientDropDownList = _viewModelServices.GetPatientSelectList(client.ID);
			ViewBag.PathUpload = "/Job/UploadFileToAzure";
			ViewBag.PathDelete = "/Job/DeleteFileFromAzure";
			return View(model);
		}

		[HttpPost, Route("job/create")]
		[ValidateAntiForgeryToken]
		public ActionResult Create(JobViewModel model, IEnumerable<HttpPostedFileBase> files)
		{
			if (ModelState.IsValid)
			{
				var client = this.GetLoggedInUser() as Client;

				Job newJob = this._clientServices.PostNewJob(client.ID, model);
				return RedirectToAction("Details/" + newJob.ID.ToString());
			}

			return View(model);
		}

		[HttpGet, Route("job/edit/{id:int}")]
		public ActionResult Edit(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var client = this.GetLoggedInUser() as Client;
			var job = client.PostedJobs.SingleOrDefault(j => j.ID == id);

			if (job == null)
				return RedirectToAction("MyAccount", "Client");

			JobViewModel model = mapJobToJobViewModel(job);
			ViewBag.pictureURL = model.PictureURL;
			return View(model);
		}

		[HttpPost, Route("job/edit/{id:int}")]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(JobViewModel model, IEnumerable<HttpPostedFileBase> files)
		{
			if (ModelState.IsValid)
			{
				var client = this.GetLoggedInUser() as Client;

				this._clientServices.EditJob(client.ID, model);
				return RedirectToAction("Jobs", "Job");
			}

			return View(model);
		}

		[HttpGet, Route("job/details/{id:int}")]
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var client = this.GetLoggedInUser() as Client;
			Job job = client.PostedJobs.SingleOrDefault(j => j.ID == id);

			if (job == null)
				return RedirectToAction("MyAccount", "Client");

			JobViewModel model = mapJobToJobViewModel(job);

			return View(model);
		}

		[Authorize(Roles = "Client, Admin")]
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var client = this.GetLoggedInUser() as Client;
			Job job = client.PostedJobs.Where(j => j.ID == id).SingleOrDefault();

			if (job == null)
				return RedirectToAction("MyAccount", "Client");

			JobViewModel model = mapJobToJobViewModel(job);

			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			var client = this.GetLoggedInUser() as Client;
			this._clientServices.DeleteJob(client.ID, id);
			return RedirectToAction("Jobs");
		}


		//upload file to Azure storage blob
		[HttpPost]
		public virtual ActionResult UploadFileToAzure()
		{
			string path = ConfigurationManager.AppSettings["uploadAzurePath_Job"];
			HttpPostedFileBase file = Request.Files[0];

			string url = this._uploadServices.UploadToAzureStorage(file, path);

			return attachmentProcess(url);
		}

		[HttpDelete]
		public virtual ActionResult DeleteFileFromAzure(string fileName)
		{
			string path = ConfigurationManager.AppSettings["uploadAzurePath_Job"];
			if (this._uploadServices.DeleteFromAzureStorage(fileName, path))
				return Json(new { message = "The file has delete !" }, "text/html");
			else
				return Json(new { message = "Error" }, "text/html");
		}

		#endregion

		#region Helper

		private List<JobViewModel> selectSorting(List<JobViewModel> jobsVM, string sortOrder)
		{
			ViewBag.CurrentSort = sortOrder;
			ViewBag.PatientSortParm = String.IsNullOrEmpty(sortOrder) ? "patient_desc" : "";
			ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
			ViewBag.AddressSortParm = sortOrder == "address" ? "addressdesc" : "address";
			ViewBag.StatusSortParm = sortOrder == "status" ? "statusdesc" : "status";
			switch (sortOrder)
			{
				case "patient_desc":
					jobsVM = jobsVM.OrderByDescending(s => s.PatientId).ToList();
					break;
				case "title_desc":
					jobsVM = jobsVM.OrderByDescending(s => s.Title).ToList();
					break;
				case "address":
					jobsVM = jobsVM.OrderBy(s => s.Address.Suburb).ToList();
					break;
				case "addressdesc":
					jobsVM = jobsVM.OrderByDescending(s => s.Address.Suburb).ToList();
					break;
				case "status":
					jobsVM = jobsVM.OrderBy(s => s.Status).ToList();
					break;
				case "statusdesc":
					jobsVM = jobsVM.OrderByDescending(s => s.Status).ToList();
					break;
				default:
					jobsVM = jobsVM.OrderBy(s => s.Title).ToList();
					break;
			}
			return jobsVM;
		}

		private Client getLoggedInClient()
		{
			var LoginIdentityId = User.Identity.GetUserId();
			var client = _clientServices.RetrieveClientByLoginID(_loginServices.GetCurrentLoginIdentityID());
			return client;
		}

		private JobViewModel mapJobToJobViewModel(Job job)
		{
			JobViewModel model = new JobViewModel();
			model = _mapper.Map<Job, JobViewModel>(job);
			return model;
		}

		private JsonResult attachmentProcess(string url)
		{
			bool isUploaded = false;
			if (url != null)
			{
				isUploaded = true;
				string message = "100% complete";

				return Json(new
				{
					statusCode = 200,
					status = "File uploaded.",
					file = url,
					isUploaded = isUploaded,
					message = message
				}, "text/html");

			}
			else
			{
				string message = "Error";
				return Json(new
				{
					statusCode = 500,
					status = "Error uploading image.",
					file = string.Empty,
					isUploaded = isUploaded,
					message = message
				}, "text/html");
			}
		}
		#endregion

	}
}