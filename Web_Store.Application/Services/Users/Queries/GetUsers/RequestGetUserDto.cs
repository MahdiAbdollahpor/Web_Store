﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_Store.Application.Services.Users.Queries.GetUsers
{
    public class RequestGetUserDto
    {
        public string SearchKey { get; set; }
        public int Page { get; set; }
    }
}
