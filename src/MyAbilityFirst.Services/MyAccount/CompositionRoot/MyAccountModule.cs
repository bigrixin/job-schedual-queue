using Autofac;
using MyAbilityFirst.Services.Common;

namespace MyAbilityFirst.Domain
{
	public class MyAccountModule : Autofac.Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			// register UserService
			builder
				.RegisterType<UserService>()
				.As<IUserService>();
		}
	}
}