using MyAbilityFirst.Domain;
using System.Collections.Generic;


namespace MyAbilityFirst.Services.Common
{
	public interface IUserService
	{
		// User Services
		void CreateClient(string loginIdentityID, string email);
		void CreateCareWorker(string loginIdentityID, string email);
		void CreateCoordinator(string loginIdentityID, string email);
		User FindUser(int? id);
		List<User> GetAllUser();
		User FindUserByLoginID(string loginID);

		// Shortlist Services
		Shortlist CreateShortlist(int ownerUserID, Shortlist shortlistData);
		Shortlist RetrieveShortlistBySelectedUserID(int ownerUserID, int selectedUserID);
		List<Shortlist> RetrieveAllShortlists(int ownerUserID);
		Shortlist UpdateShortlist(int ownerUserID, Shortlist shortlistData);

		// Category Services
		UserSubcategory CreateUserSubcategory(int ownerUserID, UserSubcategory userSubcategoryData);
		UserSubcategory RetrieveUserSubcategoryBySubcategoryID(int ownerUserID, int subcategoryID);
		List<UserSubcategory> RetrieveAllUserSubcategories(int ownerUserID);
		UserSubcategory UpdateUserSubcategory(int ownerUserID, UserSubcategory userSubcategoryData);
		List<UserSubcategory> ReplaceAllUserSubCategories(int ownerUserID, int[] postedSubCategoryIDs, List<UserSubcategory> customValueList);
	}
}