using Common.Models.Links;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IVideoConfrence
    {
        Task<ExternalVideoConfrenceResult> CreateVideoConference();
    }
}
