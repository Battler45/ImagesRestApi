﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesRestApi.Repositories
{
    interface IUnitOfWork
    {
        public int SaveChanges();
        public Task<int> SaveChangesAsync();
    }
}
