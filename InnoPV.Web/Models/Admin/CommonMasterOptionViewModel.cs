using InnoPV.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InnoPV.Web.Models.Admin;

public class CommonMasterOptionViewModel
{
    public long Id { get; set; }

    public CommonMasterType MasterType { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(250)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Code { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}