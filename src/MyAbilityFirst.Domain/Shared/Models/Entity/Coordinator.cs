using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAbilityFirst.Domain
{
	public class Coordinator : User
	{
		#region Properties

		public int OrganisationId { get; set; }
		public DateTime? StartingDate { get; set; }
		public virtual ICollection<CoordinatorJob> Jobs { get; set; }

		#endregion

		#region Ctor

		protected Coordinator()
		{
			// required by EF
			this.Jobs = new List<CoordinatorJob>();
		}

		public Coordinator(string aspNetIdentity) : base(aspNetIdentity)
		{
			this.Jobs = new List<CoordinatorJob>();
		}

		#endregion

		#region job

		public CoordinatorJob AddJob(Job jobData)
		{
			CoordinatorJob job = new CoordinatorJob(OrganisationId, jobData.ID);
			var existing = GetJob(jobData.ID);
			if (existing == null)
				this.Jobs.Add(job);
			return job;
		}

		public CoordinatorJob GetJob(int ID)
		{
			return this.Jobs.Where(a => a.ID == ID).SingleOrDefault();
		}

		public CoordinatorJob UpdateJob(CoordinatorJob jobData)
		{
			var existing = this.GetJob(jobData.ID);
			if (existing != null)
			{
				this.Jobs.Remove(existing);
				this.Jobs.Add(jobData);
				return jobData;
			}
			return null;
		}

		#endregion
	}
}
