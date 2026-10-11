using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Entities;

public class BaseEntity<TKey> : IEntity<TKey>
{
    public TKey Id { get; set; }
}
