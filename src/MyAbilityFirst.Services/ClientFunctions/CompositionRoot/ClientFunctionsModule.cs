using Autofac;

namespace MyAbilityFirst.Services.ClientFunctions
{
	public class ClientFunctionsModule : Autofac.Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			// register ClientService
			builder
					.RegisterType<ClientService>()
					.As<IClientService>();

			// register NotificationService
			builder
					.RegisterType<NotificationService>()
					.AsImplementedInterfaces();

		}
	}
}