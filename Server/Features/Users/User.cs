﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Features.Users
{
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string InvitedBy  { get; set; }
        public string Token { get; set; }

    }
}
