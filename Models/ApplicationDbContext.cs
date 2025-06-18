using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Web_Buoi5.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId);

            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasPrecision(18, 2);

            // Seed data cho Category
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Văn học" },
                new Category { Id = 2, Name = "Tâm lý" }
            );

            // Seed data cho Book
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Sách Trốn Lên Mái Nhà Khóc",
                    Author = "Nguyễn Nhật Ánh",
                    Price = 120000m, // Sử dụng kiểu decimal
                    Description = "Tập hợp những câu chuyện ngắn đầy cảm xúc về tuổi thơ.",
                    ImagePath = "/ImageBooks/b1.jpg",
                    CategoryId = 1
                },
                new Book
                {
                    Id = 2,
                    Title = "Tôi Thấy Hoa Vàng Trên Cỏ Xanh",
                    Author = "Nguyễn Nhật Ánh",
                    Price = 150000m, // Sử dụng kiểu decimal
                    Description = "Hồi ức tuổi thơ đầy cảm xúc tại làng quê Việt Nam.",
                    ImagePath = "/ImageBooks/b2.jpg",
                    CategoryId = 1
                },
                new Book
                {
                    Id = 3,
                    Title = "Đắc Nhân Tâm",
                    Author = "Dale Carnegie",
                    Price = 180000m, // Sử dụng kiểu decimal
                    Description = "Cuốn sách kinh điển về nghệ thuật giao tiếp và thuyết phục.",
                    ImagePath = "/ImageBooks/b3.jpg",
                    CategoryId = 2
                }
            );
        }
    }

    // Định nghĩa model Book
 }