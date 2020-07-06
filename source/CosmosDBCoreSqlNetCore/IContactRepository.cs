using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDBCoreSqlNetCore
{
    public interface IContactRepository
    {
        Task<List<Contact>> GetAllContactsAsync();
        Task<List<Contact>> FindContactByPhoneAsync(string phone);
        Task<List<Contact>> FindContactsByContactNameAsync(string contactName);
        Task<Contact> FindContactAsync(string id);
        Task<List<Contact>> FindContactCPAsync(string contactName, string phone);
        Task<Contact> CreateAsync(Contact contact);
        Task<Contact> UpdateAsync(Contact contact);
        Task DeleteAsync(string id, string contactName, string phone);

    }
}