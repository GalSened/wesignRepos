using Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface IPrograms
    {
        IEnumerable<Program> Read(string key,  int offset, int limit, out int totalCount);
        Task<Program> Read(Program program);
        Task Update(Program program);
        Task Delete(Program program);
        Task Create(Program program);
    }
}
