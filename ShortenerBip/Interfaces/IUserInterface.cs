using ShortenerBip.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Interfaces
{
    public interface IUserInterface
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user);
        void Update(User user, string password = null);
        void Delete(int id);
    }
}
