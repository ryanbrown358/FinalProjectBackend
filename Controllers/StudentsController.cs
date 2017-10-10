using FinalProjectBackend.Data;
using FinalProjectBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProjectBackend.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Students
        // public async Task<IActionResult> Index()
        //{
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? page)
        {

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
           
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;

            var students = from s in _context.Students select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                               || s.FirstMidName.Contains(searchString));
            }

            switch (sortOrder)
                {
                    case "name_desc":
                        students = students.OrderByDescending(s => s.LastName);
                        break;
                    case "Date":
                        students = students.OrderBy(s => s.EnrollmentDate);
                        break;
                    case "date_desc":
                        students = students.OrderByDescending(s => s.EnrollmentDate);
                        break;
                    default:
                        students = students.OrderBy(s => s.LastName);
                        break;
                }
                    int pageSize = 3;
                    return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), page ?? 1, pageSize));
        }

            //return View(await _context.Students.ToListAsync());
       // }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)

                return NotFound();

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("EnrollmentDate,FirstMidName,LastName")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists " +
                                             "see your system administrator.");
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students.SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
                return NotFound();
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (id == null)
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                return NotFound();
            //As a best practice to prevent overposting, 
            //the fields that you want to be updateable by the Edit page are whitelisted in the TryUpdateModel parameters. 
            //(The empty string preceding the list of fields in the parameter list is for a prefix to use with the form fields names.) 
            //Currently there are no extra fields that you're protecting, 
            //but listing the fields that you want the model binder to bind ensures that if you add fields to the data model in the future, 
            //they're automatically protected until you explicitly add them here.
            var studentToUpdate = await _context.Students.SingleOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync(
                studentToUpdate,
                "",
                s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                                                 "Try again, and if the problem persists, " +
                                                 "see your system administrator.");
                }
            return View(studentToUpdate);
        }

        // GET: Students/Delete/5 //   //You'll add a try-catch block to the HttpPost Delete method to handle any errors that might occur when the database is updated. //
        //If an error occurs, the HttpPost Delete method calls the HttpGet Delete method, 
        //passing it a parameter that indicates that an error has occurred. 
        //The HttpGet Delete method then redisplays the confirmation page along with the error message, 
        //giving the user an opportunity to cancel or try again.
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
     
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}


/*An entity may be in one of the following states:1
Added. The entity does not yet exist in the database. The SaveChanges method issues an INSERT statement.
Unchanged. Nothing needs to be done with this entity by the SaveChanges method. When you read an entity from the database, the entity starts out with this status.
Modified. Some or all of the entity's property values have been modified. The SaveChanges method issues an UPDATE statement.
Deleted. The entity has been marked for deletion. The SaveChanges method issues a DELETE statement.
Detached. The entity isn't being tracked by the database context.
 */



/* If improving performance in a high-volume application is a priority, 
 * you could avoid an unnecessary SQL query by instantiating a Student entity using only the primary key value and then setting the entity state to Deleted. 
 * That's all that the Entity Framework needs in order to delete the entity. 
 * (Don't put this code in your project; it's here just to illustrate an alternative.)
 
     [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    try
    {
        Student studentToDelete = new Student() { ID = id };
        _context.Entry(studentToDelete).State = EntityState.Deleted;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    catch (DbUpdateException /* ex)
{
Log the error (uncomment ex variable name and write a log.)
return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
}
}
*/


    




