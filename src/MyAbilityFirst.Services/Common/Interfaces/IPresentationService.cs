using MyAbilityFirst.Domain;
using MyAbilityFirst.Domain.AttachmentManagement;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MyAbilityFirst.Services.Common
{
	public interface IPresentationService
	{
		List<Availability> GetPrefilledAvailabilityList(int careWorkerID, ICollection<Availability> selectedAvailabilities);
		string GetSubCategoryName(int subcategoryID);
		List<DayOfWeek> GetDayOfWeekList();
		List<TimeOfDay> GetTimeOfDayList();
		SelectList GetSubCategorySelectList(string categoryName);
		List<Subcategory> GetSubCategoryList(string categoryName);
		List<Subcategory> GetSubCategoryListByUser(string categoryName, int userID);
		SelectList GetPatientSelectList(int id);
		List<Subcategory> GetCrossReferenceSubcategoryList(int[] selectedSubcategoryIDs, List<Subcategory> referenceList);
		Address GetUserAddress(int userID);
		List<AttachmentType> GetAttachmentList();
		SelectList GetOrganisationList();
		string GetOrganisationLogoURL(int organistionID);
		Organisation GetOrganisation(int organistionID);
	}
}
