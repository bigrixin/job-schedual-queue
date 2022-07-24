using Microsoft.AspNet.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyAbilityFirst.Domain
{
	public class Job
	{

		#region Properties

		public int ID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }

		public Address Address { get; set; }
		public int GenderId { get; set; }
		public int ServiceId { get; set; }

		public DateTime? ServiceAt { get; set; }
		public JobStatus Status { get; set; }
		public string PictureURL { get; set; }

		public int ClientId { get; private set; }
		public int PatientId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int? CareWorkerId { get; private set; }

		private bool CanBeUpdated => !(this.Status == JobStatus.Cancelled || this.Status == JobStatus.Completed);
		private bool CanBeFilled => this.Status == JobStatus.Pending;
		private bool CanBeCancelled => this.Status != JobStatus.Completed;
		private bool CanBeCompleted => this.Status == JobStatus.Filled;
		private bool CanBeRejected => this.Status == JobStatus.Pending || this.Status == JobStatus.Interested;
		private bool CanBeInterested => this.Status == JobStatus.New || this.Status == JobStatus.Updated || this.Status == JobStatus.Urgent;
		private bool CanBePending => this.Status == JobStatus.Interested;
		private bool CanBeUrgent => this.Status == JobStatus.New || this.Status == JobStatus.Updated;
		private bool CanBeClosed => this.Status == JobStatus.New || this.Status == JobStatus.Updated;


		#endregion

		#region Ctor

		protected Job()
		{
			// required by EF
			this.Address = new Address();
		}

		public Job(int clientId)
		{
			this.ClientId = clientId;
			this.Address = new Address();
		}

		#endregion

		public bool Cancel()
		{
			if (this.CanBeCancelled)
			{
				this.Status = JobStatus.Cancelled;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Pending()
		{
			if (this.CanBePending)
			{
				this.Status = JobStatus.Pending;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Filled()
		{
			if (this.CanBeFilled)
			{
				this.Status = JobStatus.Filled;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Reject()
		{
			if (this.CanBeRejected)
			{
				this.Status = JobStatus.Updated;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Complete()
		{
			if (this.CanBeCompleted)
			{
				this.Status = JobStatus.Completed;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Urgent()
		{
			if (this.CanBeUrgent)
			{
				this.Status = JobStatus.Urgent;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Close()
		{
			if (this.CanBeUrgent)
			{
				this.Status = JobStatus.Closed;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;
		}

		public bool Interested()
		{
			if (this.CanBeInterested)
			{
				this.Status = JobStatus.Interested;
				this.UpdatedAt = DateTime.Now;
				return true;
			}
			return false;

		}
	}

}
