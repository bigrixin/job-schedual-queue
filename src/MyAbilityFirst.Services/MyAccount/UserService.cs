using AutoMapper;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Infrastructure;
using MyAbilityFirst.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAbilityFirst.Services.Common
{
	public class UserService : IUserService
	{

		#region Fields

		private readonly IWriteEntities _entities;
		private readonly IMapper _mapper;

		#endregion

		#region Ctor

		public UserService(IWriteEntities entities, IMapper mapper)
		{
			this._entities = entities;
			this._mapper = mapper;
		}

		#endregion

		#region User Services

		public void CreateClient(string loginIdentityID, string email)
		{
			var now = DateTime.Now;
			var client = new Client(loginIdentityID);
			client.CreatedAt = now;
			client.UpdatedAt = now;
			client.Email = email;
			client.Status = UserStatus.Registered;
			this._entities.Create(client);
			this._entities.Save();
		}

		public void CreateCareWorker(string loginIdentityID, string email)
		{
			var now = DateTime.Now;
			var worker = new CareWorker(loginIdentityID);
			worker.CreatedAt = now;
			worker.UpdatedAt = now;
			worker.Email = email;
			worker.Status = UserStatus.Registered;
			this._entities.Create(worker);
			this._entities.Save();
		}

		public void CreateCoordinator(string loginIdentityID, string email)
		{
			var now = DateTime.Now;
			var coordinator = new Coordinator(loginIdentityID);
			coordinator.CreatedAt = now;
			coordinator.UpdatedAt = now;
			coordinator.Email = email;
			coordinator.Status = UserStatus.Registered;
			this._entities.Create(coordinator);
			this._entities.Save();
		}

		public User FindUser(int? id)
		{
			return this._entities.Get<User>(u => u.ID == id).Single();
		}

		public List<User> GetAllUser()
		{
			var users = this._entities.Get<User>().ToList();
			return users;
		}

		public User FindUserByLoginID(string loginID)
		{
			var users = this._entities.Get<User>(u => u.LoginIdentityId == loginID);
			if (users.Any())
			{
				return users.First();
			}
			return null;
		}

		#endregion

		#region Shortlist

		public Shortlist CreateShortlist(int ownerUserID, Shortlist shortlistData)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				user.AddNewShortlist(shortlistData);

				_entities.Update(user);
				_entities.Save();
				return shortlistData;
			}
			return null;
		}

		public Shortlist RetrieveShortlistBySelectedUserID(int ownerUserID, int selectedUserID)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				return user.GetExistingShortlist(selectedUserID);
			}
			return null;
		}

		public List<Shortlist> RetrieveAllShortlists(int ownerUserID)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				return user.GetAllExistingShortlists();
			}
			return null;
		}

		public Shortlist UpdateShortlist(int ownerUserID, Shortlist shortlistData)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				user.UpdateExistingShortlist(shortlistData);

				this._entities.Update(user);
				_entities.Save();
				return shortlistData;
			}
			return null;
		}

		#endregion
		
		#region UserSubcategory

		public UserSubcategory CreateUserSubcategory(int ownerUserID, UserSubcategory userSubcategoryData)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				user.AddNewUserSubCategory(userSubcategoryData);

				_entities.Update(user);
				_entities.Save();
				return userSubcategoryData;
			}
			return null;
		}

		public UserSubcategory RetrieveUserSubcategoryBySubcategoryID(int ownerUserID, int subcategoryID)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				return user.GetExistingUserSubcategory(subcategoryID);
			}
			return null;
		}

		public List<UserSubcategory> RetrieveAllUserSubcategories(int ownerUserID)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				return user.GetAllExistingUserSubcategories();
			}
			return null;
		}

		public UserSubcategory UpdateUserSubcategory(int ownerUserID, UserSubcategory userSubcategoryData)
		{
			User user = FindUser(ownerUserID);
			if (user != null)
			{
				user.UpdateExistingUserSubcategory(userSubcategoryData);

				this._entities.Update(user);
				_entities.Save();
				return userSubcategoryData;
			}
			return null;
		}



		public List<UserSubcategory> ReplaceAllUserSubCategories(int ownerUserID, int[] postedSubCategoryIDs, List<UserSubcategory> customValueList)
		{
			List<UserSubcategory> existingSubcategoryList = RetrieveAllUserSubcategories(ownerUserID);
			int[] previousSubCategoryIDs = existingSubcategoryList.Select(x => x.SubCategoryID).ToArray();
			postedSubCategoryIDs = postedSubCategoryIDs ?? new int[0];

			// add new
			IEnumerable<int> actionableIDs = postedSubCategoryIDs.Except(previousSubCategoryIDs);
			IEnumerable<UserSubcategory> actionableObjects = getNewUSCObjects(ownerUserID, actionableIDs, customValueList);
			foreach (UserSubcategory usc in actionableObjects)
			{
				CreateUserSubcategory(ownerUserID, usc);
			}

			// flag existing as selected
			actionableObjects = getExistingUSCObjects(existingSubcategoryList, postedSubCategoryIDs, customValueList, true);
			foreach (UserSubcategory usc in actionableObjects)
			{
				UpdateUserSubcategory(ownerUserID, usc);
			}

			// flag existing as unselected
			actionableIDs = previousSubCategoryIDs.Except(postedSubCategoryIDs);
			actionableObjects = getExistingUSCObjects(existingSubcategoryList, actionableIDs, customValueList, false);
			foreach (UserSubcategory usc in actionableObjects)
			{
				UpdateUserSubcategory(ownerUserID, usc);
			}
			return RetrieveAllUserSubcategories(ownerUserID);
		}

		#endregion

		#region Helpers

		private IEnumerable<UserSubcategory> getExistingUSCObjects(IEnumerable<UserSubcategory> existingSubcategoryList, IEnumerable<int> selectedSubcategoryIDs, List<UserSubcategory> customValueList, bool isSelected)
		{
			return (
				from usc in existingSubcategoryList
				join scID in selectedSubcategoryIDs on usc.SubCategoryID equals scID
				select usc
			)
			.Select(usc => {
				usc.Selected = isSelected;
				if (customValueList.Exists((cvl => cvl.OwnerUserID == usc.OwnerUserID && cvl.SubCategoryID == usc.SubCategoryID)))
					usc.CustomValue = customValueList.Single(cvl => cvl.OwnerUserID == usc.OwnerUserID && cvl.SubCategoryID == usc.SubCategoryID).CustomValue;
				return usc;
			}).ToList();
		}

		private IEnumerable<UserSubcategory> getNewUSCObjects(int ownerUserID, IEnumerable<int> selectedSubcategoryIDs, List<UserSubcategory> customValueList)
		{
			return selectedSubcategoryIDs.Select(scID => {
				UserSubcategory usc = new UserSubcategory();
				usc.OwnerUserID = ownerUserID;
				usc.Selected = true;
				usc.SubCategoryID = scID;
				if (customValueList.Exists((cvl => cvl.OwnerUserID == usc.OwnerUserID && cvl.SubCategoryID == usc.SubCategoryID)))
					usc.CustomValue = customValueList.Single(cvl => cvl.OwnerUserID == usc.OwnerUserID && cvl.SubCategoryID == usc.SubCategoryID).CustomValue;
				return usc;
			}).ToList();
		}

		#endregion

	}
}