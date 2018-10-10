using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer.Models
{
    public class UsersModel
    {
        private escuelaEntities dbs = new escuelaEntities();
        public List<users> GetUsers()
        {
            var users = dbs.users.ToList();
            return users;
        }

        public void AddUser(users us)
        {
            dbs.users.Add(us);
            dbs.SaveChanges();
        }
    }
}
