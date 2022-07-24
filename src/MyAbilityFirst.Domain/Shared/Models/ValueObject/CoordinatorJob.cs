using System;

namespace MyAbilityFirst.Domain
{
	public class CoordinatorJob
	{
		public int ID { get; set; }
		public int CoordinatorID { get; set; }
		public int JobID { get; set; }
		public bool Active { get; set; }
		public string Comment { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		#region Ctor

		protected CoordinatorJob()
		{
			// required by EF
		}

		public CoordinatorJob(int coordinatorID, int jobID)
		{
			this.CoordinatorID = coordinatorID;
			this.JobID = jobID;
			this.CreatedAt = DateTime.Now;
			this.UpdatedAt = DateTime.Now;
		}

		public CoordinatorJob(int coordinatorID, int jobID, string comment)
		{
			this.CoordinatorID = coordinatorID;
			this.JobID = jobID;
			this.Comment = comment;
			this.CreatedAt = DateTime.Now;
			this.UpdatedAt = DateTime.Now;
		}

		#endregion



	}


}