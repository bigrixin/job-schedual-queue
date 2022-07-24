using AutoMapper;
using Microsoft.WindowsAzure.Storage.Queue;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Domain.ClientFunctions;
using MyAbilityFirst.Infrastructure.Data;
using MyAbilityFirst.Models;
using MyAbilityFirst.Services.ClientFunctions;
using MyAbilityFirst.Services.Common;
using MyAbilityFirst.Services.CoordinatorFunctions;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace MyAbilityFirst.Controllers
{

	[Authorize(Roles = "Coordinator")]
	public class CoordinatorController : Controller
	{

		#region Fields

		private readonly IMapper _mapper;
		private readonly ICoordinatorService _coordinatorServices;
		private readonly BookingData _bookingData;
		private readonly IQueueService _queueService;
		private readonly JobData _jobData;
		private readonly INotificationService _notificationService;

		#endregion

		#region Ctor

		public CoordinatorController(IMapper mapper, ICoordinatorService coordinatorServices, BookingData bookingData, JobData jobData, IQueueService queueService, INotificationService notificationService)
		{
			this._mapper = mapper;
			this._coordinatorServices = coordinatorServices;
			this._bookingData = bookingData;
			this._queueService = queueService;
			this._jobData = jobData;
			this._notificationService = notificationService;
		}

		#endregion

		#region profile

		[Authorize(Roles = "Coordinator")]
		[HttpGet, Route("coordinator/myaccount")]
		public ActionResult MyAccount(string usertype)
		{
			var currentCoordinator = this.GetLoggedInUser() as Coordinator;
			CoordinatorDetailsViewModel model = this._coordinatorServices.GetCoordinatorVM(currentCoordinator);
			return View(model);
		}


		[Authorize(Roles = "Coordinator")]
		[HttpGet, Route("coordinator/editprofile")]
		public ActionResult EditProfile()
		{
			var currentCoordinator = this.GetLoggedInUser() as Coordinator;
			CoordinatorDetailsViewModel model = this._coordinatorServices.GetCoordinatorVM(currentCoordinator);
			return View(model);
		}

		[Authorize(Roles = "Coordinator")]
		[HttpPost, Route("coordinator/editprofile")]
		public ActionResult EditProfile(CoordinatorDetailsViewModel model)
		{
			if (ModelState.IsValid)
			{
				Coordinator updatedCoordinator = _mapper.Map(model, (Coordinator)this.GetLoggedInUser());
				this._coordinatorServices.UpdateProfile(updatedCoordinator);
			}
			return RedirectToAction("MyAccount/");
		}

		#endregion

		#region client-rating-review

		[HttpGet, Route("coordinator/ReviewRatings")]
		public ActionResult ReviewRatings()
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			var bookingsVM = this._bookingData.GetBookingRatingsVMList(coordinator.ID);
			return View(bookingsVM);
		}

		[HttpGet, Route("coordinator/approverating/{ratingID:int}")]
		public ActionResult ApproveRating(int ratingID)
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			this._coordinatorServices.ApproveRating(coordinator.ID, ratingID);
			return RedirectToAction("ReviewRatings");
		}

		#endregion

		#region careworker-review

		[HttpGet, Route("coordinator/reviewcareworkers")]
		public ActionResult ReviewCareWorkers()
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			var careWorkersVM = this._bookingData.GetCareWorkerVMList(coordinator.ID);
			return View(careWorkersVM);
		}

		[HttpGet, Route("coordinator/approvecareworker/{careWorkerid:int}")]
		public ActionResult ApproveCareWorker(int careWorkerID)
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			this._coordinatorServices.ApproveCareWorker(coordinator.ID, careWorkerID);
			return RedirectToAction("ReviewCareWorkers");
		}

		#endregion

		#region job-status

		[HttpGet, Route("coordinator/mypendingtasks")]
		public ActionResult MyPendingTasks()
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			var jobsVM = this._jobData.GetUrgentJobsListVMByCoordinator(coordinator.ID);
			ViewBag.Total = this._coordinatorServices.GetContentsCountInQueue();
			return View(jobsVM);
		}

		[HttpGet, Route("coordinator/getpendingjob/")]
		public ActionResult GetJobFromQueue()
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			this._coordinatorServices.ProcessJobInQueue(coordinator.ID);
			return RedirectToAction("MyPendingTasks");
		}

		[HttpGet, Route("coordinator/processpendingjob")]
		public ActionResult ProcessPendingJob(int jobID)
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			UrgentJobViewModel jobsVM = new UrgentJobViewModel();
			jobsVM = this._coordinatorServices.GetUrgentJobDetailsVM(coordinator.ID, jobID);
			return View(jobsVM);
		}

		[HttpPost, Route("coordinator/processpendingjob")]
		public ActionResult ProcessPendingJob(UrgentJobViewModel model)
		{
			var coordinator = this.GetLoggedInUser() as Coordinator;
			if (ModelState.IsValid)
				this._coordinatorServices.UpdatePendingJob(coordinator.ID, model);
			return RedirectToAction("MyPendingTasks");
		}

		#endregion

		#region scheduler

		//this part need to change later for id Authentication
		[AllowAnonymous]
		[HttpPost, Route("executetask")]
		public ActionResult ExecuteTask(string name)
		{
			switch (name)
			{
				case "AddPostedJobsOver4HoursInQueue":
					int jobCount = this._coordinatorServices.PostedJobsOver4HoursProcess();
					if (jobCount != 0)
					{
						var subject = "There have " + jobCount + " jobs posted over 4 hour still not filled !";
						this._notificationService.SendEmailToAllCoordinators(subject);
					}
					break;
				case "ClearUpQueueForPostedJobsOver4Hours":
					this._coordinatorServices.DeleteJobsInQueueWhenStatusIsNotUrgent();
					break;
				case "BookingAlertFor24Hours":
					// booking service before arrive start 24 hours
					break;
				case "BookingAlertFor1Hour":
					// booking service before arrive 1 hours
					break;
				case "sendEmail":
					// testing for send alert email to coordinator when error, change later for admin
					var note = "The job scheduler service has error";
					this._notificationService.SendEmailToCoordinator(3, note);
					break;
				default:
					break;
			}
			return new EmptyResult();
		}

		#endregion

	}
}