using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrashPickUp.Data;
using TrashPickUp.Models;

namespace TrashPickUp.Controllers
{
   

        [Authorize(Roles = "Employee")]
        public class EmployeesController : Controller
        {
            private readonly ApplicationDbContext _context;

            public EmployeesController(ApplicationDbContext context)
            {
                _context = context;
            }

            // GET: Employees
            public async Task<IActionResult> Index()
            {
                //need to come back to this and try to add the system.DayOfWeek to the selectlist maybe with foreach?
                List<SelectListItem> days = new List<SelectListItem>();
                days.Add(new SelectListItem { Text = "Sunday", Value = "0" });
                days.Add(new SelectListItem { Text = "Monday", Value = "1" });
                days.Add(new SelectListItem { Text = "Tuesday", Value = "2" });
                days.Add(new SelectListItem { Text = "Wednesday", Value = "3" });
                days.Add(new SelectListItem { Text = "Thursday", Value = "4" });
                days.Add(new SelectListItem { Text = "Friday", Value = "5" });
                days.Add(new SelectListItem { Text = "Saturday", Value = "6" });
                ViewBag.Day = new SelectList(days, "Value", "Text", $"{(int)DateTime.Today.DayOfWeek}");

                //var weekDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();
                //foreach (var item in weekDays)
                //{
                //    var listItem = new SelectListItem { Value = item.ToString(), Text = item.ToString() };
                //    //listItem.Selected = today.Day == item.Day;
                //    list.Add(listItem);
                //}

                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var employee = _context.Employees.Where(c => c.IdentityUserId == userId).SingleOrDefault();

                var applicationDbContext = _context.Customers.Where(c => c.ZipCode == employee.ZipCode);
                return View(await applicationDbContext.ToListAsync());
            }

            //[HttpPost]
            //[ValidateAntiForgeryToken]
            public async Task<IActionResult> Filter(DayOfWeek? Day)
            {
                //Get employee
                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var employee = _context.Employees.Where(c => c.IdentityUserId == userId).SingleOrDefault();

                //What day of the week is it today?
                DayOfWeek today = DateTime.Today.DayOfWeek;
                //What date was Sunday of this week?
                DateTime sundayOfThisWeek = DateTime.Today.Subtract(new TimeSpan(Convert.ToInt32(today), 0, 0, 0));
                //What is the date of the the user selected? Add the int value of the day that the user selected to Sunday's date.
                DateTime selectedDate = sundayOfThisWeek.AddDays(Convert.ToDouble(Day));

                //apply filtering to db with linq
                var applicationDbContext = _context.Customers
                    .Where(c => c.CollectionDay == Day || c.ExtraCollectionDay == Day)
                    .Where(c => c.ZipCode == employee.ZipCode)
                    .Where(c => c.StartDate > selectedDate || c.EndDate < selectedDate);

                //send data 
                ViewBag.Day = Day;
                ViewBag.Date = selectedDate.ToString("MM-dd-yyyy");
                return View(await applicationDbContext.ToListAsync());
            }

            // GET: Employees/Details/5
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var customer = await _context.Customers
                    .Include(e => e.IdentityUser)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }

            // GET: Employees/Create
            public IActionResult Create()
            {
                Employee employee = new Employee();
                ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id");
                return View(employee);
            }

            // POST: Employees/Create
            // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
            // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create([Bind("Id,Name,ZipCode,IdentityUserId")] Employee employee)
            {
                if (ModelState.IsValid)
                {
                    if (employee.Id == 0)
                    {
                        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        employee.IdentityUserId = userId;
                        _context.Add(employee);
                    }
                    else
                    {
                        var employeeInDB = _context.Employees.Single(m => m.Id == employee.Id);
                        employeeInDB.Name = employee.Name;
                        employeeInDB.ZipCode = employee.ZipCode;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", employee.IdentityUserId);
                return View(employee);
            }

            // GET: Employees/Edit/5
            public async Task<IActionResult> Edit(int? id)
            {
                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var employee = _context.Employees.Where(c => c.IdentityUserId == userId).SingleOrDefault();
                if (id == null)
                {
                    return NotFound();
                }

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }
                ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", employee.IdentityUserId);
                return View(customer);
            }

            // POST: Employees/Edit/5
            // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
            // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int? id, [Bind("Balance, Address, ZipCode, PickupConfirmationDate, ExtraCollectionDayConfirmation ")]Customer customer)
            {
                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var employee = _context.Employees.Where(c => c.IdentityUserId == userId).SingleOrDefault();
                customer = _context.Customers.Single(m => m.Id == id);

                if (id != customer.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        customer.PickupConfirmationDate = DateTime.Now;
                        customer.Balance += 20;
                        _context.Update(customer);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!EmployeeExists(customer.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", customer.IdentityUserId);
                return View(customer);

            }

            // GET: Employees/Delete/5
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var employee = await _context.Employees
                    .Include(e => e.IdentityUser)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (employee == null)
                {
                    return NotFound();
                }

                return View(employee);
            }

            // POST: Employees/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var employee = await _context.Employees.FindAsync(id);
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            private bool EmployeeExists(int id)
            {
                return _context.Employees.Any(e => e.Id == id);
            }
        }
    }
