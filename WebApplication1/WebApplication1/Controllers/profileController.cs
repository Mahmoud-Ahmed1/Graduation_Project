
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using WebApplication1.models;
using WebApplication1.models.dto;
using WebApplication1.Repository.Repository;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class profileController : ControllerBase
    {


        private readonly ImangeprofileRepository _profileRepository;
        private readonly IfriendRepository _friendRepository;
        public profileController( ImangeprofileRepository profileRepository, IfriendRepository friendRepository)
        {

            _profileRepository = profileRepository;

            _friendRepository = friendRepository;
        }

        

        private async Task<string> WriteFile(IFormFile file)
        {
            string filename = "";
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            filename = DateTime.Now.Ticks.ToString() + extension;

            // Define your desired path
            var filepath = @"C:\Users\Almodather\Downloads\0\public\ProfilePicture";


            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            // Combine the path with the filename
            var exactpath = Path.Combine(filepath, filename);

            // Save the file to the specified path
            using (var stream = new FileStream(exactpath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            return filename;
        }


  
        [HttpGet("getUserLogin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]


        public async Task<ActionResult<ApplicationUser>> Getuserlogin()

        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;

            if (userId == null)
                return BadRequest();
            var User = await _profileRepository.Get(e => e.Id == userId);



            {
                if (User == null) return NotFound();

                else
                {
                    return Ok(User);
                }
            }

        }


        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<post>> Delete()

        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
            var User = await _profileRepository.Get(e => e.Id == id);

            if (id == null)
                return BadRequest();
            else
            {
                if (User == null) return NotFound();
                else await _profileRepository.Remove(User);
            }
            await _profileRepository.save();
            return NoContent();



        }





        [HttpPut("updatedProfile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUser>> UpdateUserProfile(UpdateProfileDto updatedProfile)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
            var existingUser = await _profileRepository.Get(e => e.Id == id);

            if (existingUser == null)
            {
                return NotFound();
            }

            var existingUsername = await _profileRepository.Get(e => e.UserName == updatedProfile.UserName);
            if (existingUsername != null && existingUsername.Id != existingUser.Id)
            {

                return BadRequest("Username is already taken");
            }

            var existingEmail = await _profileRepository.Get(e => e.Email == updatedProfile.Email);
            if (existingEmail != null && existingEmail.Id != existingUser.Id)
            {

                return BadRequest("Email is already taken");
            }
            // Check if any field is updated
            if (existingUser.fname == updatedProfile.fname &&
                 existingUser.lname == updatedProfile.lname &&
                 existingUser.UserName == updatedProfile.UserName &&
                 existingUser.Email == updatedProfile.Email &&
                 existingUser.BIO == updatedProfile.BIO &&
                 existingUser.PhoneNumber == updatedProfile.phone &&
                 existingUser.Birthdate == updatedProfile.Birthdate &&
                 existingUser.Birthdate == updatedProfile.Birthdate &&
                 existingUser.ginder == updatedProfile.ginder)
            {
                return BadRequest("No changes detected. Please provide updated information.");
            }

            // Update the user profile
            existingUser.fname = updatedProfile.fname ?? existingUser.fname;
            existingUser.lname = updatedProfile.lname ?? existingUser.lname;
            existingUser.UserName = updatedProfile.UserName ?? existingUser.UserName;
            existingUser.Email = updatedProfile.Email ?? existingUser.Email;
            existingUser.NormalizedUserName = (updatedProfile.UserName ?? existingUser.UserName).ToUpper();
            existingUser.NormalizedEmail = (updatedProfile.Email ?? existingUser.Email).ToUpper();

            existingUser.BIO = updatedProfile.BIO;
            existingUser.PhoneNumber = updatedProfile.phone;
            existingUser.Birthdate = updatedProfile.Birthdate != DateTime.MinValue ? updatedProfile.Birthdate : existingUser.Birthdate;
            existingUser.ginder = updatedProfile.ginder;
            await _profileRepository.Update(existingUser);

            return Ok("The data has been updated successfully");
        }



        [HttpPost("createdata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUser>> createdata(createdatadto createdatadto)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
            var existingUser = await _profileRepository.Get(e => e.Id == id);

            if (existingUser == null)
            {
                return NotFound();
            }
            // Check if any field is updated
            if (
                existingUser.BIO == createdatadto.bio &&
                
                existingUser.ginder == createdatadto.ginder)
            {
                return BadRequest("No changes detected. Please provide updated information.");
            }
            existingUser.BIO = createdatadto.bio;
            existingUser.ginder = createdatadto.ginder;
            
            await _profileRepository.Update(existingUser);

            return Ok(createdatadto);
        }




        [HttpPut("profile/changepassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            var user = await _profileRepository.GetSpecialEntity<ApplicationUser>(e => e.Id == userId);

            if (user == null)
            { return NotFound("User not found"); }

            var errors = new List<string>();
            // Check if any required fields are null
            if (changePassword.NewPassword == null || changePassword.ConfirmPassword == null || changePassword.CurrentPassword == null)
            {
                errors.Add("New Password, Confirm Password, and Current Password are required");
            }

            // Check if New Password matches Confirm Password
            if (changePassword.NewPassword != changePassword.ConfirmPassword)
            {
                errors.Add("New Password and Confirm Password do not match");
            }

            // Check if New Password is the same as Current Password
            if (changePassword.NewPassword == changePassword.CurrentPassword)
            {
                errors.Add("New Password and Current Password are the same");
            }
            // If there are any errors, return them
            if (errors.Any())
            {
                return BadRequest(errors);
            }
            var result = await _profileRepository.ChangePassword(user, changePassword);


            if (result is not ApplicationUser)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost("uploadProfilePicture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUser>> uploadProfilePicture(IFormFile file)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
            var existingUser = await _profileRepository.Get(e => e.Id == id);

            if (existingUser == null)
            {
                return NotFound();
            }
            var result = await WriteFile(file);
            existingUser.imgeurl = result;


            await _profileRepository.Update(existingUser);

            return Ok(existingUser);
        }



        [HttpPut("Private")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUser>> Private( )
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
            var existingUser = await _profileRepository.Get(e => e.Id == id);

            if (existingUser == null)
            {
                return NotFound();
            }



            existingUser.IsPrivate = true;
          
            await _profileRepository.Update(existingUser);

            return Ok("Your account is private..");
        }
        
        
        [HttpPut("public")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUser>> publicAccount ()
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
            var existingUser = await _profileRepository.Get(e => e.Id == id);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.IsPrivate = false;

           await _profileRepository.Update(existingUser);

            return Ok("Your account is private..");
        }



    }
}
