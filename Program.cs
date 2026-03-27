using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
namespace LibraryManagementSystem
{
    public interface IBorrowable
    {
        void BorrowBook();
        void ReturnBook();
    }

    public abstract class Book
    {

        private string _isbn;
        public string ISBN 
        { 
            get => _isbn; 
            set => _isbn = string.IsNullOrWhiteSpace(value) ? "N/A" : value; 
        }

        public string Title { get; set; }
        public string Author { get; set; }

        private int _publicationYear;
        public int PublicationYear 
        { 
            get => _publicationYear; 
            set => _publicationYear = (value >= 1900 && value <= DateTime.Now.Year) ? value : 1900; 
        }

        private double _price;
        public double Price 
        { 
            get => _price; 
            set => _price = value >= 0 ? value : 0; 
        }

        public bool IsAvailable { get; set; } = true;
        public DateTime BorrowedDate { get; set; }

        
        public abstract string GetCategory();
        public abstract double CalculateFine(DateTime returnDate);

       
        public virtual void DisplayInfo()
        {
            Console.WriteLine($"[{GetCategory()}] ISBN: {ISBN,-15} | Tên: {Title,-20} | Tác giả: {Author,-15} | Năm: {PublicationYear} | Giá: {Price:N0}đ | {(IsAvailable ? "Trống" : "Đã mượn")}");
        }
    }

   
    public class FictionBook : Book, IBorrowable
    {
        public string Genre { get; set; }
        public override string GetCategory() => $"Tiểu thuyết ({Genre})";
        
        public override double CalculateFine(DateTime returnDate)
        {
            int days = (returnDate - BorrowedDate).Days;
            return days > 14 ? (days - 14) * 5000 : 0;
        }

        public void BorrowBook() => IsAvailable = false;
        public void ReturnBook() => IsAvailable = true;
    }

    public class Textbook : Book, IBorrowable
    {
        public string Subject { get; set; }
        public int Edition { get; set; }
        public override string GetCategory() => $"Sách giáo khoa ({Subject} - Lần {Edition})";

        public override double CalculateFine(DateTime returnDate)
        {
            int days = (returnDate - BorrowedDate).Days;
            return days > 14 ? (days - 14) * 3000 : 0;
        }

        public void BorrowBook() => IsAvailable = false;
        public void ReturnBook() => IsAvailable = true;
    }

    public class ReferenceBook : Book, IBorrowable
    {
        public string Publisher { get; set; }
        public bool CanBeBorrowed { get; set; }
        public override string GetCategory() => "Sách tham khảo";

        public override double CalculateFine(DateTime returnDate)
        {
            int days = (returnDate - BorrowedDate).Days;
            return days > 14 ? (days - 14) * 10000 : 0;
        }

        public void BorrowBook() => IsAvailable = false;
        public void ReturnBook() => IsAvailable = true;
    }

  
    public class LibraryManager
    {
        private List<Book> _books = new List<Book>();
        private const string DataFile = "library_data.json";

        public void AddBook(Book book)
        {
            if (_books.Any(b => b.ISBN == book.ISBN))
            {
                Console.WriteLine("!!! Lỗi: Mã ISBN đã tồn tại trong hệ thống.");
                return;
            }
            _books.Add(book);
            Console.WriteLine("=> Thêm sách thành công!");
        }

        public void DisplayAll()
        {
            Console.WriteLine("\n--- DANH SÁCH TẤT CẢ SÁCH ---");
            if (!_books.Any()) Console.WriteLine("Thư viện trống.");
           
            foreach (var book in _books) book.DisplayInfo();
        }

        public void Search(string keyword)
        {
            var results = _books.Where(b => b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) || b.ISBN == keyword).ToList();
            if (!results.Any()) Console.WriteLine("Không tìm thấy sách nào.");
            else results.ForEach(b => b.DisplayInfo());
        }

        public void Borrow(string isbn)
        {
            var book = _books.FirstOrDefault(b => b.ISBN == isbn);
            if (book == null) { Console.WriteLine("Không tìm thấy ISBN này."); return; }
            
            if (book is ReferenceBook rb && !rb.CanBeBorrowed)
            {
                Console.WriteLine("Đây là sách tham khảo đặc biệt, không được mượn về.");
                return;
            }

            if (book.IsAvailable)
            {
                book.IsAvailable = false;
                book.BorrowedDate = DateTime.Now;
                Console.WriteLine($"Đã mượn thành công cuốn: {book.Title} vào ngày {book.BorrowedDate:dd/MM/yyyy}");
            }
            else Console.WriteLine("Sách hiện đã có người mượn.");
        }

