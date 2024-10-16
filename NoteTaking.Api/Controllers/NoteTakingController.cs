using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoteTaking.Api.Context;
using NoteTaking.Api.Interfaces;
using NoteTaking.Api.Mappings;
using NoteTaking.Api.Middleware;
using NoteTaking.Api.Model;
using NoteTaking.Api.Services;

namespace NoteTaking.Api.Controllers
{
    //[OutputCache]
    [Route("api/[controller]")]
    [ApiController]
    public class NoteTakingController : ControllerBase
    {
        
        private readonly INoteTakingServices _notetakingServices;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EmailVerificationLinkFactory _emailVerificationLinkFactory;

        public NoteTakingController(INoteTakingServices noteTakingServices, IPasswordHasher passwordHasher, IHttpClientFactory httpClientFactory, EmailVerificationLinkFactory emailVerificationLinkFactory)
        {
            _notetakingServices = noteTakingServices;
            _passwordHasher = passwordHasher;
            _httpClientFactory = httpClientFactory;
            _emailVerificationLinkFactory = emailVerificationLinkFactory;
        }

        /// <Summary>
        /// Genertes Token to Authenticate a User trying to perform an action   
        /// </Summary>
        /// <param name="item"></param>
        /// <returns> An authentication token for user.
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "Username":"User123",
        ///     "Password": "Password123"
        /// }
        /// </remarks>

        //[HttpPost("AuthenticateLogin")]
        //public async Task<ActionResult> AuthenticateUser()
        //{
        //    try
        //    {
        //        var token = await  _notetakingServices.AuthenticateUser();
        //       return Ok(new Response(){
        //                ResponseCode = "00",
        //                ResponseDescription = "User Authentication Successful.",
        //                errorResponse = null,
        //                ResponseData = token
        //        });
        //    }
        //    catch(Exception ex)
        //    {
        //        return Ok
        //        (
        //            new Response(){
        //                ResponseCode = "90",
        //                ResponseDescription = $"User Authentication Failed.",
        //                errorResponse = new ErrorResponse(){Title = "An error occurred.", ErrorMessage = ex.Message},
        //                ResponseData = null
        //        });
        //    }
        //}


        [HttpGet("verifyEmail/{token}")]
        public async Task VerifyUser(Guid token)
        {
            var VerifyResponse = await _emailVerificationLinkFactory.VerifyEmail(token);
            if (!VerifyResponse.IsSuccessful)
            {
                switch(VerifyResponse.ResponseCode)
                {
                    case "90":
                        Results.BadRequest(new Response(){ResponseCode = VerifyResponse.ResponseCode, errorResponse = null, ResponseData = null, ResponseDescription = $"User Verification Failed: Invalid Token Supplied"});
                        break;
                    case "99":
                        Results.BadRequest(new Response(){ResponseCode = VerifyResponse.ResponseCode, errorResponse = null, ResponseData = null, ResponseDescription = $"User Verification Failed: Token Already Expired."});
                        break;
                        
                    default:
                        Results.BadRequest(new Response(){ResponseCode = VerifyResponse.ResponseCode, errorResponse = null, ResponseData = null, ResponseDescription = $"User Verification Failed"});
                        break;
                }
            }

            Results.Ok(new Response(){ResponseCode = "00", errorResponse = null, ResponseData = null, ResponseDescription = "User Verification Successful."});
        }

        [HttpPost("/login")]
        public async Task<ActionResult> GetTokenService(TokenUser user)
        {
                
            try
            {
                var httpClient = _httpClientFactory.CreateClient("AuthenticateUserClient");

                var serializedUser = JsonConvert.SerializeObject(user);
                var stringContent = new StringContent(serializedUser, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("", stringContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                return Ok(responseContent);
            }
            catch(Exception ex)
            {
                return Ok(ex.Message);
            }
            
        }

        [HttpDelete("DeleteNote")]
        public async Task<ActionResult> DeleteNote(string UserID, int NoteID)
        {
            Console.WriteLine();
            
            try{
                await _notetakingServices.DeleteNote(NoteID, UserID);

                return Ok(
                        new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "Note deleted succssfully.",
                    ResponseData = null,
                    errorResponse = null
                });
            }

            catch(ArgumentNullException ex)
            {
                return Ok(
                  new Response(){
                    ResponseCode = "90",
                    ResponseDescription = $"Note with Id: {NoteID} not found.",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "Note not found.", ErrorMessage = $"Could not delete note with Id: {NoteID} due to {ex.Message}"}
                  }  
                );
            }

            catch(Exception ex)
            {
                return Ok(
                  new Response(){
                    ResponseCode = "99",
                    ResponseDescription = $"Could not delete note with Id: {NoteID}",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = ex.Source, ErrorMessage = $"Could not delete note with Id: {NoteID} due to {ex.Message}"}
                  }  
                );
            }
        }


