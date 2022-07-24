using System;
using System.ComponentModel.DataAnnotations;

namespace MyAbilityFirst.Models
{
	public class CareWorkerViewModel
	{
		public int CareWorkerID { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Suburb { get; set; }
		public string Email { get; set; }
		[Display(Name = "Photo")]
		public string PhotoURL { get; set; }

		public Double Distance { get; set; }
	}
}
