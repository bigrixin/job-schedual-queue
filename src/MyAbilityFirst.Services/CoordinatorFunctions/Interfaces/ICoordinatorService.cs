using Microsoft.WindowsAzure.Storage.Queue;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Models;
using System.Collections.Generic;

namespace MyAbilityFirst.Services.CoordinatorFunctions
{
	public interface ICoordinatorService
	{
		CoordinatorDetailsViewModel GetCoordinatorVM(Coordinator currentCoordinator);
		void UpdateProfile(Coordinator updatedCoordinator);
		void ApproveRating(int coordinatorID, int ratingID);
		void ApproveCareWorker(int coordinatorID, int careWorkerID);
		int GetContentsCountInQueue();
		string DeleteMessageInQueue(string queueName, CloudQueueMessage message);
		List<UrgentJobViewModel> GetJobsPostedOver4HoursListVM();
		void ProcessJobInQueue(int coordinatorID);
		UrgentJobViewModel GetUrgentJobDetailsVM(int coordinatorID, int jobID);
		void UpdatePendingJob(int coordinatorID, UrgentJobViewModel model);
		int PostedJobsOver4HoursProcess();
		void DeleteJobsInQueueWhenStatusIsNotUrgent();
	}
}
