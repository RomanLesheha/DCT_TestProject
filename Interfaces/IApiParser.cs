using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_TestProject.Interfaces
{
    interface IApiParser
    {
        Task<T> ParseAsync<T>(string endpoint);
    }
}
