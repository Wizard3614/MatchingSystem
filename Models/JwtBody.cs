﻿namespace MatchingSystem.Models
{
    public class JwtBody
    {
        public string UserId { get; set; } 
        public string Code { get; set; }    

        public string IsOnline { get; set; }

        public DateTime Expiration { get; set; }

    }

}
