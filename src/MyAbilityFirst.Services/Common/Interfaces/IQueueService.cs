using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;

namespace MyAbilityFirst.Services.Common
{
	public interface IQueueService
	{
		CloudQueueMessage GetQueueMessage(string queueName);
		void AddMessageInQueue(string queueName, string message);
		string PeekMessageFromQueue(string queueName);
		void UpdateMessageInQueue(string queueName, string newMessage);
		void DeleteMessageInQueueWithID(string queueName, string messageID, string messagePopReceipt);
		void DeleteMessageInQueue(string queueName, CloudQueueMessage queueMessage);
		int GetQueueMessageCount(string queueName);
		List<string> ProcessMessages(string queueName, int messageCount, int holdTime);
		List<CloudQueueMessage> GetMessagesFromQueue(string queueName, int numberOfMessages, int holdTime);
	}
}
