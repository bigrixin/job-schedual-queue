using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAbilityFirst.Domain
{
	public abstract class User
	{

		#region Properties

		public int ID { get; set; }

		public UserStatus Status { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int GenderID { get; set; }
		public DateTime? DoB { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public Address Address { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public string LoginIdentityId { get; private set; }

		[System.ComponentModel.DataAnnotations.Schema.ForeignKey("OwnerUserID")]
		public virtual ICollection<Shortlist> Shortlists { get; set; }

		[System.ComponentModel.DataAnnotations.Schema.ForeignKey("OwnerUserID")]
		public virtual ICollection<UserSubcategory> UserSubCategories { get; set; }


		#endregion

		#region Ctor

		protected User()
		{
			// required by EF
			this.Address = new Address();
		}

		public User(string LoginIdentityId)
		{
			this.LoginIdentityId = LoginIdentityId;
			this.Address = new Address();
		}

		#endregion

		#region Shortlist

		public Shortlist AddNewShortlist(Shortlist shortlistData)
		{
			var shortlists = Shortlists.Where(s => s.ID == shortlistData.ID);
			if (shortlists.Any())
			{
				return shortlists.FirstOrDefault();
			}

			Shortlists.Add(shortlistData);
			return shortlistData;
		}

		public Shortlist UpdateExistingShortlist(Shortlist shortlistData)
		{
			if (shortlistData == null)
				return null;

			if (shortlistData.OwnerUserID != this.ID)
				return null;

			var shortlists = Shortlists.Where(s => s.ID == shortlistData.ID);
			if (shortlists.Any())
			{
				Shortlists.Remove(shortlists.Single(s => s.ID == shortlistData.ID));
				Shortlists.Add(shortlistData);
				return shortlistData;
			}
			return null;
		}

		public Shortlist GetExistingShortlist(int selectedUserID)
		{
			var shortlists = Shortlists.Where(s => s.SelectedUserID == selectedUserID);
			if (shortlists.Any())
			{
				return shortlists.Single(s => s.SelectedUserID == selectedUserID);
			}
			return null;
		}

		public List<Shortlist> GetAllExistingShortlists()
		{
			return this.Shortlists.ToList();
		}

		#endregion

		#region UserSubCategory

		public UserSubcategory AddNewUserSubCategory(UserSubcategory userSubcategoryData)
		{
			var userSubcategories = UserSubCategories.Where(usc => usc.ID == userSubcategoryData.ID);
			if (userSubcategories.Any())
			{
				return userSubcategories.FirstOrDefault();
			}

			this.UserSubCategories.Add(userSubcategoryData);
			return userSubcategoryData;
		}

		public UserSubcategory GetExistingUserSubcategory(int subcategoryID)
		{
			var userSubcategories = UserSubCategories.Where(usc => usc.SubCategoryID == subcategoryID && usc.OwnerUserID == this.ID);
			if (userSubcategories.Any())
			{
				return userSubcategories.Single(usc => usc.SubCategoryID == subcategoryID && usc.OwnerUserID == this.ID);
			}
			return null;
		}

		public List<UserSubcategory> GetAllExistingUserSubcategories()
		{
			return this.UserSubCategories.Where(usc => usc.OwnerUserID == this.ID).ToList();
		}

		
		public UserSubcategory UpdateExistingUserSubcategory(UserSubcategory userSubcategoryData)
		{
			if (userSubcategoryData == null)
				return null;

			if (userSubcategoryData.OwnerUserID != this.ID)
				return null;

			var userSubcategories = UserSubCategories.Where(usc => usc.OwnerUserID == userSubcategoryData.OwnerUserID && usc.SubCategoryID == userSubcategoryData.SubCategoryID);
			if (userSubcategories.Any())
			{
				UserSubCategories.Remove(userSubcategories.Single(usc => usc.OwnerUserID == userSubcategoryData.OwnerUserID && usc.SubCategoryID == userSubcategoryData.SubCategoryID));
				UserSubCategories.Add(userSubcategoryData);
				return userSubcategoryData;
			}
			return null;
		}


		#endregion
	}
}
