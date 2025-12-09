using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services
{

	public interface IQueueHandlerSimple
	{
		public async Task AddMessageAsync(string msg)
		{
		}
	}

	public class QueueHandlerSimple(IOptions<VacancyAiConfiguration> configuration) : IQueueHandlerSimple
	{
		private string Queue_connection_string { get; set; } = "t";
		private string Queue_name { get; set; } = "t";		
		

		public async Task AddMessageAsync(string msg)
		{
			var config = configuration.Value;

			Console.WriteLine("QUEUE CONNECTION STRING");
			Console.WriteLine(config.QueueKey);
			Console.WriteLine(config.QueueName);
			var client = new QueueClient(config.QueueKey, config.QueueName);

			await client.CreateIfNotExistsAsync();
			string cloudMessage = msg;
			await client.SendMessageAsync(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cloudMessage)));
		}
	}
}