        [HttpDelete("DeleteUser/{userID}")]
        public async Task<ActionResult> DeleteUser(string userID)
        {
             Console.WriteLine();
            try
            {
                await _notetakingServices.DeleteUser(userID);

                return Ok(
                        new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "User deleted succssfully.",
                    ResponseData = null,
                    errorResponse = null
                });
            }
            
            catch(ArgumentNullException ex)
            {
                return Ok(
                  new Response(){
                    ResponseCode = "90",
                    ResponseDescription = $"User with Id: {userID} not found.",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "User not found", ErrorMessage = $"Could not delete user with Id: {userID} due to: {ex.Message}"}
                  }  
                );
            }

            catch(Exception ex)
            {
                return Ok(
                  new Response(){
                    ResponseCode = "99",
                    ResponseDescription = $"Could not delete puser with Id: {userID}",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = ex.Source, ErrorMessage = $"Could not delete user with Id: {userID} due to: {ex.Message}"}
                  }  
                );
            }
        }

        [HttpGet("GetUserNote")]
        public async Task<ActionResult> GetNote(int NoteID, string UserID)
        {
            try
            {
                var note = await _notetakingServices.GetNote(UserID, NoteID);
                
                Console.WriteLine();

                var response = new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "Note fetched succssfully.",
                    ResponseData = note,
                    errorResponse = null
                };

                return Ok(response);
            }
            catch(ArgumentNullException ex)
            {
                return Ok(
                  new Response(){
                    ResponseCode = "90",
                    ResponseDescription = $"No note found with id: {NoteID}",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "No Note Found", ErrorMessage = $"No note with id: {NoteID} for user: {UserID} due to: {ex.Message}"}
                  }  
                );
            }
            catch(Exception ex)
            {
                return Ok(
                  new Response(){
                    ResponseCode = "99",
                    ResponseDescription = $"Error fetching Note.",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "An error occurred.", ErrorMessage = $"No note with id: {NoteID} for user: {UserID} due to: {ex.Message}"}
                  }  
                );
            }
            
        }

        [HttpGet("GetNoteById/{Id}")]
        public async Task<Note> GetNoteById(int Id)
        {
            var note = await _notetakingServices.GetNoteById(Id);

            return note;
        }

        [HttpPut("UpdateNote")]
        public async Task<ActionResult> UpdateNote(int NoteID, UpdateNoteDTO UpdatedNote)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(UpdatedNote);
            }

            try
            {
                await _notetakingServices.UpdateNote(NoteID, UpdatedNote);
                return Accepted(new Response(){
                        ResponseCode = "00",
                        ResponseDescription = $"Note with Id: {NoteID} updated successfully.",
                        errorResponse = null,
                        ResponseData = UpdatedNote
                });
            }

            catch(ArgumentNullException ex)
            {   
                return StatusCode(Response.StatusCode, new Response(){
                    ResponseCode = "90",
                    ResponseData = null,
                    ResponseDescription = $"No note with Id: {NoteID}",
                    errorResponse = new ErrorResponse(){Title = "Note not found.", ErrorMessage = ex.Message}
                });
            }

            catch(Exception ex)
            {
                return StatusCode(Response.StatusCode, new Response(){
                    ResponseCode = "99",
                    ResponseData = null,
                    ResponseDescription = "Error Updating Record",
                    errorResponse = new ErrorResponse(){Title = "An error occurred.", ErrorMessage = ex.Message}
                });
            }
        }

        [Authorize]
        [HttpGet("FetchAllNotes")]
        public async Task<ActionResult> FetchNotes()
        {
            try
            {
                var Notes = await _notetakingServices.FetchNotes();

                return Ok(
                    new Response(){
                        ResponseCode = "00",
                        ResponseDescription = "Notes Fetched Successfully.",
                        ResponseData = Notes,
                        errorResponse = null
                    }
                );
            }
            catch(Exception ex)
            {
                return Ok(
                    new Response(){
                    ResponseCode = "99",
                    ResponseData = null,
                    ResponseDescription = "An error occcurrd fetching Notes",
                    errorResponse = new ErrorResponse(){Title = "An error occurred.", ErrorMessage = ex.Message}
                });
            }
        }

        //[Authorize]
        [HttpGet("FetchAllUsers")]
        public async Task<ActionResult> FetchUsers()
        {
            try
            {
                var Users = await _notetakingServices.FetchUsers();
                return Ok
                (
                    new Response(){
                        ResponseCode = "00",
                        ResponseDescription = "Users Fetched Successfully.",
                        ResponseData = Users,
                        errorResponse = null
                    }
                );
            }
            catch(Exception ex)
            {
                return Ok(
                    new Response(){
                    ResponseCode = "99",
                    ResponseData = null,
                    ResponseDescription = "An error occcurrd fetching Users",
                    errorResponse = new ErrorResponse(){Title = "An error occurred.", ErrorMessage = ex.Message}
                });
            }
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult> GetAllUsers()
        {
            var AllUsers = await _notetakingServices.GetAllUsers();

            if(AllUsers.Count == 0)
            {
                return Ok
                (
                    new Response(){
                        ResponseCode = "99",
                        ResponseDescription = "No users found",
                        ResponseData = AllUsers,
                        errorResponse = new ErrorResponse(){ErrorMessage = "No users found", Title = "No Users"}
                    }
                );
            }

            var response = new Response()
            {
                ResponseCode = "00",
                ResponseDescription = "Users fetched successfully.",
                ResponseData = AllUsers,
                errorResponse = null
            };

            return Ok(response);
        }
        
        [HttpGet("GetUserNotes")]
        public async Task<ActionResult> GetUserNotes(string UserId)
        {
            var notes = await _notetakingServices.GetUserNotes(UserId);
            var response = new Response()
            {
                ResponseCode = "00",
                ResponseDescription = "Notes fetched successfully.",
                ResponseData = notes,
                errorResponse = null
            };

            return Ok(response);

        }
        

        [HttpPost("CreateNote")]
        public async Task<ActionResult> CreateNote(CreateNoteDTO NewNote)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _notetakingServices.CreateNote(NewNote);

                var response = new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "Note Created Successully",
                    ResponseData = NewNote.ToNoteEntity(),
                    errorResponse = null
                };
                return Ok(response);
            }

            catch(Exception ex)
            {
                var response = new Response()
                {
                    ResponseCode = Response.StatusCode.ToString(),
                    ResponseDescription = "Error Creating Record",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = ex.Source, ErrorMessage = ex.Message}
                };
                return StatusCode(Response.StatusCode, response);
            }


        }

        [HttpGet("GetUser/{UserID}")]
        public async Task<ActionResult> GetUser(string UserID)
        {
            try
            {
                var user = await _notetakingServices.GetUser(UserID);
                Console.WriteLine();

                var response = new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "User Fetched Successfully.",
                    ResponseData = user,
                    errorResponse = null
                };
                return Ok(response);
            }
            catch(ArgumentNullException ex)
            {
                return NotFound(new Response()
                    {
                        ResponseCode = "99", 
                        ResponseDescription = $"No user found with specified id: {UserID}",
                        ResponseData = null,
                        errorResponse = new ErrorResponse(){
                            Title = "User Not Found",
                            ErrorMessage = ex.Message
                        }
                    });
            }
            catch(Exception ex)
            {
                var response = new Response()
                {
                    ResponseCode = Response.StatusCode.ToString(),
                    ResponseDescription = "Error Fetching Record",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "Error Fetching User", ErrorMessage = ex.Message}
                };
                
                return StatusCode(Response.StatusCode, response);
            }
        }

        [HttpPost("LoginUser")]
        public async Task<ActionResult> LoginUser(CreateUserDTO LoginUser)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _notetakingServices.LoginUser(LoginUser);
                var response = new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "User login Successful.",
                    ResponseData = user,
                    errorResponse = null
                };
                return Ok(response);
            }
            catch (NotImplementedException ex)
            {
                var response = new Response()
                {
                    ResponseCode = "99",
                    ResponseDescription = "User Activation Required",
                    ResponseData = ex.Message,
                    errorResponse = new ErrorResponse(){Title = "User Login Failed.", ErrorMessage = "Activate with the provided link."}
                };
                return Ok(response);
            }
            catch(ArgumentNullException ex)
            {
                var response = new Response()
                {
                    ResponseCode = "99",
                    ResponseDescription = "Incorrect password or Username.",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "User Login Failed.", ErrorMessage = ex.Message}
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                var response = new Response()
                {
                    ResponseCode = "90",
                    ResponseDescription = "Error logging user in.",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = "User Login Failed.", ErrorMessage = ex.Message}
                };
                return Ok(response);
            }
        }
        
        
        [HttpPost("CreateUser")]
        public async Task<ActionResult> CreateUser(CreateUserDTO NewUser)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var CreatedUser = await _notetakingServices.CreateUser(NewUser);

                //var VerificationLink = await _notetakingServices.ActivateUser(CreatedUser);
                

                var response = new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = $"User Created Successully. Pending Verifiaction: {CreatedUser.Item2}",
                    ResponseData = CreatedUser.Item1.ToUserDTO(),
                    errorResponse = null
                };
                return Ok(response);
            }
            catch(InvalidOperationException ex)
            {
                 var response = new Response()
                {
                    ResponseCode = Response.StatusCode.ToString(),
                    ResponseDescription = "Error Creating Record",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = ex.Source, ErrorMessage = ex.Message}
                };
                return StatusCode(Response.StatusCode, response);   
            }
            catch (Exception ex)
            {
                var response = new Response()
                {
                    ResponseCode = Response.StatusCode.ToString(),
                    ResponseDescription = "Error Creating Record",
                    ResponseData = null,
                    errorResponse = new ErrorResponse(){Title = ex.Source, ErrorMessage = ex.Message}
                };
                return StatusCode(Response.StatusCode, response);
            }
        }
    
        [HttpDelete("RemoveUser")]
        public async Task<ActionResult> RemoveUser(string UserId)
        {
            try
            {
                await _notetakingServices.RemoveDeletedUser(UserId);
                return Ok("User deleted Successfully.");

            }
            catch(ArgumentNullException ex)
            {
                return NotFound($"No User fonud with  specified Id: {UserId}: {ex.Message}");
            }
            catch(ArgumentException ex)
            {
                return NotFound($"The specified User is not yet deleted: {ex.Message}");
            }
            catch(DbUpdateException ex)
            {
                return BadRequest($"{ex.Message}");
            }
            catch(Exception ex)
            {
                //return Problem(StatusCode: 200, );
                
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserSummary")]
        public async Task<ActionResult> GetUserSummary()
        {
            try
            {
                var UsersSummary = await _notetakingServices.GetUserSummary();

                var response = new Response()
                {
                    ResponseCode = "00",
                    ResponseDescription = "User Summary Fetched",
                    ResponseData = UsersSummary,
                    errorResponse = null
                };
                return Ok(response);
            }
            catch(Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("RemoveNote")]
        public async Task<ActionResult> RemoveNote(int Id, string UserID)
        {
            try
            {
                await _notetakingServices.RemoveDeletedNote(Id, UserID);
                return Ok("Note deleted Successfully.");
            }

            catch(ArgumentNullException ex)
            {
                return NotFound($"No Note fonud with  specified Id: {Id}: {ex.Message}");
            }
            catch(ArgumentException ex)
            {
                return NotFound($"The specified Note is not yet deleted: {ex.Message}");
            }
            catch(DbUpdateException ex)
            {
                return BadRequest($"{ex.Message}");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);

            }

        }
    
    }


}
