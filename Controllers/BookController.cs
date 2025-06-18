using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web_Buoi5.Models;
using Microsoft.Extensions.Logging; // Thêm namespace cho ILogger

namespace Web_Buoi5.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BookController> _logger; // Thêm logger

        public BookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<BookController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger; // Khởi tạo logger
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Truy cập Index của BookController lúc {Time}", DateTime.Now); // Log truy cập
            var books = await _context.Books.Include(b => b.Category).ToListAsync();
            return View(books);
        }

        public IActionResult Create()
        {
            _logger.LogInformation("Truy cập Create của BookController lúc {Time}", DateTime.Now); // Log truy cập
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? ImageFile)
        {
            _logger.LogInformation("Bắt đầu tạo sản phẩm {ProductName} lúc {Time}", book.Title, DateTime.Now); // Log bắt đầu
            // if (!ModelState.IsValid) // Bỏ tạm thời như code của bạn
            // {
            //     ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            //     return View(book);
            // }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folder = Path.Combine(_webHostEnvironment.WebRootPath, "ImageBooks");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    book.ImagePath = "/ImageBooks/" + fileName;
                    _logger.LogInformation("Lưu ảnh thành công cho sản phẩm {ProductName}", book.Title); // Log thành công
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lưu ảnh cho sản phẩm {ProductName}", book.Title); // Log lỗi
                    ModelState.AddModelError("", "Lỗi khi lưu ảnh: " + ex.Message);
                    ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                    return View(book);
                }
            }

            _context.Add(book);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Thêm sản phẩm {ProductName} thành công lúc {Time}", book.Title, DateTime.Now); // Log thành công
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("Truy cập Details cho sản phẩm ID {Id} lúc {Time}", id, DateTime.Now); // Log truy cập
            if (id == null) return NotFound();
            var book = await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("Truy cập Edit cho sản phẩm ID {Id} lúc {Time}", id, DateTime.Now); // Log truy cập
            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? ImageFile)
        {
            _logger.LogInformation("Bắt đầu chỉnh sửa sản phẩm ID {Id} lúc {Time}", id, DateTime.Now); // Log bắt đầu
            if (id != book.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                return View(book);
            }

            var existing = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null) return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existing.ImagePath))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existing.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                        _logger.LogInformation("Xóa ảnh cũ của sản phẩm ID {Id}", id); // Log xóa ảnh
                    }
                }

                var folder = Path.Combine(_webHostEnvironment.WebRootPath, "ImageBooks");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                book.ImagePath = "/ImageBooks/" + fileName;
                _logger.LogInformation("Cập nhật ảnh mới cho sản phẩm ID {Id}", id); // Log cập nhật ảnh
            }
            else
            {
                book.ImagePath = existing.ImagePath;
            }

            _context.Update(book);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cập nhật sản phẩm ID {Id} thành công lúc {Time}", id, DateTime.Now); // Log thành công
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("Truy cập Delete cho sản phẩm ID {Id} lúc {Time}", id, DateTime.Now); // Log truy cập
            if (id == null) return NotFound();

            var book = await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Bắt đầu xóa sản phẩm ID {Id} lúc {Time}", id, DateTime.Now); // Log bắt đầu
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                if (!string.IsNullOrEmpty(book.ImagePath))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, book.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation("Xóa ảnh của sản phẩm ID {Id}", id); // Log xóa ảnh
                    }
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Xóa sản phẩm ID {Id} thành công lúc {Time}", id, DateTime.Now); // Log thành công
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(b => b.Id == id);
        }
    }
}