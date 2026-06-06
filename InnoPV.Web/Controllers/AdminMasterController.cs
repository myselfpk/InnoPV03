using InnoPV.Domain.Entities;
using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminMasterController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminMasterController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Master Data Home Page
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // ============================================================
    // COMMON MASTER
    // Reporter Type, Outcome, Route, Frequency, Causality etc.
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Common(CommonMasterType type = CommonMasterType.ReporterType)
    {
        ViewBag.MasterType = type;

        var list = await _context.CommonMasterOptions
            .Where(x => x.MasterType == type && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CommonMasterOptionViewModel
            {
                Id = x.Id,
                MasterType = x.MasterType,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult CreateCommon(CommonMasterType type)
    {
        var model = new CommonMasterOptionViewModel
        {
            MasterType = type,
            DisplayOrder = 1,
            IsActive = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCommon(CommonMasterOptionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var name = model.Name.Trim();

        var exists = await _context.CommonMasterOptions.AnyAsync(x =>
            x.MasterType == model.MasterType &&
            x.Name == name &&
            !x.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "This option already exists.");
            return View(model);
        }

        var entity = new CommonMasterOption
        {
            MasterType = model.MasterType,
            Name = name,
            Code = model.Code?.Trim(),
            Description = model.Description?.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.CommonMasterOptions.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Master option created successfully.";
        return RedirectToAction(nameof(Common), new { type = model.MasterType });
    }

    [HttpGet]
    public async Task<IActionResult> EditCommon(long id)
    {
        var entity = await _context.CommonMasterOptions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Master option not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CommonMasterOptionViewModel
        {
            Id = entity.Id,
            MasterType = entity.MasterType,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCommon(CommonMasterOptionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.CommonMasterOptions
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Master option not found.";
            return RedirectToAction(nameof(Index));
        }

        var name = model.Name.Trim();

        var duplicateExists = await _context.CommonMasterOptions.AnyAsync(x =>
            x.Id != model.Id &&
            x.MasterType == model.MasterType &&
            x.Name == name &&
            !x.IsDeleted);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(model.Name), "This option already exists.");
            return View(model);
        }

        entity.Name = name;
        entity.Code = model.Code?.Trim();
        entity.Description = model.Description?.Trim();
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsActive = model.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Master option updated successfully.";
        return RedirectToAction(nameof(Common), new { type = model.MasterType });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCommonStatus(long id)
    {
        var entity = await _context.CommonMasterOptions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Master option not found.";
            return RedirectToAction(nameof(Index));
        }

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = entity.IsActive
            ? "Master option activated successfully."
            : "Master option deactivated successfully.";

        return RedirectToAction(nameof(Common), new { type = entity.MasterType });
    }

    // ============================================================
    // PRODUCT MASTER
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Products()
    {
        var list = await _context.ProductMasters
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ProductName)
            .Select(x => new ProductMasterViewModel
            {
                Id = x.Id,
                ProductName = x.ProductName,
                GenericName = x.GenericName,
                ProductCode = x.ProductCode,
                ProductType = x.ProductType,
                Strength = x.Strength,
                DosageForm = x.DosageForm,
                ManufacturerName = x.ManufacturerName,
                MarketingAuthorizationHolder = x.MarketingAuthorizationHolder,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult CreateProduct()
    {
        return View(new ProductMasterViewModel
        {
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductMasterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var productName = model.ProductName.Trim();

        var exists = await _context.ProductMasters.AnyAsync(x =>
            x.ProductName == productName &&
            !x.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.ProductName), "This product already exists.");
            return View(model);
        }

        var entity = new ProductMaster
        {
            ProductName = productName,
            GenericName = model.GenericName?.Trim(),
            ProductCode = model.ProductCode?.Trim(),
            ProductType = model.ProductType?.Trim(),
            Strength = model.Strength?.Trim(),
            DosageForm = model.DosageForm?.Trim(),
            ManufacturerName = model.ManufacturerName?.Trim(),
            MarketingAuthorizationHolder = model.MarketingAuthorizationHolder?.Trim(),
            Remarks = model.Remarks?.Trim(),
            IsActive = model.IsActive,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.ProductMasters.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(long id)
    {
        var entity = await _context.ProductMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Products));
        }

        var model = new ProductMasterViewModel
        {
            Id = entity.Id,
            ProductName = entity.ProductName,
            GenericName = entity.GenericName,
            ProductCode = entity.ProductCode,
            ProductType = entity.ProductType,
            Strength = entity.Strength,
            DosageForm = entity.DosageForm,
            ManufacturerName = entity.ManufacturerName,
            MarketingAuthorizationHolder = entity.MarketingAuthorizationHolder,
            Remarks = entity.Remarks,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(ProductMasterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.ProductMasters
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Products));
        }

        var productName = model.ProductName.Trim();

        var duplicateExists = await _context.ProductMasters.AnyAsync(x =>
            x.Id != model.Id &&
            x.ProductName == productName &&
            !x.IsDeleted);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(model.ProductName), "This product already exists.");
            return View(model);
        }

        entity.ProductName = productName;
        entity.GenericName = model.GenericName?.Trim();
        entity.ProductCode = model.ProductCode?.Trim();
        entity.ProductType = model.ProductType?.Trim();
        entity.Strength = model.Strength?.Trim();
        entity.DosageForm = model.DosageForm?.Trim();
        entity.ManufacturerName = model.ManufacturerName?.Trim();
        entity.MarketingAuthorizationHolder = model.MarketingAuthorizationHolder?.Trim();
        entity.Remarks = model.Remarks?.Trim();
        entity.IsActive = model.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product updated successfully.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductStatus(long id)
    {
        var entity = await _context.ProductMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Products));
        }

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = entity.IsActive
            ? "Product activated successfully."
            : "Product deactivated successfully.";

        return RedirectToAction(nameof(Products));
    }

    // ============================================================
    // SPONSOR MASTER
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Sponsors()
    {
        var list = await _context.SponsorMasters
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SponsorName)
            .Select(x => new SponsorMasterViewModel
            {
                Id = x.Id,
                SponsorName = x.SponsorName,
                SponsorCode = x.SponsorCode,
                ContactPerson = x.ContactPerson,
                ContactEmail = x.ContactEmail,
                ContactPhone = x.ContactPhone,
                Address = x.Address,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult CreateSponsor()
    {
        return View(new SponsorMasterViewModel
        {
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSponsor(SponsorMasterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sponsorName = model.SponsorName.Trim();

        var exists = await _context.SponsorMasters.AnyAsync(x =>
            x.SponsorName == sponsorName &&
            !x.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.SponsorName), "This sponsor already exists.");
            return View(model);
        }

        var entity = new SponsorMaster
        {
            SponsorName = sponsorName,
            SponsorCode = model.SponsorCode?.Trim(),
            ContactPerson = model.ContactPerson?.Trim(),
            ContactEmail = model.ContactEmail?.Trim(),
            ContactPhone = model.ContactPhone?.Trim(),
            Address = model.Address?.Trim(),
            Remarks = model.Remarks?.Trim(),
            IsActive = model.IsActive,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.SponsorMasters.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Sponsor created successfully.";
        return RedirectToAction(nameof(Sponsors));
    }

    [HttpGet]
    public async Task<IActionResult> EditSponsor(long id)
    {
        var entity = await _context.SponsorMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Sponsor not found.";
            return RedirectToAction(nameof(Sponsors));
        }

        var model = new SponsorMasterViewModel
        {
            Id = entity.Id,
            SponsorName = entity.SponsorName,
            SponsorCode = entity.SponsorCode,
            ContactPerson = entity.ContactPerson,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            Address = entity.Address,
            Remarks = entity.Remarks,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSponsor(SponsorMasterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.SponsorMasters
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Sponsor not found.";
            return RedirectToAction(nameof(Sponsors));
        }

        var sponsorName = model.SponsorName.Trim();

        var duplicateExists = await _context.SponsorMasters.AnyAsync(x =>
            x.Id != model.Id &&
            x.SponsorName == sponsorName &&
            !x.IsDeleted);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(model.SponsorName), "This sponsor already exists.");
            return View(model);
        }

        entity.SponsorName = sponsorName;
        entity.SponsorCode = model.SponsorCode?.Trim();
        entity.ContactPerson = model.ContactPerson?.Trim();
        entity.ContactEmail = model.ContactEmail?.Trim();
        entity.ContactPhone = model.ContactPhone?.Trim();
        entity.Address = model.Address?.Trim();
        entity.Remarks = model.Remarks?.Trim();
        entity.IsActive = model.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Sponsor updated successfully.";
        return RedirectToAction(nameof(Sponsors));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSponsorStatus(long id)
    {
        var entity = await _context.SponsorMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Sponsor not found.";
            return RedirectToAction(nameof(Sponsors));
        }

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = entity.IsActive
            ? "Sponsor activated successfully."
            : "Sponsor deactivated successfully.";

        return RedirectToAction(nameof(Sponsors));
    }

    // ============================================================
    // STUDY MASTER
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Studies()
    {
        var list = await _context.StudyMasters
            .Include(x => x.SponsorMaster)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.StudyTitle)
            .Select(x => new StudyMasterViewModel
            {
                Id = x.Id,
                StudyTitle = x.StudyTitle,
                ProtocolNo = x.ProtocolNo,
                StudyCode = x.StudyCode,
                SponsorMasterId = x.SponsorMasterId,
                SponsorName = x.SponsorMaster != null ? x.SponsorMaster.SponsorName : null,
                Indication = x.Indication,
                StudyType = x.StudyType,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> CreateStudy()
    {
        var model = new StudyMasterViewModel
        {
            IsActive = true
        };

        await LoadSponsorOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudy(StudyMasterViewModel model)
    {
        await LoadSponsorOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var studyTitle = model.StudyTitle.Trim();
        var protocolNo = model.ProtocolNo?.Trim();

        var exists = await _context.StudyMasters.AnyAsync(x =>
            x.StudyTitle == studyTitle &&
            x.ProtocolNo == protocolNo &&
            !x.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.StudyTitle), "This study already exists.");
            return View(model);
        }

        var entity = new StudyMaster
        {
            StudyTitle = studyTitle,
            ProtocolNo = protocolNo,
            StudyCode = model.StudyCode?.Trim(),
            SponsorMasterId = model.SponsorMasterId,
            Indication = model.Indication?.Trim(),
            StudyType = model.StudyType?.Trim(),
            Remarks = model.Remarks?.Trim(),
            IsActive = model.IsActive,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.StudyMasters.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Study created successfully.";
        return RedirectToAction(nameof(Studies));
    }

    [HttpGet]
    public async Task<IActionResult> EditStudy(long id)
    {
        var entity = await _context.StudyMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Study not found.";
            return RedirectToAction(nameof(Studies));
        }

        var model = new StudyMasterViewModel
        {
            Id = entity.Id,
            StudyTitle = entity.StudyTitle,
            ProtocolNo = entity.ProtocolNo,
            StudyCode = entity.StudyCode,
            SponsorMasterId = entity.SponsorMasterId,
            Indication = entity.Indication,
            StudyType = entity.StudyType,
            Remarks = entity.Remarks,
            IsActive = entity.IsActive
        };

        await LoadSponsorOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStudy(StudyMasterViewModel model)
    {
        await LoadSponsorOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.StudyMasters
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Study not found.";
            return RedirectToAction(nameof(Studies));
        }

        var studyTitle = model.StudyTitle.Trim();
        var protocolNo = model.ProtocolNo?.Trim();

        var duplicateExists = await _context.StudyMasters.AnyAsync(x =>
            x.Id != model.Id &&
            x.StudyTitle == studyTitle &&
            x.ProtocolNo == protocolNo &&
            !x.IsDeleted);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(model.StudyTitle), "This study already exists.");
            return View(model);
        }

        entity.StudyTitle = studyTitle;
        entity.ProtocolNo = protocolNo;
        entity.StudyCode = model.StudyCode?.Trim();
        entity.SponsorMasterId = model.SponsorMasterId;
        entity.Indication = model.Indication?.Trim();
        entity.StudyType = model.StudyType?.Trim();
        entity.Remarks = model.Remarks?.Trim();
        entity.IsActive = model.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Study updated successfully.";
        return RedirectToAction(nameof(Studies));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStudyStatus(long id)
    {
        var entity = await _context.StudyMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Study not found.";
            return RedirectToAction(nameof(Studies));
        }

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = entity.IsActive
            ? "Study activated successfully."
            : "Study deactivated successfully.";

        return RedirectToAction(nameof(Studies));
    }

    // ============================================================
    // PRIVATE METHOD
    // ============================================================

    private async Task LoadSponsorOptionsAsync(StudyMasterViewModel model)
    {
        model.SponsorOptions = await _context.SponsorMasters
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SponsorName)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SponsorName
            })
            .ToListAsync();
    }
}