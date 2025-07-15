using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Entities;
public interface IEntity<TId>
{
    TId Id { get; set; }

}
