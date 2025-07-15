using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Entities;

public abstract class Auditable<TKey> : BaseEntity<TKey>
{
    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    [Column(TypeName = "timestamp without time zone")]
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
}
