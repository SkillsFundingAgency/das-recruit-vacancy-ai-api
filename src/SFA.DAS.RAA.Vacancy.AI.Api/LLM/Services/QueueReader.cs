using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services
{

	public interface IQueueReader
	{
		public async Task ReadMessages()
		{
		}
	}

	public class QueueReader(IOptions<VacancyAiConfiguration> configuration) : IQueueReader
	{

		public async Task ReadMessages()
		{
			var config = configuration.Value;

			Console.WriteLine("QUEUE CONNECTION STRING");
		

			var client = new QueueClient(config.QueueKey, config.QueueName);
			QueueMessage[] messages = await client.ReceiveMessagesAsync(maxMessages: 10);

			Console.WriteLine("MESSAGES FROM CONN");
			// Process and delete messages from the queue
			foreach (QueueMessage message in messages)
			{
				// "Process" the message
				string decodedMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                Console.WriteLine($"Message: {decodedMessage}");
				
			}
		}
	}
}


