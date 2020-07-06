using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDBCoreSqlNetCore
{
    public class ConsoleApplication
    {
        private readonly IServiceProvider _provider;

        public ConsoleApplication(IServiceProvider provider)
        {
            _provider = provider;
        }
        public async Task Run()
        {
            var contactRepository = _provider.GetService<IContactRepository>();

            var contact = new Contact { ContactName = "contact-Name", ContactType = "Contact-Type", Email = "Test@Test.com", Phone = "0123456789" };

            await contactRepository.CreateAsync(contact);

        }
    }
}