﻿using BaseLibrary.Entities;
using BaseLibrary.Responese;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.Repositories.Implementations
{
    public class TownRepository(AppDbContext appDbContext) : IGenericRepositoryInterface<Town>
    {
        public async Task<GeneralResponse> DeleteById(int id)
        {
            var town = await appDbContext.Towns.FindAsync(id);
            if (town is null) return NotFound();
            appDbContext.Towns.Remove(town);
            await Commit();
            return Success();
        }

        public async Task<List<Town>> GetAll() => await appDbContext
            .Towns
            .AsNoTracking()
            .Include(t=>t.City)
            .ToListAsync();

        public async Task<Town> GetById(int id) 
        {
            var result = await appDbContext.Towns.FindAsync(id);
            return result!;
        }

        public async Task<GeneralResponse> Insert(Town item)
        {
            if (!await CheckName(item.Name!)) return new GeneralResponse(false, "Town already added");
            appDbContext.Towns.Add(item);
            await Commit();
            return Success();
        }

        public async Task<GeneralResponse> Update(Town item)
        {
            var town = await appDbContext.Towns.FindAsync(item.Id);
            if (town is null) return NotFound();
            town.Name = item.Name;
            town.CityID = item.CityID;
            await Commit();
            return Success();
        }

        private static GeneralResponse NotFound() => new(false, "Sorry department not found");
        private static GeneralResponse Success() => new(true, "Process completed");
        private async Task Commit() => await appDbContext.SaveChangesAsync();
        //This method is used to ensure that no department has the same name before adding a new department to the database.
        private async Task<bool> CheckName(string name)
        {
            var item = await appDbContext.Departments.FirstOrDefaultAsync(x => x.Name!.ToLower().Equals(name.ToLower()));
            //If no department is found that matches name, item will be null, and the method will return true.If one is found, the method will return false.
            return item is null;
        }
    }
}
