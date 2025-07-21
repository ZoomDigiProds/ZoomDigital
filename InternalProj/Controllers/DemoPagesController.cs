using InternalProj.Filters;
using Microsoft.AspNetCore.Mvc;

public class DemoPagesController : Controller
{
    [DepartmentAuthorize("FRONT OFFICE")]
    public IActionResult FrontOffice()
    {
        return View();
    }

    [DepartmentAuthorize("ACCOUNTS")]
    public IActionResult Accounts()
    {
        return View();
    }

    [DepartmentAuthorize("CORRECTION")]
    public IActionResult Correction()
    {
        return View();
    }

    [DepartmentAuthorize("COPYING")]
    public IActionResult Copying()
    {
        return View();
    }

    [DepartmentAuthorize("CROPPING")]
    public IActionResult Cropping()
    {
        return View();
    }

    [DepartmentAuthorize("PDF")]
    public IActionResult PDF()
    {
        return View();
    }

    [DepartmentAuthorize("PRINTING")]
    public IActionResult Printing()
    {
        return View();
    }

    [DepartmentAuthorize("COVER PAGE")]
    public IActionResult CoverPage()
    {
        return View();
    }

    [DepartmentAuthorize("MAKING")]
    public IActionResult Making()
    {
        return View();
    }

    [DepartmentAuthorize("EMBOSSING")]
    public IActionResult Embossing()
    {
        return View();
    }

    [DepartmentAuthorize("DELIVER")]
    public IActionResult Deliver()
    {
        return View();
    }
}
