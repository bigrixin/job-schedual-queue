using AutoMapper;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Domain.ClientFunctions;
using MyAbilityFirst.Infrastructure;
using MyAbilityFirst.Infrastructure.Data;
using MyAbilityFirst.Models;
using MyAbilityFirst.Services.CareWorkerFunctions;
using MyAbilityFirst.Services.Common;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MyAbilityFirst.Services.ClientFunctions
{
	public class ClientService : UserService, IClientService
	{

		#region Fields

		private readonly IWriteEntities _entities;
		private readonly IMapper _mapper;
		private readonly BookingData _bookingData;
		private readonly INotificationService _notificationService;
		private readonly ICareWorkerService _careWorkerService;

		#endregion

		#region Ctor

		public ClientService(IWriteEntities entities, IMapper mapper, BookingData bookingData, INotificationService notificationService, ICareWorkerService careWorkerService) : base(entities, mapper)
		{
			this._entities = entities;
			this._mapper = mapper;
			this._bookingData = bookingData;
			this._notificationService = notificationService;
			this._careWorkerService = careWorkerService;
		}

		#endregion

		#region IClientService

		public Client RetrieveClient(int clientID)
		{
			return this._entities.Single<Client>(c => c.ID == clientID);
		}

		public Client RetrieveClientByLoginID(string identityId)
		{
			return this._entities.Single<Client>(c => c.LoginIdentityId == identityId);
		}

		public Client UpdateClient(Client clientData)
		{
			clientData.Status = UserStatus.Active;
			clientData.UpdatedAt = DateTime.Now;
			this._entities.Update(clientData);
			this._entities.Save();
			return clientData;
		}

		public Patient CreatePatient(int clientID, Patient patientData)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				Patient newPatient = parentClient.AddNewPatient(patientData);
				this._entities.Update(parentClient);
				this._entities.Save();
				return newPatient;
			}
			return null;
		}

		public Patient RetrievePatient(int clientID, int patientID)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				return parentClient.GetExistingPatient(patientID);
			}
			return null;
		}

		public List<Patient> RetrieveAllPatients(int clientID)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				return parentClient.GetAllExistingPatients();
			}
			return null;
		}

		public Patient UpdatePatient(int clientID, Patient patientData)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				parentClient.UpdateExistingPatient(patientData);
				this._entities.Update(parentClient);
				this._entities.Save();
				return patientData;
			}
			return null;

		}

		public Contact CreateContact(int clientID, int patientID, Contact contactData)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				Patient parentPatient = parentClient.GetExistingPatient(patientID);
				if (parentPatient != null)
				{
					Contact newContact = parentPatient.AddNewContact(contactData);
					parentClient.UpdateExistingPatient(parentPatient);
					this._entities.Update(parentClient);
					this._entities.Save();
					return newContact;
				}
				return null;
			}
			return null;
		}

		public Contact RetrieveContact(int clientID, int patientID, int contactID)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				Patient parentPatient = parentClient.GetExistingPatient(patientID);
				if (parentPatient != null)
				{
					Contact contact = parentPatient.GetExistingContact(contactID);
					return contact;
				}
				return null;
			}
			return null;
		}

		public List<Contact> RetrieveAllContacts(int clientID, int patientID)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				Patient parentPatient = parentClient.GetExistingPatient(patientID);
				if (parentPatient != null)
				{
					var contacts = parentPatient.GetAllExistingContacts();
					return contacts;
				}
				return null;
			}
			return null;
		}

		public Contact UpdateContact(int clientID, int patientID, Contact contactData)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				Patient parentPatient = parentClient.GetExistingPatient(patientID);
				if (parentPatient != null)
				{
					Contact existingContact = RetrieveContact(clientID, patientID, contactData.ID);
					_mapper.Map(contactData, existingContact);
					parentPatient.UpdateExistingContact(existingContact);
					parentClient.UpdateExistingPatient(parentPatient);
					this._entities.Update(existingContact);
					this._entities.Save();
					return existingContact;
				}
				return null;
			}
			return null;
		}

		public bool DeleteContact(int clientID, int patientID, int contactID)
		{
			Client parentClient = RetrieveClient(clientID);
			if (parentClient != null)
			{
				Patient parentPatient = parentClient.GetExistingPatient(patientID);
				if (parentPatient != null)
				{
					Contact contact = parentPatient.RemoveContact(contactID);
					this._entities.Delete(contact);
					this._entities.Save();
					return true;
				}
				return false;
			}
			return false;
		}

		public List<Contact> ReplaceAllContacts(int clientID, int patientID, List<Contact> contacts)
		{
			List<Contact> existingContacts = RetrieveAllContacts(clientID, patientID) ?? new List<Contact>();
			contacts = contacts ?? new List<Contact>();
			IEnumerable<Contact> actionablecontacts = contacts.Except(existingContacts, new ContactComparer());
			// CREATE new Contact if old dataset doesn't have new contacts
			foreach (Contact contact in actionablecontacts)
			{
				contact.PatientID = patientID;
				CreateContact(clientID, contact.PatientID, contact);
			}
			// UPDATE Contact if already exists
			actionablecontacts = contacts.Intersect(existingContacts, new ContactComparer());
			foreach (Contact contact in actionablecontacts)
			{
				UpdateContact(clientID, contact.PatientID, contact);
			}
			// DELETE old Contact if new dataset doesn't have old contacts
			actionablecontacts = existingContacts.Except(contacts, new ContactComparer());
			foreach (Contact contact in actionablecontacts)
			{
				DeleteContact(clientID, contact.PatientID, contact.ID);
			}
			return contacts;
		}

		public Job PostNewJob(int ownerClientId, JobViewModel model)
		{
			if (ownerClientId < 1)
				throw new ArgumentException("ownerClientId must be an ID greater than 1.");

			var client = this._entities.Single<Client>(c => c.ID == ownerClientId);
			if (client == null)
				throw new ArgumentNullException("client");

			var now = DateTime.Now;
			var clientId = client.ID;

			var job = new Job(clientId);
			job = _mapper.Map<JobViewModel, Job>(model);
			job.CreatedAt = now;
			job.UpdatedAt = now;
			job.Status = JobStatus.New;

			client.PostedJobs.Add(job);

			this._entities.Update(client);
			this._entities.Save();

			return job;
		}

		public void EditJob(int ownerClientId, JobViewModel model)
		{
			if (ownerClientId < 1)
				return;

			var client = this._entities.Single<Client>(c => c.ID == ownerClientId);
			if (client == null)
				throw new ArgumentNullException("client");

			var now = DateTime.Now;
			var clientId = client.ID;

			Job job = client.PostedJobs.SingleOrDefault<Job>(j => j.ID == model.Id);

			if (job != null)
			{
				job = _mapper.Map<JobViewModel, Job>(model, job);
				job.UpdatedAt = now;
				job.Status = JobStatus.Updated;

				this._entities.Update(client);
				this._entities.Save();
			}
		}

		public void DeleteJob(int ownerClientId, int jobId)
		{
			if (ownerClientId < 1)
				return;

			var client = this._entities.Single<Client>(c => c.ID == ownerClientId);
			if (client == null)
				throw new ArgumentNullException("client");

			Job job = client.PostedJobs.SingleOrDefault(j => j.ID == jobId);
			if (job != null)
			{
				client.PostedJobs.Remove(job);
				this._entities.Delete(job);
				this._entities.Update(client);
				this._entities.Save();
			}
		}

		public Booking CreateNewBooking(int clientID, int careWorkerID, Schedule schedule, string message, int? jobID)
		{
			if (clientID < 1)
				throw new ArgumentOutOfRangeException("Value must be an int greater than 1", "clientID");

			if (careWorkerID < 1)
				throw new ArgumentOutOfRangeException("Value must be an int greater than 1", "careWorkerID");

			var client = this._entities.Single<Client>(c => c.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("client");

			var carer = this._entities.Single<CareWorker>(cw => cw.ID == careWorkerID);
			if (carer == null)
				throw new ArgumentNullException("carer");

			var booking = new Booking(client.ID, carer.ID, schedule, message, (int)jobID);

			// upddate job status
			if (jobID != null && jobID != 0)
			{
				var job = client.GetJob((int)jobID);
				job.Pending();
			}

			// update case note
			var note = $"{client.FirstName} has created a request for help.";
			booking.AddCaseNote(client.ID, note);

			client.CreateNewBooking(booking);

			this._entities.Update(client);
			this._entities.Save();

			// send alert email to careworker
			this._notificationService.SendBookingRequestedEmail(booking, note);

			return booking;
		}

		public void CancelBooking(int clientID, int bookingID)
		{
			if (clientID < 1)
				throw new ArgumentOutOfRangeException("Value must be an int greater than 1", "clientID");

			if (bookingID < 1)
				throw new ArgumentOutOfRangeException("Value must be an int greater than 1", "bookingID");

			var client = this._entities.Single<Client>(c => c.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("client");

			// get the booking to cancel or die
			var booking = client.GetBooking(bookingID);
			if (booking == null)
				throw new ArgumentNullException("booking");

			// cancel booking
			if (booking.Cancel())
			{
				// update case note
				var note = $"{client.FirstName} has canceled his help request.";
				booking.AddCaseNote(client.ID, note);

				this._entities.Update(client);
				this._entities.Save();

				// send alert email to client
				this._notificationService.SendBookingCancelledEmail(booking, note);
			}
		}

		public void UpdateBooking(int clientID, UpdateBookingViewModel bookingDetails)
		{
			var client = this._entities.Single<Client>(u => u.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("bookingOwnerID not found as user");

			Booking booking = client.GetBooking(bookingDetails.BookingID);
			if (booking == null)
				throw new ArgumentNullException("booking");

			// update message
			booking.Message = bookingDetails.Message;

			// update schedule
			booking.UpdateSchedule(new Schedule(bookingDetails.Start, bookingDetails.End));

			// update case note
			string note = "";
			if (bookingDetails.Note == null)
				note = $"{client.FirstName} has updated his booking content: From {bookingDetails.Start} to {bookingDetails.End}, {booking.Message}";
			else
			{
				note = $"{client.FirstName} has added a note: {bookingDetails.Note}";
				booking.AddCaseNote(clientID, note);
			}

			// save changes to database
			this._entities.Update(client);
			this._entities.Save();

			// send alert email to the other booking party to let him/her know the booking has changed
			this._notificationService.SendBookingUpdatedByClientEmail(booking, note);
		}

		public void CompleteBooking(int clientID, int bookingID)
		{
			if (clientID < 1)
				throw new ArgumentOutOfRangeException("Value must be an int greater than 1", "clientID");

			if (bookingID < 1)
				throw new ArgumentOutOfRangeException("Value must be an int greater than 1", "bookingID");

			var client = this._entities.Single<Client>(c => c.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("client");

			// get the booking to cancel or do nothing
			var booking = client.GetBooking(bookingID);
			if (booking == null)
				throw new ArgumentNullException("booking");

			// complete booking
			if (booking.Complete())
			{
				// update case note
				var note = $"{client.FirstName} has marked his booking as completed.";
				booking.AddCaseNote(client.ID, note);

				this._entities.Update(client);
				this._entities.Save();

				// send alert email to client
				this._notificationService.SendBookingCompletedEmail(booking, note);
			}
		}

		public UpdateBookingViewModel GetBookingViewModel(int bookingID, int clientID)
		{
			var client = this._entities.Single<Client>(c => c.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("client");

			var booking = client.GetBooking(bookingID);
			if (booking == null)
				throw new ArgumentNullException("booking");

			var careWorkerID = booking.CareWorkerID;
			var careWorker = this._entities.Single<CareWorker>(c => c.ID == careWorkerID);
			if (careWorker == null)
				throw new ArgumentNullException("careWorker");

			var vm = _mapper.Map<Booking, UpdateBookingViewModel>(booking);
			vm.CaseNotes = vm.CaseNotes.AsEnumerable().Reverse().ToList();
			vm.CareWorkerFirstName = careWorker.FirstName;
			vm.OwnerUserID = clientID;

			if (!string.IsNullOrWhiteSpace(client.Address.FullAddress))
			{
				var clientGeoPoint = new GeoCoordinate((double)client.Address.Latitude, (double)client.Address.Longitude);
				var carerGeoPoint = new GeoCoordinate((double)careWorker.Address.Latitude, (double)careWorker.Address.Longitude);
				var distanceDouble = clientGeoPoint.GetDistanceTo(carerGeoPoint) / 1000;
				var distance = distanceDouble.ToString("F1", CultureInfo.InvariantCulture) + " km";

				vm.Distance = distance;
			}

			return vm;
		}

		public IEnumerable<UpdateBookingViewModel> GetListOfBookingsViewModel(int clientID)
		{
			var client = this._entities.Single<Client>(a => a.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("Client");

			return this._bookingData.GetBookingVMListByClient(client.ID);
		}

		public IEnumerable<NewBookingShortlistViewModel> GetShortlist(int ownerUserID)
		{
			return this._bookingData.GetShortlist(ownerUserID);
		}

		public List<JobViewModel> GetJobsListVM(int clientID)
		{
			var client = this._entities.Single<Client>(c => c.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("client");

			var jobs = client.PostedJobs.OrderByDescending(a => a.UpdatedAt).ToList();
			var jobsVM = new List<JobViewModel>();
			jobs.ToList().ForEach(j =>
			{
				JobViewModel model = new JobViewModel();
				var vm = _mapper.Map<Job, JobViewModel>(j);
				if (j.CareWorkerId != null)
					vm.CareWorkerFirstName = this._entities.Single<CareWorker>(a => a.ID == j.CareWorkerId).FirstName;
				jobsVM.Add(vm);
			});

			return jobsVM;
		}

		#endregion

		#region Rating

		public void AddRating(int clientID, RatingViewModel ratingData)
		{
			var client = this._entities.Single<Client>(a => a.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("Client");
			var booking = client.GetBooking(ratingData.BookingID);

			Rating rating = _mapper.Map<RatingViewModel, Rating>(ratingData);
			rating.Status = RatingStatus.New;
			booking.AddRating(rating);

			this._entities.Update(client);
			this._entities.Save();

			// send alert email to client
			this._notificationService.SendBookingRatedEmail(booking, $"{client.FirstName} has rated for your a booking.");
		}

		public void UpdateRating(int clientID, double oldRating, RatingViewModel ratingData)
		{
			var client = this._entities.Single<Client>(a => a.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("Client");
			var booking = client.GetBooking(ratingData.BookingID);
			Rating rating = booking.GetRating();
			rating = _mapper.Map<RatingViewModel, Rating>(ratingData, rating);
			rating.Status = RatingStatus.Update;

			booking.UpdateRating(rating);
			this._entities.Update(client);
			this._entities.Save();

			// send alert email to client
			this._notificationService.SendBookingRatingUpdatedEmail(booking, $"{client.FirstName} has updated a rating.");
		}


		public RatingViewModel GetRatingVM(int clientID, int bookingID)
		{
			var client = this._entities.Single<Client>(a => a.ID == clientID);
			if (client == null)
				throw new ArgumentNullException("Client");
			var booking = client.GetBooking(bookingID);
			var rating = booking.GetRating();
			var vm = new RatingViewModel();
			vm = _mapper.Map<Rating, RatingViewModel>(rating);
			return vm;
		}


		#endregion

		#region Jobs

		public IEnumerable<SelectListItem> GetCarerInShortlist(int careWorkerID)
		{
			var carer = this._entities.Get<CareWorker>(a => a.ID == careWorkerID).SingleOrDefault();
			var selectItem = new SelectListItem { Value = carer.ID.ToString(), Text = carer.FirstName };
			var dropdown = new List<SelectListItem>();
			dropdown.Add(selectItem);
			return dropdown;
		}

		public void RejectCareWorker(int clientID, int careWorkerID, int jobID)
		{
			// upddate job status
			var client = this._entities.Single<Client>(c => c.ID == clientID);
			var job = client.GetJob(jobID);
			if (job.Reject())
			{
				this._entities.Update(job);
				this._entities.Save();
			}

			// send alert email to care worker
			//string subject = "Re: A client has rejected your job requestment !";
			//this._notificationService.SendJobRejectedEmail(careWorkerID, subject);
		}

		#endregion
	}
}