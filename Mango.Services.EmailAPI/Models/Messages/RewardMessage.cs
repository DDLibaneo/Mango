﻿namespace Mango.Services.EmailAPI.Models.Messages
{
    public class RewardMessage
    {
        public string UserId { get; set; }

        public int RewardsActivity { get; set; }

        public int OrderId { get; set; }
    }
}
