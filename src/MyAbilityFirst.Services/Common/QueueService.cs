using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MyAbilityFirst.Infrastructure;
using System;
using System.Collections.Generic;

namespace MyAbilityFirst.Services.Common
{
	public class QueueService : IQueueService
	{
		#region Fields

		private readonly IWriteEntities _entities;
		private readonly CloudQueue _queue;

		#endregion

		#region Ctor

		public QueueService(IWriteEntities entities)
		{
			this._entities = entities;
		}

		public QueueService(IWriteEntities entities, CloudQueue queue)
		{
			this._entities = entities;
			this._queue = queue;
		}

		#endregion

		#region service

		public CloudQueueMessage GetQueueMessage(string queueName)
		{
			CloudQueue queue = GetQueue(queueName);
			CloudQueueMessage retrievedMessage = queue.GetMessage();
			if (retrievedMessage == null) return null;
			return retrievedMessage;
		}

		public void AddMessageInQueue(string queueName, string message)
		{
			CloudQueue queue = GetQueue(queueName);
			CloudQueueMessage queueMessage = new CloudQueueMessage(message);
			queue.AddMessage(queueMessage);
			//queue.AddMessage(queueMessage, TimeSpan.FromSeconds(30));
		}

		public string PeekMessageFromQueue(string queueName)
		{
			CloudQueue queue = GetQueue(queueName);
			CloudQueueMessage peekedMessage = queue.PeekMessage();
			return peekedMessage.AsString;
		}

		public void UpdateMessageInQueue(string queueName, string newMessage)
		{
			CloudQueue queue = GetQueue(queueName);
			CloudQueueMessage message = queue.GetMessage();
			message.SetMessageContent(newMessage);
			// Make it invisible for another 60 seconds.
			queue.UpdateMessage(message, TimeSpan.FromSeconds(60.0), MessageUpdateFields.Content | MessageUpdateFields.Visibility);
		}

		public void DeleteMessageInQueueWithID(string queueName, string messageID, string messagePopReceipt)
		{
			CloudQueue queue = GetQueue(queueName);
			queue.DeleteMessage(messageID, messagePopReceipt);
		}

		public void DeleteMessageInQueue(string queueName, CloudQueueMessage queueMessage)
		{
			CloudQueue queue = GetQueue(queueName);
			//Process the message in less than 30(default) seconds, and then delete the message
			if (queueMessage != null)
				queue.DeleteMessage(queueMessage);
		}

		public int GetQueueMessageCount(string queueName)
		{
			CloudQueue queue = GetQueue(queueName);
			queue.FetchAttributes();
			return (int)queue.ApproximateMessageCount;
		}

		public List<string> ProcessMessages(string queueName, int messageCount, int holdTime)
		{
			List<string> messageList = new List<string>();
			CloudQueue queue = GetQueue(queueName);
			foreach (CloudQueueMessage message in queue.GetMessages(messageCount, TimeSpan.FromMinutes(holdTime)))
			{
				// Process all messages in less than 5 minutes, 
				messageList.Add(message.AsString);

				// Deleting each message after processing.
				queue.DeleteMessage(message);
			}
			return messageList;
		}

		public List<CloudQueueMessage> GetMessagesFromQueue(string queueName, int numberOfMessages, int holdTime)
		{
			List<CloudQueueMessage> messageList = new List<CloudQueueMessage>();
			CloudQueue queue = GetQueue(queueName);
			foreach (CloudQueueMessage message in queue.GetMessages(numberOfMessages, TimeSpan.FromMinutes(holdTime)))
			{
				// Process 'numberOfMessages' messages in less than 'holdTime' minutes
				messageList.Add(message);
			}
			return messageList;
		}

		#endregion

		#region Helper

		private CloudQueue GetQueue(string queueName)
		{
			string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
			////string connectionString = "UseDevelopmentStorage=true";   //this for testing only;

			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
			CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
			CloudQueue queue = queueClient.GetQueueReference(queueName);
			queue.CreateIfNotExists();
			return queue;
		}

		#endregion
	}
}
