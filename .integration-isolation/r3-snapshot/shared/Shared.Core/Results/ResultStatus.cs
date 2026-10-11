using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Results;

public enum ResultStatus
{
    Ok = 200,
    Error,
    Forbidden,
    Unauthorized = 401,
    Invalid = 400,
    NotFound = 404,
    Conflict,
    CriticalError = 500,
    Unavailable
}
