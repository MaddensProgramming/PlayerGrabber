using System;

namespace PlayerGrabber
{
    public class Game
    {
        public Player Opponent { get; set; }

        public ResultEnum Result { get; set; }

        public ColorEnum Color { get; set; }


        public decimal Gains { get; set; }
        public int RatingOpponent { get; set; }

        public DateTime Date { get; set; }


    }
}