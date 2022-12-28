using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerGrabber
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Period> Periods { get; set; }
    }
}
