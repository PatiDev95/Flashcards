﻿using Flashcards.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Flashcards.Infrastructure.IRepositories
{
    public interface IUserRepository
    {
        Task<User> GetAsync(Guid id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}