using MyAbilityFirst.Domain;

namespace MyAbilityFirst.Services.ClientFunctions
{
	public interface INotificationService
	{
		void SendBookingRequestedEmail(Booking booking, string subject);
		void SendBookingCancelledEmail(Booking booking, string subject);
		void SendBookingAcceptedEmail(Booking booking, string subject);
		void SendBookingRejectedEmail(Booking booking, string subject);
		void SendBookingUpdatedByClientEmail(Booking booking, string subject);
		void SendBookingUpdatedByCareWorkerEmail(Booking booking, string subject);
		void SendBookingCompletedEmail(Booking booking, string subject);
		void SendBookingRatedEmail(Booking booking, string subject);
		void SendBookingRatingUpdatedEmail(Booking booking, string subject);
		void SendEmailToCoordinator(int coordinatorID, string subject);
		void SendEmailToAllCoordinators(string subject);
	}
}