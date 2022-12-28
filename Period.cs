using System.Collections.Generic;

namespace PlayerGrabber
{
    public class Period
    {
        public int Year { get; set; }
        public string Month { get; set; }

        public List<Game> Games { get; set; }

    }
}