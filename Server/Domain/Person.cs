﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class Person
    {
        public int PersonId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string InvitedBy { get; set; } 
        [JsonIgnore]
        public string HashedPassword { get; set; }
        [JsonIgnore]
        public List<ChildPerson> ChildPersons { get; set; } 
        [JsonIgnore]
        public List<ChatPerson> ChatPersons { get; set; } 
        [JsonIgnore]
        public List<Message> Message { get; set; }

    }
}
