using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreAPI.Data;
using StoreAPI.Models;

namespace StoreAPI.Controllerrs;

[Authorize]  //ล็อกอินก่อน
//[Authorize(Roles =UserRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase{
//สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    //IwebHostEnitoment คืออะไร
    //IwebHostEnitoment เป็นอินเตอร์เฟสใน asp.net core ที่ใช้สำหรับดึงข้อมูลเกี่ยวกับสภาพแวดล้อมการโฮสต์เว็บแอป
    //Contentrootpath เป็นเส้นทางไปยังไฟลเกอร์รากของเว็บแอปพลิเคชัน
    private readonly IWebHostEnvironment _env;
    

    //สร้าง constructor รับค่า applicationDbCpntext
    public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    //ทดสอบเชื่้อม database 
    [HttpGet("testconnectdb")]
    public void TestConnection()
    {
        //ถ้าเชื่อมได้ แสดงข้อความ Connected
        if(_context.Database.CanConnect())
        {
            Response.WriteAsync("Connected");
        }
        //ถ้าเชื่อมไม่ได้ แสดงข้อความNot Connected
        else
        {
            Response.WriteAsync("Not Connected");
        }
    }

    //ฟังก์ชั่นสำหรับดึงข้อมูลสินค้าทั้งหมด
    //Get:/api/Product
    [HttpGet]
    public ActionResult<product>GetPrpducts()
    {
        //LINQ สำหรับการดึงข้อมูลจากตางราง products ทั้งหมด 
       // var products = _context.products.ToList();
        
        //แบบมีเงื่อนไช
        //var products = _context.products.Where( p =>p.unit_price>45000).ToList();

        //แบบเชื่อมเงื่อนไขกับตารางอื่น
        var products = _context.products.Join(
            _context.categories,
            p=>p.category_id,
            c=>c.category_id,
            (p,c)=>new
            {
            p.product_id,
            p.product_name,
            p.unit_price,
            p.unit_in_stock,
            c.category_name
            }
        ).ToList();

        //ส่งข้อมูลไปให้ผู้ใช้
        return Ok(products);

    }

    [HttpGet("{id}")]
    public ActionResult<product>Getproduct(int id)
    {
    var product = _context.products.FirstOrDefault(p=> p.product_id==id);

    //ถ้าไม่เจอสิ้นค้า
    if (product == null)
    {
        return NotFound();
    }
    return Ok(product);
    }
    
    [HttpPost]
    public async Task<ActionResult<product>> CreateProduct([FromForm] product product, IFormFile image)
    {
        //เพิ่มข้อมูลในตาราง Products
    _context.products.Add(product);

    //ตรวจสอบว่ามรการอัพโหลดรูปภาพหรือไม่
    if(image != null){
        //กำหนดชื่อไฟล์ใหม่
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension( image.FileName);

        //บันทึกไฟล์รูปภาพ
        string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");

        if(!Directory.Exists(uploadFolder)){
            Directory.CreateDirectory(uploadFolder);
        }
        using (var fileStream = new FileStream(Path.Combine(uploadFolder,fileName),FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }
        //บันทึกชื่อไฟล์ลงในฐานข้อมูล
        product.product_picture = fileName;
    }
    
    _context.SaveChanges();


    return Ok(product);
    }
    
    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id,product product)
    {
        var existingProduct = _context.products.FirstOrDefault(p=>p.product_id==id);
        if (existingProduct == null)
        {
            return NotFound();
        }
        existingProduct.product_name = product.product_name;
        existingProduct.unit_price = product.unit_price;
        existingProduct.unit_in_stock = product.unit_in_stock;
        existingProduct.category_id= product.category_id;

        _context.SaveChanges();

        return Ok(existingProduct);
    }
    [HttpDelete("{id}")]
    public ActionResult<product>DeleteProduct(int id)
    {
    var product = _context.products.FirstOrDefault(p=>p.product_id==id);
        if (product == null)
        {
            return NotFound();
        }
       
        _context.products.Remove(product);
        _context.SaveChanges();

        return Ok(product);
    }
}

