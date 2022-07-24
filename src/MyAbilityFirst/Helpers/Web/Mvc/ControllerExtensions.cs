using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MyAbilityFirst.Domain;
using MyAbilityFirst.Infrastructure;
using MyAbilityFirst.Infrastructure.Auth;
using System.Web.Security;
using System.Security.Claims;

public static class ControllerExtensions
{

	#region Extensions

	public static User GetLoggedInUser(this Controller c)
	{
		if (!c.HttpContext.Request.IsAuthenticated)
			return null;

		var entities = DependencyResolver.Current.GetService<IReadEntities>();
		var loginIdentityId = c.HttpContext.User.Identity.GetUserId();
		User user = null;

		if (c.HttpContext.User.IsInRole("Client"))
			user = entities.Single<Client>(u => u.LoginIdentityId == loginIdentityId);
		else if (c.HttpContext.User.IsInRole("CareWorker"))
			user = entities.Single<CareWorker>(u => u.LoginIdentityId == loginIdentityId);
		else if (c.HttpContext.User.IsInRole("Coordinator"))
			user = entities.Single<Coordinator>(u => u.LoginIdentityId == loginIdentityId);
		return user;
	}

	#endregion

}