        public void Return(string isbn)
        {
            var book = _books.FirstOrDefault(b => b.ISBN == isbn);
            if (book == null || book.IsAvailable) { Console.WriteLine("Dữ liệu không hợp lệ."); return; }

            double fine = book.CalculateFine(DateTime.Now);
            book.IsAvailable = true;
            Console.WriteLine($"Đã trả sách: {book.Title}. Phí phạt: {fine:N0}đ");
        }

        public void ShowStats()
        {
            int total = _books.Count;
            int borrowed = _books.Count(b => !b.IsAvailable);
            Console.WriteLine($"\n--- THỐNG KÊ ---");
            Console.WriteLine($"Tổng số sách: {total}");
            Console.WriteLine($"Đang cho mượn: {borrowed}");
            Console.WriteLine($"Sẵn sàng: {total - borrowed}");
        }

       
       

    class Program
    {
        static LibraryManager manager = new LibraryManager();

        static void Main(string[] args)
        {   
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n===== HỆ THỐNG QUẢN LÝ THƯ VIỆN OOP =====");
                Console.WriteLine("1. Thêm sách mới");
                Console.WriteLine("2. Hiển thị danh sách sách");
                Console.WriteLine("3. Tìm kiếm sách (ISBN/Tên)");
                Console.WriteLine("4. Mượn sách");
                Console.WriteLine("5. Trả sách");
                Console.WriteLine("6. Thống kê");
                Console.WriteLine("0. Thoát");
                Console.Write("Chọn chức năng: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": InputBook(); break;
                    case "2": manager.DisplayAll(); break;
                    case "3": 
                        Console.Write("Nhập từ khóa: ");
                        manager.Search(Console.ReadLine());
                        break;
                    case "4":
                        Console.Write("Nhập ISBN để mượn: ");
                        manager.Borrow(Console.ReadLine());
                        break;
                    case "5":
                        Console.Write("Nhập ISBN để trả: ");
                        manager.Return(Console.ReadLine());
                        break;
                    case "6": manager.ShowStats(); break;
                    case "0": exit = true; break;
                    default: Console.WriteLine("Lựa chọn không hợp lệ!"); break;
                }
            }
        }

        static void InputBook()
        {   
            Console.WriteLine("\nChọn loại sách: 1. Fiction | 2. Textbook | 3. Reference");
            string type = Console.ReadLine();
            
            Console.Write("Nhập ISBN: "); string isbn = Console.ReadLine();
            Console.Write("Nhập Tiêu đề: "); string title = Console.ReadLine();
            Console.Write("Nhập Tác giả: "); string author = Console.ReadLine();
            
            int year;
            Console.Write("Nhập Năm XB (1900-Nay): ");
            while (!int.TryParse(Console.ReadLine(), out year) || year < 1900 || year > DateTime.Now.Year) 
                Console.Write("Sai định dạng! Nhập lại năm: ");

            double price;
            Console.Write("Nhập Giá: ");
            while (!double.TryParse(Console.ReadLine(), out price) || price < 0)
                Console.Write("Giá không hợp lệ! Nhập lại giá: ");

            switch (type)
            {
                case "1":
                    Console.Write("Nhập Thể loại: ");
                    manager.AddBook(new FictionBook { ISBN = isbn, Title = title, Author = author, PublicationYear = year, Price = price, Genre = Console.ReadLine() });
                    break;
                case "2":
                    Console.Write("Nhập Môn học: "); string sub = Console.ReadLine();
                    Console.Write("Tái bản lần thứ: "); int ed = int.Parse(Console.ReadLine());
                    manager.AddBook(new Textbook { ISBN = isbn, Title = title, Author = author, PublicationYear = year, Price = price, Subject = sub, Edition = ed });
                    break;
                case "3":
                    Console.Write("Nhập Nhà XB: "); string pub = Console.ReadLine();
                    Console.Write("Có cho mượn không? (y/n): "); bool can = Console.ReadLine().ToLower() == "y";
                    manager.AddBook(new ReferenceBook { ISBN = isbn, Title = title, Author = author, PublicationYear = year, Price = price, Publisher = pub, CanBeBorrowed = can });
                    break;
            }
        }
    }
    }
}
    
