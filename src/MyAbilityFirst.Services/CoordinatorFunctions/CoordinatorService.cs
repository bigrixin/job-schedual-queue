using AutoMapper;
using Microsoft.WindowsAzure.Storage.Queue;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Domain.ClientFunctions;
using MyAbilityFirst.Infrastructure;
using MyAbilityFirst.Infrastructure.Data;
using MyAbilityFirst.Models;
using MyAbilityFirst.Services.ClientFunctions;
using MyAbilityFirst.Services.Common;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace MyAbilityFirst.Services.CoordinatorFunctions
{
	public class CoordinatorService : UserService, ICoordinatorService
	{

		#region Fields

		private readonly IWriteEntities _entities;
		private IMapper _mapper;
		private readonly INotificationService _notificationService;
		private readonly JobData _jobData;
		private readonly IQueueService _queueService;
		private double jobPostedDays = 7;
		private int messageNumber = 1;    //may change late
		private int minute = 1;           //may change late

		#endregion

		#region Ctor

		public CoordinatorService(IWriteEntities entities, IMapper mapper, INotificationService notificationService, JobData jobData, IQueueService queueService) : base(entities, mapper)
		{
			this._entities = entities;
			this._mapper = mapper;
			this._notificationService = notificationService;
			this._jobData = jobData;
			this._queueService = queueService;
		}

		#endregion

		#region profile

		public CoordinatorDetailsViewModel GetCoordinatorVM(Coordinator currentCoordinator)
		{
			CoordinatorDetailsViewModel model = new CoordinatorDetailsViewModel();
			model = _mapper.Map<Coordinator, CoordinatorDetailsViewModel>(currentCoordinator);
			return model;
		}


		public void UpdateProfile(Coordinator updatedCoordinator)
		{
			updatedCoordinator.Status = UserStatus.Active;
			updatedCoordinator.UpdatedAt = DateTime.Now;
			this._entities.Update(updatedCoordinator);
			this._entities.Save();
		}

		#endregion

		#region review-approval

		public void ApproveRating(int coordinatorID, int ratingID)
		{
			Rating currentRating = this._entities.Single<Rating>(a => a.ID == ratingID);
			currentRating.Status = RatingStatus.Approved;
			currentRating.ApprovedDate = DateTime.Now;
			currentRating.CoordinatorID = coordinatorID;
			var booking = this._entities.Single<Booking>(b => b.ID == currentRating.BookingID);
			var careWorker = this._entities.Single<CareWorker>(c => c.ID == booking.CareWorkerID);
			careWorker.AddOverallRating(currentRating.OverallScore);

			this._entities.Update(careWorker);
			this._entities.Save();
			// send Email to client
		}

		public void ApproveCareWorker(int coordinatorID, int careWorkerID)
		{
			var careWorker = this._entities.Single<CareWorker>(c => c.ID == careWorkerID);
			if (careWorker == null)
				throw new ArgumentNullException("careWorker");
			careWorker.ApprovedDate = DateTime.Now;
			careWorker.CoordinatorID = coordinatorID;
			careWorker.Status = UserStatus.Active;

			this._entities.Update(careWorker);
			this._entities.Save();
			// send Email to carer worker
		}

		#endregion

		#region job-queue

		public int GetContentsCountInQueue()
		{
			string queueName = ConfigurationManager.AppSettings["AzureQueueName_PostedJobsOver4Hours"];
			return this._queueService.GetQueueMessageCount(queueName);
		}

		public string DeleteMessageInQueue(string queueName, CloudQueueMessage message)
		{
			this._queueService.DeleteMessageInQueueWithID(queueName, message.Id, message.PopReceipt);
			return message.AsString;
		}

		public List<UrgentJobViewModel> GetJobsPostedOver4HoursListVM()
		{
			string queueName = ConfigurationManager.AppSettings["AzureQueueName_PostedJobsOver4Hours"];
			List<CloudQueueMessage> messages = this._queueService.GetMessagesFromQueue(queueName, messageNumber, minute);
			var jobVMList = getJobsListVMInQueue(messages);
			return jobVMList;
		}

		public void ProcessJobInQueue(int coordinatorID)
		{
			string queueName = ConfigurationManager.AppSettings["AzureQueueName_PostedJobsOver4Hours"];
			var coordinator = this._entities.Single<Coordinator>(a => a.ID == coordinatorID);
			if (coordinator == null)
				throw new ArgumentNullException("coordinator");

			List<string> messages = this._queueService.ProcessMessages(queueName, messageNumber, minute);
			foreach (var message in messages)
			{
				int jobID = Convert.ToInt32(message);
				var job = this._entities.Single<Job>(j => j.ID == jobID);
				if (job != null && message != null)
					coordinator.AddJob(job);
				this._entities.Update(coordinator);
				this._entities.Save();
			}
		}

		public UrgentJobViewModel GetUrgentJobDetailsVM(int coordinatorID, int jobID)
		{
			UrgentJobViewModel vm = new UrgentJobViewModel();
			Job retrievedJob = this._entities.Single<Job>(j => j.ID == jobID);
			var coordinatorJob = this._entities.Single<CoordinatorJob>(j => j.CoordinatorID == coordinatorID && j.JobID == jobID);
			if (retrievedJob == null)
				return null;

			if (retrievedJob.Status == JobStatus.Urgent)
			{
				vm = _mapper.Map<Job, UrgentJobViewModel>(retrievedJob);
				Client client = this._entities.Single<Client>(c => c.ID == vm.ClientId);
				if (client != null)
				{
					vm.ClientFirstName = client.FirstName;
					vm.ClientPhone = client.Phone;
					vm.ClientEmail = client.Email;
					vm.Comment = coordinatorJob.Comment;
					vm.UpdatedCommentAt = coordinatorJob.UpdatedAt;
				}
			}
			return vm;
		}

		public void UpdatePendingJob(int coordinatorID, UrgentJobViewModel model)
		{
			var coordinator = this._entities.Single<Coordinator>(c => c.ID == coordinatorID);
			var job = this._entities.Single<CoordinatorJob>(j => j.JobID == model.Id && j.CoordinatorID == coordinator.ID);
			job.Comment = model.Comment;
			coordinator.UpdateJob(job);
			this._entities.Update(coordinator);
			this._entities.Save();
		}

		#endregion

		#region scheduler

		public int PostedJobsOver4HoursProcess()
		{
			string queueName = ConfigurationManager.AppSettings["AzureQueueName_PostedJobsOver4Hours"];
			DateTime endDate = DateTime.Now.AddDays(-jobPostedDays);
			var jobs = this._entities.Get<Job>(j => j.CreatedAt < endDate && (j.Status == JobStatus.New || j.Status == JobStatus.Updated));
			int jobCounter = 0;
			foreach (var job in jobs)
			{
				if (job.ServiceAt < DateTime.Now)
				{
					job.Close();
					// send email to client
					// var subject = "The job scheduler service has error";
					// this._notificationService.SendEmailToClient(clientID, subject);
				}
				else
				{
					this._queueService.AddMessageInQueue(queueName, job.ID.ToString());
					job.Urgent();
					jobCounter++;
				}
				this._entities.Update(job);
			}
			this._entities.Save();
			return jobCounter;
		}

		public void DeleteJobsInQueueWhenStatusIsNotUrgent()
		{
			string queueName = ConfigurationManager.AppSettings["AzureQueueName_PostedJobsOver4Hours"];
			List<CloudQueueMessage> messages = this._queueService.GetMessagesFromQueue(queueName, messageNumber, minute);
			foreach (var message in messages)
			{
				int jobID = Convert.ToInt32(message.AsString);
				Job retrievedJob = this._entities.Single<Job>(j => j.ID == jobID);
				if (retrievedJob != null && retrievedJob.Status != JobStatus.Urgent)
					this._queueService.DeleteMessageInQueue(queueName, message);
			}
		}

		#endregion

		#region Helper

		private List<UrgentJobViewModel> getJobsListVMInQueue(List<CloudQueueMessage> messages)
		{
			List<UrgentJobViewModel> jobVMList = new List<UrgentJobViewModel>();
			foreach (var message in messages)
			{
				UrgentJobViewModel vm = new UrgentJobViewModel();
				int jobID = Convert.ToInt32(message.AsString);
				Job retrievedJob = this._entities.Single<Job>(j => j.ID == jobID);
				if (retrievedJob == null)
					return null;

				if (retrievedJob.Status == JobStatus.Urgent)
				{
					vm = _mapper.Map<Job, UrgentJobViewModel>(retrievedJob);
					Client client = this._entities.Single<Client>(c => c.ID == vm.ClientId);
					if (client != null)
					{
						vm.ClientFirstName = client.FirstName;
						vm.ClientPhone = client.Phone;
						vm.ClientEmail = client.Email;
						vm.QueueMessage = message;
					}
				}
				jobVMList.Add(vm);
			}

			return jobVMList;
		}

		#endregion
	}
}
