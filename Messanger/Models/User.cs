using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messanger.Models
{
    public class User
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public int Id { get; set; }

        public Dictionary<User, List<Message>> Messages = new Dictionary<User, List<Message>>();  
    }
}
