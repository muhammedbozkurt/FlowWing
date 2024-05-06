using FlowWing.DataAccess.Abstract;
using FlowWing.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowWing.DataAccess.Concrete
{
    public class RoleRepository : IRoleRepository
    {
        private readonly FlowWingDbContext _dbContext;
        public RoleRepository(FlowWingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Role role)
        {
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var role = await GetByIdAsync(id);
            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _dbContext.Roles.ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _dbContext.Roles.FindAsync(id);
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task UpdateAsync(Role role)
        {
            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Role> GetRoleByUserEmail(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                if (user.RoleId == 1)
                {
                    return await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                }
                else if (user.RoleId == 2)
                {
                    return await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
