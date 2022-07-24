using MyAbilityFirst.Domain;
using MyAbilityFirst.Models;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;

namespace MyAbilityFirst.Infrastructure.Data
{
	public class JobData
	{

		#region Fields

		public readonly MyAbilityFirstDbContext _context;

		#endregion

		#region Ctor

		public JobData(MyAbilityFirstDbContext context)
		{
			this._context = context;
		}

		#endregion

		#region Helpers

		public List<JobViewModel> GetJobsVMListByCarer(Address carerAddress, int range)
		{
			var vmList =
			from j in this._context.Jobs
			join b in this._context.Clients on j.ClientId equals b.ID
			join sc in this._context.Subcategories on j.ServiceId equals sc.ID
			where (j.Status == JobStatus.New || j.Status == JobStatus.Updated || j.Status == JobStatus.Urgent)
			&& (DbGeography.FromText("POINT(" + j.Address.Longitude.ToString() + " " + j.Address.Latitude.ToString() + ")")
				 			.Distance(DbGeography.FromText("POINT(" + carerAddress.Longitude.ToString() + " " + carerAddress.Latitude.ToString()
						+ ")")).Value / 1000 < (double)range)
			orderby j.UpdatedAt descending
			select new JobViewModel
			{
				ClientId = j.ClientId,
				ClientFirstName = b.FirstName,
				CareWorkerID = j.CareWorkerId,
				Id = j.ID,
				Title = j.Title,
				Description = j.Description,
				Address = j.Address,
				ServicedAt = j.ServiceAt,
				ServiceId = j.ServiceId,
				Service = sc.Name,
				UpdatedAt = j.UpdatedAt,
				CreatedAt = j.CreatedAt,
				Distance = DbGeography.FromText("POINT(" + j.Address.Longitude.ToString() + " " + j.Address.Latitude.ToString() + ")")
				 			.Distance(DbGeography.FromText("POINT(" + carerAddress.Longitude.ToString() + " " + carerAddress.Latitude.ToString()
						+ ")")).Value / 1000
			};

			return vmList.ToList();
		}

		public List<CareWorker> GetPossibleCareWorkersForJob(int serviceID, decimal lat, decimal lng)
		{
			// serviceID will be used late for match service, currently can not found avaliable service ID in care worker ??
			var vmList =
				from cw in this._context.CareWorkers
				where (DbGeography.FromText("POINT(" + cw.Address.Longitude.ToString() + " " + cw.Address.Latitude.ToString() + ")")
								.Distance(DbGeography.FromText("POINT(" + lng.ToString() + " " + lat.ToString() + ")")).Value / 1000 <= cw.LocationCoverageRadius)
				//			&& (serviceID==cw.ServiceID)
				select cw;
			return vmList.ToList();
		}

		public List<CareWorkerViewModel> GetCareWorkerVMListByClient(Address carerAddress)
		{
			var vmList =
			from cw in this._context.CareWorkers
			select new CareWorkerViewModel
			{
				CareWorkerID = cw.ID,
				FirstName = cw.FirstName,
				LastName = cw.LastName,
				Suburb = cw.Address.Suburb,
				Email = cw.Email,
				//	PhotoURL = cw.PhotoURL,
				Distance = DbGeography.FromText("POINT(" + cw.Address.Longitude.ToString() + " " + cw.Address.Latitude.ToString() + ")")
				 			.Distance(DbGeography.FromText("POINT(" + carerAddress.Longitude.ToString() + " " + carerAddress.Latitude.ToString()
						+ ")")).Value / 1000
			};

			return vmList.OrderBy(a => a.Distance).ToList();
		}

		public List<UrgentJobViewModel> GetUrgentJobsListVMByCoordinator(int coordinatiorID)
		{
			var vmList =
			from job in this._context.Jobs
			join c in this._context.Clients on job.ClientId equals c.ID
			join co in this._context.CoordinatorJobs on job.ID equals co.JobID
			where co.CoordinatorID == coordinatiorID && job.Status == JobStatus.Urgent
			select new UrgentJobViewModel
			{
				Id = job.ID,
				ClientId = c.ID,
				ClientEmail = c.Email,
				ClientFirstName = c.FirstName,
				ClientPhone = c.Phone,
				Title = job.Title,
				Description = job.Description,
				ServicedAt = job.ServiceAt
			};
			return vmList.ToList();
		}

		#endregion
	}
}