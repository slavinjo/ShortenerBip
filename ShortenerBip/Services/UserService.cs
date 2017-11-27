using Microsoft.AspNetCore.Identity;
using ShortenerBip.Helper;
using ShortenerBip.Interfaces;
using ShortenerBip.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenerBip.Services
{
    public class UserService : IUserInterface
    {
        private DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(DataContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public User Authenticate(string accountId, string password)
        {
            if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.AccountId == accountId);

            // check if username exists
            if (user == null)
                return null;



            // check if password is correct /*todo */
            //if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            //    return null;

            // authentication successful
            _signInManager.SignInAsync(user,true);
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public User Create(User user)
        {
            // validation
            //if (string.IsNullOrWhiteSpace(password))
            //    throw new AppException("Password is required");

            if (string.IsNullOrWhiteSpace(user.AccountId))
                throw new AppException("Account ID is required");

            String password = Password.GeneratePassword(8, 0);
            user.Password = password;

            if (_context.Users.Any(x => x.AccountId == user.AccountId))
                throw new AppException("Account ID " + user.AccountId + " is already taken, please choose different Account ID.");

            user.UserName = user.AccountId;
            user.Token = user.Password;
            
            //byte[] passwordHash, passwordSalt; /*todo*/
            //CreatePasswordHash(password, out passwordHash, out passwordSalt);

            //user.PasswordHash = passwordHash;
            //user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Update(User userParam, string password = null)
        {
            var user = _context.Users.Find(userParam.AccountId);

            if (user == null)
                throw new AppException("User not found");

            if (userParam.AccountId != user.AccountId)
            {
                // username has changed so check if the new username is already taken
                if (_context.Users.Any(x => x.AccountId == userParam.AccountId))
                    throw new AppException("AccountID " + userParam.AccountId + " is already taken");
            }

            // update user properties
            //user.FirstName = userParam.FirstName;
            //user.LastName = userParam.LastName;
            user.AccountId = userParam.AccountId;
            user.UserName = userParam.UserName;
            user.Password = userParam.Password;
            user.Token = userParam.Password;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                //user.PasswordHash = passwordHash;
                //user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}