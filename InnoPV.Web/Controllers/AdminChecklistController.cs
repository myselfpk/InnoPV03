using InnoPV.Domain.Entities;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminChecklistController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminChecklistController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // CHECKLIST MASTER LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _context.ChecklistMasters
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.ApplicableRole)
            .ThenBy(x => x.ChecklistName)
            .Select(x => new ChecklistMasterViewModel
            {
                Id = x.Id,
                ChecklistName = x.ChecklistName,
                ApplicableRole = x.ApplicableRole,
                ApplicableStage = x.ApplicableStage,
                VersionNo = x.VersionNo,
                EffectiveFrom = x.EffectiveFrom,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(list);
    }

    // ============================================================
    // CREATE CHECKLIST MASTER
    // ============================================================

    [HttpGet]
    public IActionResult Create()
    {
        var model = new ChecklistMasterViewModel
        {
            VersionNo = "1.0",
            EffectiveFrom = DateTime.UtcNow,
            IsActive = true
        };

        LoadDropdowns(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChecklistMasterViewModel model)
    {
        LoadDropdowns(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var checklistName = model.ChecklistName.Trim();

        var exists = await _context.ChecklistMasters.AnyAsync(x =>
            x.ChecklistName == checklistName &&
            x.ApplicableRole == model.ApplicableRole &&
            x.ApplicableStage == model.ApplicableStage &&
            !x.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.ChecklistName), "Checklist already exists for this role and stage.");
            return View(model);
        }

        var entity = new ChecklistMaster
        {
            ChecklistName = checklistName,
            ApplicableRole = model.ApplicableRole,
            ApplicableStage = model.ApplicableStage,
            VersionNo = model.VersionNo.Trim(),
            EffectiveFrom = model.EffectiveFrom,
            IsActive = model.IsActive,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.ChecklistMasters.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Checklist created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // EDIT CHECKLIST MASTER
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var entity = await _context.ChecklistMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Checklist not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ChecklistMasterViewModel
        {
            Id = entity.Id,
            ChecklistName = entity.ChecklistName,
            ApplicableRole = entity.ApplicableRole,
            ApplicableStage = entity.ApplicableStage,
            VersionNo = entity.VersionNo,
            EffectiveFrom = entity.EffectiveFrom,
            IsActive = entity.IsActive
        };

        LoadDropdowns(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ChecklistMasterViewModel model)
    {
        LoadDropdowns(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.ChecklistMasters
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Checklist not found.";
            return RedirectToAction(nameof(Index));
        }

        var checklistName = model.ChecklistName.Trim();

        var duplicateExists = await _context.ChecklistMasters.AnyAsync(x =>
            x.Id != model.Id &&
            x.ChecklistName == checklistName &&
            x.ApplicableRole == model.ApplicableRole &&
            x.ApplicableStage == model.ApplicableStage &&
            !x.IsDeleted);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(model.ChecklistName), "Checklist already exists for this role and stage.");
            return View(model);
        }

        entity.ChecklistName = checklistName;
        entity.ApplicableRole = model.ApplicableRole;
        entity.ApplicableStage = model.ApplicableStage;
        entity.VersionNo = model.VersionNo.Trim();
        entity.EffectiveFrom = model.EffectiveFrom;
        entity.IsActive = model.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Checklist updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // ACTIVATE / DEACTIVATE CHECKLIST MASTER
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleChecklistStatus(long id)
    {
        var entity = await _context.ChecklistMasters
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Checklist not found.";
            return RedirectToAction(nameof(Index));
        }

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = entity.IsActive
            ? "Checklist activated successfully."
            : "Checklist deactivated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // CHECKLIST ITEMS LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Items(long checklistMasterId)
    {
        var checklist = await _context.ChecklistMasters
            .FirstOrDefaultAsync(x => x.Id == checklistMasterId && !x.IsDeleted);

        if (checklist == null)
        {
            TempData["ErrorMessage"] = "Checklist not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ChecklistMasterId = checklist.Id;
        ViewBag.ChecklistName = checklist.ChecklistName;
        ViewBag.ApplicableRole = checklist.ApplicableRole;
        ViewBag.ApplicableStage = checklist.ApplicableStage;

        var items = await _context.ChecklistItems
            .Where(x => x.ChecklistMasterId == checklistMasterId && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ItemText)
            .Select(x => new ChecklistItemViewModel
            {
                Id = x.Id,
                ChecklistMasterId = x.ChecklistMasterId,
                ChecklistName = checklist.ChecklistName,
                ItemText = x.ItemText,
                IsMandatory = x.IsMandatory,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(items);
    }

    // ============================================================
    // CREATE CHECKLIST ITEM
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> CreateItem(long checklistMasterId)
    {
        var checklist = await _context.ChecklistMasters
            .FirstOrDefaultAsync(x => x.Id == checklistMasterId && !x.IsDeleted);

        if (checklist == null)
        {
            TempData["ErrorMessage"] = "Checklist not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ChecklistItemViewModel
        {
            ChecklistMasterId = checklistMasterId,
            ChecklistName = checklist.ChecklistName,
            IsMandatory = true,
            IsActive = true,
            DisplayOrder = 1
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(ChecklistItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var checklist = await _context.ChecklistMasters
            .FirstOrDefaultAsync(x => x.Id == model.ChecklistMasterId && !x.IsDeleted);

        if (checklist == null)
        {
            TempData["ErrorMessage"] = "Checklist not found.";
            return RedirectToAction(nameof(Index));
        }

        var itemText = model.ItemText.Trim();

        var exists = await _context.ChecklistItems.AnyAsync(x =>
            x.ChecklistMasterId == model.ChecklistMasterId &&
            x.ItemText == itemText &&
            !x.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.ItemText), "This checklist item already exists.");
            model.ChecklistName = checklist.ChecklistName;
            return View(model);
        }

        var entity = new ChecklistItem
        {
            ChecklistMasterId = model.ChecklistMasterId,
            ItemText = itemText,
            IsMandatory = model.IsMandatory,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            CreatedOnUtc = DateTime.UtcNow
        };

        _context.ChecklistItems.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Checklist item created successfully.";
        return RedirectToAction(nameof(Items), new { checklistMasterId = model.ChecklistMasterId });
    }

    // ============================================================
    // EDIT CHECKLIST ITEM
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> EditItem(long id)
    {
        var entity = await _context.ChecklistItems
            .Include(x => x.ChecklistMaster)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Checklist item not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ChecklistItemViewModel
        {
            Id = entity.Id,
            ChecklistMasterId = entity.ChecklistMasterId,
            ChecklistName = entity.ChecklistMaster.ChecklistName,
            ItemText = entity.ItemText,
            IsMandatory = entity.IsMandatory,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(ChecklistItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _context.ChecklistItems
            .Include(x => x.ChecklistMaster)
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Checklist item not found.";
            return RedirectToAction(nameof(Index));
        }

        var itemText = model.ItemText.Trim();

        var duplicateExists = await _context.ChecklistItems.AnyAsync(x =>
            x.Id != model.Id &&
            x.ChecklistMasterId == model.ChecklistMasterId &&
            x.ItemText == itemText &&
            !x.IsDeleted);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(model.ItemText), "This checklist item already exists.");
            model.ChecklistName = entity.ChecklistMaster.ChecklistName;
            return View(model);
        }

        entity.ItemText = itemText;
        entity.IsMandatory = model.IsMandatory;
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsActive = model.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Checklist item updated successfully.";
        return RedirectToAction(nameof(Items), new { checklistMasterId = model.ChecklistMasterId });
    }

    // ============================================================
    // ACTIVATE / DEACTIVATE CHECKLIST ITEM
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleItemStatus(long id)
    {
        var entity = await _context.ChecklistItems
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (entity == null)
        {
            TempData["ErrorMessage"] = "Checklist item not found.";
            return RedirectToAction(nameof(Index));
        }

        entity.IsActive = !entity.IsActive;
        entity.ModifiedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = entity.IsActive
            ? "Checklist item activated successfully."
            : "Checklist item deactivated successfully.";

        return RedirectToAction(nameof(Items), new { checklistMasterId = entity.ChecklistMasterId });
    }

    // ============================================================
    // PRIVATE DROPDOWN METHOD
    // ============================================================

    private static List<SelectListItem> GetRoleOptions()
    {
        return new List<SelectListItem>
        {
            new() { Value = AppRoles.PvAssociate, Text = AppRoles.PvAssociate },
            new() { Value = AppRoles.PvManager, Text = AppRoles.PvManager },
            new() { Value = AppRoles.MedicalReviewer, Text = AppRoles.MedicalReviewer }
        };
    }

    private static List<SelectListItem> GetStageOptions()
    {
        return new List<SelectListItem>
        {
            new() { Value = "PV Associate Data Entry", Text = "PV Associate Data Entry" },
            new() { Value = "PV Manager QC Review", Text = "PV Manager QC Review" },
            new() { Value = "Medical Reviewer Assessment", Text = "Medical Reviewer Assessment" }
        };
    }

    private static void LoadDropdowns(ChecklistMasterViewModel model)
    {
        model.RoleOptions = GetRoleOptions();
        model.StageOptions = GetStageOptions();
    }
}