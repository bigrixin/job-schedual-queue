using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyAbilityFirst.Models
{
	public class UrgentJobViewModel : JobViewModel
	{
		[Display(Name = "Client Phone")]
		public string ClientPhone { get; set; }
		[Display(Name = "Client Email")]
		public string ClientEmail { get; set; }
		public CloudQueueMessage QueueMessage { get; set; }
		[Display(Name = "My Comment")]
		public string Comment { get; set; }
		[Display(Name = "Comment Update")]
		public DateTime UpdatedCommentAt { get; set; }
	}
}