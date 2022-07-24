using Autofac;
using AutoMapper;
using System.Collections.Generic;

namespace MyAbilityFirst.Services.Common
{
	public class CommonModule : Autofac.Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			// register presentation layer service
			builder
				.RegisterType<PresentationService>()
				.As<IPresentationService>();

			// register upload service
			builder
				.RegisterType<UploadService>()
				.As<IUploadService>();

			// register upload service
			builder
				.RegisterType<QueueService>()
				.As<IQueueService>();
		}
	}
}