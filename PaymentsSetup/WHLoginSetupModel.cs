using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentsSetup
{
    public class WHLoginSetupModel
    {
        public string ServerName { get; set; }
        public string PMCId { get; set; }
        public string SiteId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Unit { get; set; }
        public string NewPassword { get; set; }
        public string Environment { get; set; }
        public string UserFullName { get; set; }
    }
}
