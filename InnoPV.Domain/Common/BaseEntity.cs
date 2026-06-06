using System;
using System.Collections.Generic;
using System.Text;

namespace InnoPV.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public string? CreatedBy { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
}
