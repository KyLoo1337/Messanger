using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messanger.Models
{
    public class MessageRequest
    {

        public string Subject { get; set; }

        public string MessageText { get; set; }

        public string SenderEmail { get; set; }

        public string RecieverEmail { get; set; }
        
        public DateTime Time { get; set; }

    }
}
