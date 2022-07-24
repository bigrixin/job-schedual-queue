using AutoMapper;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Domain.ClientFunctions;
using MyAbilityFirst.Models;
using MyAbilityFirst.Services.AttachmentManagement;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MyAbilityFirst.Services.Common
{
	public class ClientMappingProfile : Profile
	{

		#region Fields

		private readonly IPresentationService _presentationService;
		private readonly IAttachmentService _attachmentService;

		public override string ProfileName => "ClientMappingProfile";

		#endregion

		#region Ctor

		public ClientMappingProfile(IPresentationService presentationService, IAttachmentService attachmentService)
		{
			this._presentationService = presentationService;
			this._attachmentService = attachmentService;

			this.MapClient();
			this.MapPatient();
			this.MapContact();
			this.MapBooking();
			this.MapJob();
			this.MapRating();
		}

		#endregion

		#region Helpers

		private void MapClient()
		{
			CreateMap<Client, ClientDetailsViewModel>()
				.ForMember(dest => dest.ClientID, opt => opt.MapFrom(src => src.ID))
				.ForMember(dest => dest.GenderDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Gender")))
					.AfterMap((src, dest) =>
					{
						dest.Gender = dest.GenderDropDownList.SingleOrDefault(list => list.Value == src.GenderID.ToString()) == null ?
						"" : dest.GenderDropDownList.SingleOrDefault(list => list.Value == src.GenderID.ToString()).Text;
					})
				.ForMember(dest => dest.MarketingInfoList, opt => opt.MapFrom(src => this._presentationService.GetSubCategoryList("ClientMarketingInfo")))
				.ForMember(dest => dest.PreviousMarketingInfo, opt => opt.MapFrom(src => this._presentationService.GetSubCategoryListByUser("ClientMarketingInfo", src.ID)));

			CreateMap<ClientDetailsViewModel, Client>()
				.IncludeBase<UserDetailsViewModel, User>()
				.ForMember(dest => dest.Patients, opt => opt.Ignore())
				.ForMember(dest => dest.PostedJobs, opt => opt.Ignore());
		}

		private void MapPatient()
		{
			CreateMap<Patient, PatientDetailsViewModel>()
				.ForMember(dest => dest.PatientID, opt => opt.MapFrom(src => src.ID))
				.ForMember(dest => dest.GenderDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Gender")))
					.AfterMap((src, dest) =>
					{
						dest.Gender = dest.GenderDropDownList.SingleOrDefault(list => list.Value == src.GenderID.ToString()) == null ?
						"" : dest.GenderDropDownList.SingleOrDefault(list => list.Value == src.GenderID.ToString()).Text;
					})
				.ForMember(dest => dest.LanguageDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Language")))
					.AfterMap((src, dest) =>
					{
						dest.FirstLanguage = dest.LanguageDropDownList.SingleOrDefault(list => list.Value == src.FirstLanguageID.ToString()) == null ?
						"" : dest.LanguageDropDownList.SingleOrDefault(list => list.Value == src.FirstLanguageID.ToString()).Text;
					})
					.AfterMap((src, dest) =>
					{
						dest.SecondLanguage = dest.LanguageDropDownList.SingleOrDefault(list => list.Value == src.SecondLanguageID.ToString()) == null ?
						"" : dest.LanguageDropDownList.SingleOrDefault(list => list.Value == src.SecondLanguageID.ToString()).Text;
					})
				.ForMember(dest => dest.CultureDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Culture")))
					.AfterMap((src, dest) =>
					{
						dest.Culture = dest.CultureDropDownList.SingleOrDefault(list => list.Value == src.CultureID.ToString()) == null ?
						"" : dest.CultureDropDownList.SingleOrDefault(list => list.Value == src.CultureID.ToString()).Text;
					})
				.ForMember(dest => dest.ReligionDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Religion")))
					.AfterMap((src, dest) =>
					{
						dest.Religion = dest.ReligionDropDownList.SingleOrDefault(list => list.Value == src.ReligionID.ToString()) == null ?
						"" : dest.ReligionDropDownList.SingleOrDefault(list => list.Value == src.ReligionID.ToString()).Text;
					})
				.ForMember(dest => dest.CareTypeDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("CareType")))
					.AfterMap((src, dest) =>
					{
						dest.CareType = dest.CareTypeDropDownList.SingleOrDefault(list => list.Value == src.CareTypeID.ToString()) == null ?
						"" : dest.CareTypeDropDownList.SingleOrDefault(list => list.Value == src.CareTypeID.ToString()).Text;
					})
				.ForMember(dest => dest.RelationshipDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Relationship")))
				.ForMember(dest => dest.PetDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Pet")))
				.ForMember(dest => dest.InterestList, opt => opt.MapFrom(src => this._presentationService.GetSubCategoryList("Interest")))
				.ForMember(dest => dest.MedicalLivingNeedsList, opt => opt.MapFrom(src => this._presentationService.GetSubCategoryList("MedicalLivingNeed")))
				.ForMember(dest => dest.PreviousInterestList, opt => opt.MapFrom(src => this._presentationService.GetSubCategoryListByUser("Interest", src.ID)))
				.ForMember(dest => dest.PreviousMedicalLivingNeedsList, opt => opt.MapFrom(src => this._presentationService.GetSubCategoryListByUser("MedicalLivingNeed", src.ID)))
				.ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Contacts));

			CreateMap<PatientDetailsViewModel, Patient>()
				.ForMember(dest => dest.Contacts, opt => opt.Ignore());

			CreateMap<Patient, PatientAttachmentViewModel>()
				.ForMember(dest => dest.PatientID, opt => opt.MapFrom(src => src.ID))
				.ForMember(dest => dest.AttachmentList, opt => opt.MapFrom(src => this._presentationService.GetAttachmentList()))
				.ForMember(dest => dest.PreviousAttachmentList, opt => opt.MapFrom(src => this._attachmentService.GetAttachmentsForUser(src.ID)));
		}

		private void MapContact()
		{
			CreateMap<Contact, Contact>();

			CreateMap<List<Contact>, List<Contact>>();
		}

		private void MapBooking()
		{
			CreateMap<Booking, UpdateBookingViewModel>()
				.ForMember(dest => dest.BookingID, opt => opt.MapFrom(src => src.ID))
				.ForMember(dest => dest.Shortlist, opt => opt.ResolveUsing<ShortlistResolver>())
				.ForMember(dest => dest.Start, opt => opt.MapFrom(src => src.Schedule.Start))
				.ForMember(dest => dest.End, opt => opt.MapFrom(src => src.Schedule.End))
				.ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.Status == BookingStatus.Cancelled))
				.ForMember(dest => dest.IsLapsed, opt => opt.MapFrom(src => src.Schedule.End < DateTime.Now));
		}

		private void MapJob()
		{

			CreateMap<Job, JobViewModel>()
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
			.ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
			.ForMember(dest => dest.ServicedAt, opt => opt.MapFrom(src => src.ServiceAt))
			.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
			.ForMember(dest => dest.CareWorkerID, opt => opt.MapFrom(src => src.CareWorkerId))
			.ForMember(dest => dest.GenderDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Gender")))
			.ForMember(dest => dest.ServiceDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("JobService")))
			.ForMember(dest => dest.PatientDropDownList, opt => opt.MapFrom(src => this._presentationService.GetPatientSelectList(src.ClientId)));

			CreateMap<JobViewModel, Job>()
				.ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
				.ForMember(dest => dest.ServiceAt, opt => opt.MapFrom(src => src.ServicedAt));

			CreateMap<Job, UrgentJobViewModel>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
				.ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
				.ForMember(dest => dest.ServicedAt, opt => opt.MapFrom(src => src.ServiceAt))
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
				.ForMember(dest => dest.CareWorkerID, opt => opt.MapFrom(src => src.CareWorkerId))
				.ForMember(dest => dest.GenderDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("Gender")))
				.ForMember(dest => dest.ServiceDropDownList, opt => opt.MapFrom(src => this._presentationService.GetSubCategorySelectList("JobService")))
				.ForMember(dest => dest.PatientDropDownList, opt => opt.MapFrom(src => this._presentationService.GetPatientSelectList(src.ClientId)));

			CreateMap<UrgentJobViewModel, Job>()
				.ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
				.ForMember(dest => dest.ServiceAt, opt => opt.MapFrom(src => src.ServicedAt));
		}

		private void MapRating()
		{
			CreateMap<Rating, RatingViewModel>()
			 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID));

			CreateMap<RatingViewModel, Rating>()
			 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID));
		}

		#endregion

	}
}
