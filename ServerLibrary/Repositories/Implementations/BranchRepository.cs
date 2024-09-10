using BaseLibrary.Entities;
using BaseLibrary.Responese;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class BranchRepository(AppDbContext appDbContext) : IGenericRepositoryInterface<Branch>
    {
        public async Task<GeneralResponse> DeleteById(int id)
        {
            var branch = await appDbContext.Branches.FindAsync(id);
            if(branch is null) return NotFound();

            appDbContext.Branches.Remove(branch);
            await Commit();
            return Success();
        }

        public async Task<List<Branch>> GetAll() => await appDbContext
            .Branches
            .AsNoTracking()
            .Include(d=>d.Department)
            .ToListAsync();

        public async Task<Branch> GetById(int id)
        {
            var result = await appDbContext.Branches.FindAsync(id);
            return result!;
        }

        public async Task<GeneralResponse> Insert(Branch item)
        {
            if (!await CheckName(item.Name!)) return new GeneralResponse(false, "Branch added already");

            appDbContext.Branches.Add(item);
            await Commit();
            return Success();
        }

        public async Task<GeneralResponse> Update(Branch item)
        {
            var branch = await appDbContext.Branches.FindAsync(item.Id);
            if (branch is null) return NotFound();
            branch.Name = item.Name;
            branch.DepartmentId = item.DepartmentId;
            await Commit();
            return Success();
        }

        private static GeneralResponse NotFound() => new(false, "Sorry branch not found");
        private static GeneralResponse Success() => new(true, "Process completed");
        private async Task Commit() => await appDbContext.SaveChangesAsync();

        private async Task<bool> CheckName(string name)
        {
            var branch = await appDbContext.Branches.FirstOrDefaultAsync(x => x.Name.ToLower().Equals(name.ToLower()));
            return branch is null;
        } 
    }
}
