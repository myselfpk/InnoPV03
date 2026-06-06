using InnoPV.Domain.Common;
using InnoPV.Domain.Enums;

namespace InnoPV.Domain.Entities;

public class CommonMasterOption : BaseEntity
{
    public CommonMasterType MasterType { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 1;
}