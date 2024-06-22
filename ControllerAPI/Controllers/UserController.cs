using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]// api/user
public class UserController : ControllerBase
{
//mock data for users
private static readonly List<User> _users = new List<User>
{
    new User {

             Id=1,
             Username="john",
             Email="john@email.com",
             Fullname="john Doe"
        },
        new User {

             Id=2,
             Username="samit",
             Email="samit@email.com",
             Fullname="samit Doe"
        },
};

//get:api/User
[HttpGet]
public ActionResult<IEnumerable<User>> GetUsers()
{     
    //   IEnumerable คืออะไร
    //   IEnumerable เป็น interface ใน .net framework ที่ใช้แทน collection ของ object
    //   interface นี้กำหนด method เพียงตัวเดียวคือ get Enumerator()

    //วนซ้ำผ่าน collection โดยใช้ foreach
    //foreach(var user in _users)
    //{
     // Console.WriteLine($"{user.Id} - {user.Username}");
    //}
    return Ok(_users);
}

//get user by ID
//get:api/User/1
[HttpGet("{id}")]
public ActionResult<User> GetUser(int id)
{     
    //LINQ
    var user = _users.Find(u => u.Id == id); //find user by Id
    if (user == null){
        return NotFound();
    }
    return Ok(user);
}

//Creat new user
//POST: api/User
[HttpPost]
public ActionResult<User> CreateUser([FromBody] User user)
{
    _users.Add(user);
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
//Creat new user
//POST: api/User
[HttpPut("{id}")]
public IActionResult UpdateUser(int id, [FromBody] User user)
{
    //validate user id
    if (id != user.Id){
        return BadRequest();
    }

    //find existing user
    var existingUser = _users.Find(u => u.Id == id);
    if (existingUser == null){
        return NotFound();
    }

    //update user
    existingUser.Username = user.Username;
    existingUser.Email = user.Email;
    existingUser.Fullname = user.Fullname;

    //retune updated user
    return Ok(existingUser);
}
[HttpDelete("{id}")]
public ActionResult DeleteUser(int id)
{
    var user = _users.Find(u => u.Id == id);
    if(user == null){
        return NotFound();
    }
    _users.Remove(user);
    return NoContent();
}

}