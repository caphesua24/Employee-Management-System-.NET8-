
using BaseLibrary.Entities;
using BaseLibrary.Responese;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;

namespace ServerLibrary.Repositories.Implementations
{
    public class GeneralDepartmentRepository(AppDbContext appDbContext) : IGenericRepositoryInterface<GeneralDepartment>
    {
        public async Task<GeneralResponse> DeleteById(int id)
        {
            var dep = await appDbContext.GeneralDepartments.FindAsync(id);
            if (dep is null) return NotFound();
            appDbContext.GeneralDepartments.Remove(dep);
            await Commit();
            return Success();
        }

        public async Task<List<GeneralDepartment>> GetAll() => await appDbContext.GeneralDepartments.ToListAsync();

        public async Task<GeneralDepartment> GetById(int id)
        {
            var result = await appDbContext.GeneralDepartments.FindAsync(id);
            return result!;
        }

        public async Task<GeneralResponse> Insert(GeneralDepartment item)
        {
            var checkIfNull = await CheckName(item.Name);
            if (!checkIfNull) return new GeneralResponse(false, "General Department added already");
            appDbContext.GeneralDepartments.Add(item);
            await Commit();
            return Success();
        } 

        public async Task<GeneralResponse> Update(GeneralDepartment item)
        {
            var dep = await appDbContext.GeneralDepartments.FindAsync(item.Id);
            if (dep is null) return NotFound();
            dep.Name = item.Name;
            await Commit();
            return Success();
        }

        private static GeneralResponse NotFound() => new(false, "Sorry general department not found");
        private static GeneralResponse Success() => new(true, "Process completed");
        private async Task Commit() => await appDbContext.SaveChangesAsync();
        //This method is used to ensure that no department has the same name before adding a new department to the database.
        private async Task<bool> CheckName(string name)
        {
            var item = await appDbContext.GeneralDepartments.FirstOrDefaultAsync(x => x.Name!.ToLower().Equals(name.ToLower()));
            //If no department is found that matches name, item will be null, and the method will return true.If one is found, the method will return false.
            return item is null ? true:false;
        }
    }
}
