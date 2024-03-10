using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using WebApplication1.models;
using WebApplication1.models.dto;
using WebApplication1.Repository.Repository.ProjectsRepository;

namespace WebApplication1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectsRepository _projectsRepository;
        private readonly appDbcontext1 _appDbcontext1;
        public ProjectController(IProjectsRepository projectsRepository, appDbcontext1 appDbcontext1)
        {
            _projectsRepository = projectsRepository;
            _appDbcontext1 = appDbcontext1;
        }


        [HttpPost("UploadFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file , string Brief)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;

            var usernameClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "username");
            var username = usernameClaim?.Value;
            if (file == null)
                return BadRequest();

            var filename = await _projectsRepository.WriteFile(file);

            if (filename == null)
                return BadRequest();

            // Save file information to the database
            var fileInformation = new Projects
            {
                FilePath = @"D:\facutly\GP\WebApplication1\FilesProjects",
                FileName = filename,
                UserId = userId,
                UserName = username,
                Status = "Pending",
                Brief = Brief
            };

            _appDbcontext1.projects.Add(fileInformation);
            await _appDbcontext1.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("aprovedProject")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ApprovedProject(int id)
        {
            var project = await _projectsRepository.GetFileById(e => e.Id == id);
            if (project == null)
                return BadRequest();
            project.Status = "Approved";
            await _appDbcontext1.SaveChangesAsync();
            return Ok("Approved");
        }

    
        [HttpPut("RejectedProject")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RejectedProject(int id)
        {
            var project = await _projectsRepository.GetFileById(e => e.Id == id);
            if (project == null)
                return BadRequest();
            project.Status = "Rejected";
            await _appDbcontext1.SaveChangesAsync();
            return Ok("Rejected");
        }



        [HttpGet("GetPendingProject")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPendingProjects()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opject = await _projectsRepository.GetAllPendingProjects();
            if (opject == null)
                return NotFound();
            return Ok(opject);
        }

        [HttpGet("GetApprovedProjects")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetApprovedProjects()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opject = await _projectsRepository.GetAllApprovedProjects();
            if (opject == null)
                return NotFound();
            return Ok(opject);
        }


        [HttpGet("GetRejectedProjects")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetRejectedProjects()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opject = await _projectsRepository.GetAllRejectedProjects();
            if (opject == null)
                return NotFound();
            return Ok(opject);
        }

        [HttpDelete("DeleteFiles")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest();
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            if (userId == null)
                return BadRequest();
            var opject = await _projectsRepository.GetFileById(e => e.Id == id);
            if (opject == null)
                return NotFound();

            var filePath = Path.Combine(opject.FilePath, opject.FileName);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Handle exception if necessary
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete file.");
                }
            }

          //  var projectEventOpject = await _projectsRepository.GetProjectEventById(q => q.Id == id);
            if (opject.UserId == userId)
            {
                await _projectsRepository.Remove(opject);
               // await _projectsRepository.Remove(projectEventOpject);

            }
            return NoContent();

        }

        [HttpGet("GetFileById")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ProjectDto>> GetFileById(int id)
        {
            if (id == 0)
                return BadRequest();

            var opject = await _projectsRepository.GetFileById(e => e.Id == id );
            if (opject == null)
                return NotFound();
            return Ok(opject);
        }

        [HttpGet("GetAllFiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDto>> GetAllFiles()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var opject = await _projectsRepository.GetAllProjects();
            if (opject == null)
                return NotFound();
            return Ok(opject);
        }

        [HttpPost("like/id:int")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Like(int id)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;

            var Project = await _projectsRepository.Get(e => e.Id == id);
            if (Project == null)
                return NotFound();

            var NewProjectEvent = await _projectsRepository.GetSpecialEntity<ProjectEvent>
                (e => e.projectId == id && e.userid == userId && e.typeEvent == "Like");
            if (NewProjectEvent != null)
            {
                DeleteLike(id);
                return NoContent();
            }
            ProjectEvent ProjectEvent = new()
            {
                projectId = id,
                typeEvent = "like",
                userid = userIdClaim.Value,
                eventDate = DateTime.UtcNow,
                text = null
            };

            var opject = await _projectsRepository.AddProjectEvent(ProjectEvent);
            if (opject == null)
                return NotFound();
            Project.totallike++;
            await _projectsRepository.save();
            return Ok();

        }
        [HttpPost("dislike/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDto>> dislike(int id)

        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;

            var post = await _projectsRepository.Get(e => e.Id == id);

            if (post == null)
                return NotFound();

            var NewProjectEvent = await _projectsRepository.GetSpecialEntity<ProjectEvent>
               (e => e.projectId == id && e.userid == userId && e.typeEvent == "DisLike");
            if (NewProjectEvent != null)
            {
                DeleteDislike(id);
                return NoContent();
            }

            ProjectEvent ProjectEvent = new()
            {
                projectId = id,
                typeEvent = "DisLike",
                userid = userIdClaim.Value,
                eventDate = DateTime.UtcNow,
                text = null
            };

            var opject = await _projectsRepository.AddProjectEvent(ProjectEvent);
            if (opject == null)
                return NotFound();
            post.totaldislike++;
            await _projectsRepository.save();
            return NoContent();

        }

        [HttpDelete("DeleteLike/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectEvent>> DeleteLike(int projectid)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            var Project = await _projectsRepository.Get(e => e.Id == projectid);

            var existingLike = await _projectsRepository.GetSpecialEntity<ProjectEvent>(e => e.projectId == projectid && e.userid == userId && e.typeEvent == "like");

            if (projectid == 0 || userId == null)
                return BadRequest();
            if (existingLike == null)
                return NotFound();
            await _projectsRepository.Remove(existingLike);
            Project.totallike--;
            await _projectsRepository.save();
            return NoContent();

        }

        [HttpGet("Getlikes/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectEventDto>> Getliskss(int id)

        {
            if (id == null)
                return BadRequest();


            var Project = await _projectsRepository.GetAllTEntity<ProjectEvent>(e => e.typeEvent == "like" && e.projectId == id);



            if (Project == null)
                return NotFound();


            return Ok(Project);



        }

        [HttpDelete("DeleteDisLike/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDto>> DeleteDislike(int projectid)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            var DisLikeOpject = await _projectsRepository.GetSpecialEntity<ProjectEvent>(e => e.projectId == projectid && e.userid == userId && e.typeEvent == "DisLike");
            var Projectopject = await _projectsRepository.Get(q => q.Id == projectid);
            if (projectid == 0 || userId == null)
                return BadRequest();

            if (DisLikeOpject == null)
                return NotFound();

            await _projectsRepository.Remove(DisLikeOpject);
            Projectopject.totaldislike--;
            await _projectsRepository.save();
            return NoContent();

        }

        [HttpGet("Getdislikes/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectEvent>> GetDisLikes(int id)
        {
            if (id == null)
                return BadRequest();
            var opject = await _projectsRepository.GetAllTEntity<ProjectEvent>(e => e.typeEvent == "dislike" && e.projectId == id);
            if (opject == null)
                return NotFound();
            return Ok(opject);
        }

        [HttpPost("comment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectEventDto>> Comment(int id, string Comment)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            var project = await _projectsRepository.Get(e => e.Id == id);

            if (project == null)
                return NoContent();


            ProjectEvent ProjectEvent = new()
            {
                projectId = id,
                typeEvent = "Comment",
                userid = userIdClaim.Value,
                eventDate = DateTime.UtcNow,
                text = Comment
            };

            var opject = await _projectsRepository.AddProjectEvent(ProjectEvent);
            if (opject == null)
                return NotFound();
            project.totalcomment++;
            await _projectsRepository.save();
            return NoContent();
        }

        [HttpDelete("comment/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<post>> Deletecomment(int projectid)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            var existingcomment = await _projectsRepository.GetSpecialEntity<ProjectEvent>(e => e.projectId == projectid && e.userid == userId && e.typeEvent == "comment");

            if (projectid == 0 || userId == null)
                return BadRequest();

            if (existingcomment == null)
                return NotFound();

            // await _projectsRepository.RemoveProjectEvent(existingcomment);
            await _projectsRepository.Remove(existingcomment);


            await _projectsRepository.save();
            return NoContent();

        }

        [HttpGet("Getcomment/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<post>> Getcomment(int id)

        {
            if (id == null)
                return BadRequest();

            var comment = await _projectsRepository.GetAllTEntity<ProjectEvent>(e => e.typeEvent == "comment" && e.projectId == id);


            {
                if (comment == null) return NotFound();

                else
                {

                    return Ok(comment);
                }
            }

        }



        [HttpPost("Bookmark/id ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<post>> Bookmark(int id)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;

            var Project = await _projectsRepository.Get(e => e.Id == id);

            if (Project == null)
                return NotFound();

            var NewProjectEvent = await _projectsRepository.GetSpecialEntity<ProjectEvent>
               (e => e.projectId == id && e.userid == userId && e.typeEvent == "Bookmark");
            if (NewProjectEvent != null)
            {
                DeleteDislike(id);
                return NoContent();
            }

            ProjectEvent ProjectEvent = new()
            {
                projectId = id,
                typeEvent = "Bookmark",
                userid = userIdClaim.Value,
                eventDate = DateTime.UtcNow,
                text = null
            };

            var opject = await _projectsRepository.AddProjectEvent(ProjectEvent);
            if (opject == null)
                return NotFound();
            
            
            return NoContent();

        }




        [HttpPut("unBookmark/projectid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<evnet>> unBookmark(int projectid)
        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var userId = userIdClaim.Value;
            var Project = await _projectsRepository.Get(e => e.Id == projectid);

            var existingLike = await _projectsRepository.GetSpecialEntity<ProjectEvent>(e => e.projectId == projectid && e.userid == userId && e.typeEvent == "Bookmark");

            if (projectid == 0 || userId == null)
                return BadRequest();
            if (existingLike == null)
                return NotFound();
            await _projectsRepository.Remove<ProjectEvent>(existingLike);
          
            
            return NoContent();

        }

        [HttpGet("GetBookmark/id:int ")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<post>> GetBookmark()

        {
            var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
            var id = userIdClaim.Value;
           


            var Project = await _projectsRepository.GetAllTEntity<ProjectEvent>(e => e.typeEvent == "Bookmark" && e.userid == id);



            if (Project == null)
                return NotFound();


            return Ok(Project);



        }
       
       

        

        //[HttpGet("GetProjectByUserId")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult> GetProjectByUserId()
        //{
        //    var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
        //    var userId = userIdClaim.Value;

        //    // var projects = await _appDbcontext1..Where(p => p.UserId == userId).ToListAsync();
        //    var projects = await _projectsRepository.GetAllTEntity<Projects>(p => p.UserId == userId);

        //    if (projects == null)
        //        return NotFound();
        //    return Ok(projects);
        //}

        //[HttpPut("id:int ")]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //public async Task<ActionResult<ProjectUpdateDto>> update(int id, [FromBody] ProjectUpdateDto updateDto)
        //{
        //    var userIdClaim = (HttpContext.User.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "uid");
        //    var userId = userIdClaim.Value;
        //    var opject = await _projectsRepository.GetSpecialEntity<Projects>(e => e.Id == id);
        //    var opject = await _appDbcontext1.projects.AnyAsync(p => p.Id == id);
        //    if (!opject)
        //        return BadRequest();
        //    if (opject == null)
        //        return BadRequest();
        //    if (updateDto == null)
        //        return BadRequest();

        //    _projectsRepository.Detach(opject); // Detach the entity
        //    Projects model = new()
        //    {
        //        Id = id,
        //        FileName = updateDto.Name,
        //        UserId = userId,
        //    };


        //    await _projectsRepository.UpdateProject(model);
        //    return NoContent();

        //}

    }
}
    