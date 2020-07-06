using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmosDBCoreSqlNetCore
{
    public class CosmosContactRepository : IContactRepository
    {
        private readonly ILogger<CosmosContactRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly Container _container;

        public CosmosContactRepository(IOptions<CosmosUtility> cosmosUtility, ILogger<CosmosContactRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var cosmosEndpoint = cosmosUtility.Value.CosmosEndpoint;
            var cosmosKey = cosmosUtility.Value.CosmosKey;
            var databaseId = "multiDb";
            var containerId = "contacts";

            var cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
            cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).GetAwaiter().GetResult();
            var database = cosmosClient.GetDatabase(databaseId);
            database.CreateContainerIfNotExistsAsync(containerId, "/contactName").GetAwaiter().GetResult();
            _container = database.GetContainer(containerId);

            database = cosmosClient.GetDatabase(databaseId);
            _container = database.GetContainer(containerId);
        }

        private async Task<List<Contact>> GetContacts(string sqlQuery)
        {
            _logger.LogDebug("Begin : GetContacts()");
            QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
            FeedIterator<Contact> queryResultIterator = _container.GetItemQueryIterator<Contact>(queryDefinition);
            List<Contact> contactsList = new List<Contact>();

            while (queryResultIterator.HasMoreResults)
            {
                FeedResponse<Contact> currentResultSet = await queryResultIterator.ReadNextAsync();
                foreach (var item in currentResultSet)
                {
                    contactsList.Add(item);
                }
                return contactsList;
            }
            return null;
        }

        public async Task<Contact> CreateAsync(Contact contact)
        {
            _logger.LogDebug("Begin : CreateAsync()");
            contact.Id = Guid.NewGuid().ToString();
            Console.WriteLine("CreateAsync");
            var contactResponse =
                await _container.CreateItemAsync<Contact>(contact, new PartitionKey(contact.ContactName));
            if (contactResponse != null)
            {
                return contact;
            }
            return null;
        }

        public async Task DeleteAsync(string id, string contactName, string phone)
        {
            _logger.LogDebug("Begin : DeleteAsync()");

            var contactResponse = await _container.DeleteItemAsync<Contact>(id, new PartitionKey(contactName));
        }
        public async Task<Contact> FindContactAsync(string id)
        {
            _logger.LogDebug("Begin : FindContactAsync()");

            var sqlQuery = $"select * from c where c.id='{id}'";
            var contactsList = await GetContacts(sqlQuery);
            return contactsList[0];
        }

        public async Task<List<Contact>> FindContactByPhoneAsync(string phone)
        {
            _logger.LogDebug("Begin : FindContactByPhoneAsync()");

            var sqlQuery = $"select * from c where c.phone='{phone}'";
            var contactsList = await GetContacts(sqlQuery);
            return contactsList;
        }

        public async Task<List<Contact>> FindContactCPAsync(string contactName, string phone)
        {
            _logger.LogDebug("Begin : FindContactCPAsync()");

            var sqlQuery = $"select * from c where c.contactName='{contactName}' c.phone='{phone}'";
            var contactsList = await GetContacts(sqlQuery);
            return contactsList;
        }

        public async Task<List<Contact>> FindContactsByContactNameAsync(string contactName)
        {
            _logger.LogDebug("Begin : FindContactsByContactNameAsync()");

            var sqlQuery = $"select * from c where c.contactName='{contactName}'";
            var contactsList = await GetContacts(sqlQuery);
            return contactsList;
        }

        public async Task<List<Contact>> GetAllContactsAsync()
        {
            _logger.LogDebug("Begin : GetAllContactsAsync()");

            var sqlQuery = $"select * from c";
            var contactsList = await GetContacts(sqlQuery);
            return contactsList;
        }

        public async Task<Contact> UpdateAsync(Contact contact)
        {
            _logger.LogDebug("Begin : UpdateAsync()");

            var contactResponse = await _container.ReadItemAsync<Contact>(contact.Id, new PartitionKey(contact.ContactName));
            var contactResult = contactResponse.Resource;

            contactResult.Id = contact.Id;
            contactResult.ContactName = contact.ContactName;
            contactResult.Phone = contact.Phone;
            contactResult.ContactType = contact.ContactType;
            contactResult.Email = contact.Email;

            contactResponse = await _container.ReplaceItemAsync<Contact>(contactResult, contactResult.Id);

            if (contactResponse.Resource != null)
            {
                return contactResponse;
            }
            return null;
        }
    }
}